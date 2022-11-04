namespace BetterSort.Accuracy.Test {
  using BetterSort.Accuracy.External;
  using BetterSort.Accuracy.Sorter;
  using BetterSort.Test.Common.Mocks;
  using SiraUtil.Web;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Net.Http;
  using System.Threading;
  using System.Threading.Tasks;
  using Xunit;
  using Xunit.Abstractions;
  using Zenject;
  using IPALogger = IPA.Logging.Logger;

  public class LeaderboardImportTest {
    private readonly IPALogger _logger;
    private readonly DiContainer _container;
    private readonly ScoresaberImporter _scoresaber;
    private readonly BeatLeaderImporter _beatLeader;

    public LeaderboardImportTest(ITestOutputHelper output) {
      _logger = new MockLogger(output);

      var container = new DiContainer();
      container.BindInterfacesAndSelfTo<IPALogger>().FromInstance(_logger).AsSingle();
      container.BindInterfacesAndSelfTo<FixedClock>().AsSingle();
      container.BindInterfacesAndSelfTo<PlainHttpService>().AsSingle();
      container.Install<AccuracyInstaller>();

      _scoresaber = container.Resolve<ScoresaberImporter>();
      _beatLeader = container.Resolve<BeatLeaderImporter>();
      _container = container;
    }

    [Fact]
    public async Task TestScoresaber() {
      var page = await _scoresaber.GetPagedRecord("76561198159100356", 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.NotEmpty(records);
      Assert.InRange(records[0].Accuracy, 0, int.MaxValue);
      Assert.InRange(maxPage, 1, int.MaxValue);
    }

    [Fact]
    public async Task TestBeatLeader() {
      var page = await _beatLeader.GetPagedRecord("76561198159100356", 1).ConfigureAwait(false);
      if (page is not (var records, var maxPage)) {
        Assert.Fail("Failed to get data");
        throw new Exception();
      }
      Assert.NotEmpty(records);
      Assert.InRange(records[0].Accuracy, 0, int.MaxValue);
      Assert.InRange(maxPage, 1, int.MaxValue);
    }
  }

  public class PlainHttpService : IHttpService {
    private readonly HttpClient _client = new();

    public string? Token { set { } }
    public string? BaseURL {
      get => null; set { }
    }
    public string? UserAgent {
      get => null; set { }
    }

    public IDictionary<string, string> Headers => throw new NotImplementedException();

    public Task<IHttpResponse> DeleteAsync(string url, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }

    public async Task<IHttpResponse> GetAsync(string url, IProgress<float>? progress = null, CancellationToken? cancellationToken = null) {
      var response = await _client.GetAsync(url, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
      return new MockHttpResponse() {
        Code = (int)response.StatusCode,
        Response = await response.Content.ReadAsStringAsync().ConfigureAwait(false),
        Successful = true,
      };
    }

    public Task<IHttpResponse> PatchAsync(string url, object? body = null, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }

    public Task<IHttpResponse> PostAsync(string url, object? body = null, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }

    public Task<IHttpResponse> PutAsync(string url, object? body = null, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }

    public Task<IHttpResponse> SendAsync(HTTPMethod method, string url, string? body = null, IDictionary<string, string>? withHeaders = null, IProgress<float>? downloadProgress = null, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }
  }

  public class MockHttpResponse : IHttpResponse {
    public int Code { get; set; }

    public bool Successful { get; set; }

    public string? ErrorString { get; set; }

    public string? Response { get; set; }

    public Task<string?> Error() {
      return Task.FromResult(ErrorString);
    }

    public Task<byte[]> ReadAsByteArrayAsync() {
      throw new NotImplementedException();
    }

    public Task<Stream> ReadAsStreamAsync() {
      throw new NotImplementedException();
    }

    public Task<string> ReadAsStringAsync() {
      return Task.FromResult(Response ?? "");
    }
  }
}
