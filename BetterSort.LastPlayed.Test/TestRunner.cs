namespace Nanikit.Test {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using IPALogger = IPA.Logging.Logger;
  using Zenject;
  using System.Threading.Tasks;

  public class TestRunner {
    private readonly IPALogger? _logger;

    public TestRunner(IPALogger? logger = null) {
      _logger = logger;
    }

    public void Test(IEnumerable<Assembly> targets) {
      var testMethods = targets.SelectMany(GetTests).ToList();
      _logger?.Info($"{testMethods.Count} tests found. Test start.");
      RunTestsWithConsoleOutput(testMethods);
    }

    private void RunTestsWithConsoleOutput(IEnumerable<MethodInfo> testMethods) {
      int success = 0;
      int total = 0;
      foreach (var result in RunTests(testMethods)) {
        string? typeName = result.Method.DeclaringType.Name;
        string? methodName = result.Method.Name;
        if (result.Exception == null) {
          _logger?.Info($"PASS: {typeName}.{methodName}");
          success++;
        }
        else {
          _logger?.Error($"\nFAIL: {typeName}.{methodName}\n{result.Exception}");
        }
        total++;
      }
      _logger?.Notice($"Test finished. {success}/{total} tests passed");
    }

    private IEnumerable<TestResult> RunTests(IEnumerable<MethodInfo> tests) {
      object[]? parameters = new object[] { };

      var container = new DiContainer();
      if (_logger != null) {
        container.Bind<IPALogger>().FromInstance(_logger).AsSingle();
      }

      object Resolve(Type type) {
        if (!container.HasBinding(type)) {
          container.BindInterfacesAndSelfTo(type).AsTransient();
        }
        return container.Resolve(type);
      }

      foreach (var method in tests) {
        Exception? exception = null;
        try {
          object? instance = Resolve(method.DeclaringType);

          bool isAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
          if (isAwaitable) {
            ((Task)method.Invoke(instance, parameters)).Wait();
          }
          else {
            method.Invoke(instance, parameters);
          }
        }
        catch (Exception thrown) {
          exception = thrown;
        }
        yield return new TestResult(method, exception?.InnerException ?? exception);
      }
    }

    private static List<MethodInfo> GetTests(Assembly targetAssembly) {
      var testAttribute = typeof(Test);
      var testMethods = new List<MethodInfo>();
      foreach (var type in targetAssembly.GetTypes()) {
        foreach (var method in type.GetMethods()) {
          if (Attribute.IsDefined(method, testAttribute)) {
            testMethods.Add(method);
          }
        }
      }
      return testMethods;
    }
  }

  public class TestResult {
    public MethodInfo Method { get; set; }
    public Exception? Exception { get; set; }

    public TestResult(MethodInfo method, Exception? exception) {
      Method = method;
      Exception = exception;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class Test : Attribute { }
}
