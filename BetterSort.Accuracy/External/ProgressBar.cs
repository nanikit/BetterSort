using BS_Utils;
using BS_Utils.Utilities;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace BetterSort.Accuracy.External {
  public record ProgressMessage(
    string Title = "",
    string Body = "",
    string Progress = "",
    float? Ratio = null,
    TimeSpan? Time = null
  );

  public interface IProgressBar {

    Task SetMessage(ProgressMessage? message);
  }

  internal record TextProperties(Vector2 Position, Vector2 Size, float FontSizeMax);

  public class ProgressBar(SiraLog logger) : MonoBehaviour, IProgressBar {
    private static readonly Vector3 Position = new(-1.25f, 0.15f, 3);
    private static readonly Vector3 Rotation = new(80, 336, 0);
    private static readonly Vector3 Scale = new(0.01f, 0.01f, 0.01f);
    private static readonly Vector2 CanvasSize = new(100, 50);
    private static readonly Vector2 LoadingBarSize = new(100, 16);
    private static readonly Vector2 LoadingBarPosition = new(0, (-CanvasSize.y + LoadingBarSize.y) / 2f);
    private static readonly TextProperties Title = new(new(0, (CanvasSize.y - 10) / 2f), new(CanvasSize.x, 10), 10);
    private static readonly TextProperties Progress = new(new(0, LoadingBarPosition.y), new(CanvasSize.x, LoadingBarSize.y), LoadingBarSize.y);
    private static readonly TextProperties Body = new(new(0, (Title.Position.y + Progress.Position.y) / 2), new(CanvasSize.x, CanvasSize.y - Title.Size.y - Progress.Size.y), 9);
    private static readonly Color BackgroundColor = new(0, 0, 0, 0.2f);

    [Inject]
    private readonly SiraLog _logger = logger;

    private Canvas? _canvas;
    private TMP_Text? _progressText;
    private TMP_Text? _bodyText;
    private TMP_Text? _titleText;
    private Image? _loadingBackground;
    private Image? _loadingBar;

    private bool _hasMessage;
    private bool _isMenuScene;

    public Task SetMessage(ProgressMessage? message) {
      return UnityMainThreadTaskScheduler.Factory.StartNew(() => UpdateUi(message));
    }

    private static bool IsMenuScene(Scene scene) {
      return scene.name == SceneNames.Menu;
    }

    private static TextMeshProUGUI CreateText(RectTransform parent, TextProperties props) {
      var gameObject = new GameObject("CustomUIText");
      var text = gameObject.AddComponent<TextMeshProUGUI>();
      text.rectTransform.SetParent(parent, worldPositionStays: false);
      text.fontSize = 4f;
      text.color = Color.white;
      text.rectTransform.sizeDelta = props.Size;
      text.rectTransform.anchoredPosition = props.Position;

      text.fontSizeMax = props.FontSizeMax;
      text.fontSizeMin = 4f;
      text.enableAutoSizing = true;
      text.alignment = TextAlignmentOptions.Center;
      return text;
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private void Awake() {
      try {
        gameObject.transform.position = Position;
        gameObject.transform.eulerAngles = Rotation;
        gameObject.transform.localScale = Scale;

        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        var rectTransform = _canvas.transform as RectTransform;
        rectTransform!.sizeDelta = CanvasSize;

        _isMenuScene = IsMenuScene(SceneManager.GetActiveScene());
        UpdateVisibility();

        DontDestroyOnLoad(gameObject);

        BSEvents.lateMenuSceneLoadedFresh += (_) => {
          try {
            _logger.Info("Initializing...");
            Initialize();
          }
          catch (Exception exception) {
            _logger.Error(exception);
          }
        };
      }
      catch (Exception exception) {
        _logger.Error(exception);
      }
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private void OnEnable() {
      SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    private void OnDisable() {
      SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
    }

    private void UpdateUi(ProgressMessage? message) {
      StopAllCoroutines();

      if (message is null) {
        _hasMessage = false;
        UpdateVisibility();
        return;
      }

      _hasMessage = true;

      var (title, body, progress, ratio, time) = message;
      if (_titleText != null) {
        _titleText.text = title;
      }
      if (_bodyText != null) {
        _bodyText.text = body;
      }
      if (_progressText != null) {
        _progressText.text = progress;
      }
      if (_loadingBar != null) {
        if (ratio is float r) {
          _loadingBar.enabled = true;
          _loadingBar.fillAmount = r;
        }
        else {
          _loadingBar.enabled = false;
        }
      }
      if (_loadingBackground != null) {
        _loadingBackground.enabled = ratio is float;
      }
      if (time is TimeSpan span) {
        StartCoroutine(DisableCanvasRoutine((float)span.TotalSeconds));
      }

      UpdateVisibility();
    }

    private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene) {
      _isMenuScene = IsMenuScene(newScene);

      UpdateVisibility();
    }

    private IEnumerator DisableCanvasRoutine(float seconds) {
      yield return new WaitForSecondsRealtime(seconds);
      _hasMessage = false;
      UpdateVisibility();
    }

    private void UpdateVisibility() {
      bool shouldShow = _isMenuScene && _hasMessage;
      SetVisibility(shouldShow);
      _logger.Info($"shouldShow: {shouldShow}, _isMenuScene: {_isMenuScene}, _hasMessage: {_hasMessage}");
    }

    private void SetVisibility(bool value) {
      if (_canvas != null) {
        _canvas.enabled = value;
      }
    }

    private void Initialize() {
      if (_canvas == null) {
        _logger.Warn("Can't show progress because _canvas is null.");
        return;
      }

      var parent = _canvas.transform as RectTransform;
      if (parent == null) {
        _logger.Warn("Can't show progress because parent is null.");
        return;
      }

      _loadingBackground = new GameObject("Background").AddComponent<Image>();
      var rectTransform = _loadingBackground.transform as RectTransform;
      rectTransform!.SetParent(parent, false);
      rectTransform.sizeDelta = LoadingBarSize;
      rectTransform.anchoredPosition = LoadingBarPosition;
      _loadingBackground.color = BackgroundColor;

      _loadingBar = new GameObject("Loading Bar").AddComponent<Image>();
      rectTransform = _loadingBar.transform as RectTransform;
      rectTransform!.SetParent(parent, false);
      rectTransform.sizeDelta = LoadingBarSize;
      rectTransform.anchoredPosition = LoadingBarPosition;
      var texture = Texture2D.whiteTexture;
      var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100, 1);
      _loadingBar.sprite = sprite;
      _loadingBar.type = Image.Type.Filled;
      _loadingBar.fillMethod = Image.FillMethod.Horizontal;
      _loadingBar.color = new Color(1, 1, 1, 0.2f);

      _titleText = CreateText(parent, Title);
      _bodyText = CreateText(parent, Body);
      _progressText = CreateText(parent, Progress);
      _progressText.color = new Color(0.6f, 0.8f, 1);
    }
  }
}
