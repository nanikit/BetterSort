using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace BetterSort.Test.Common {

  public class TestRunner {
    private readonly IPALogger? _logger;

    public TestRunner(IPALogger? logger = null) {
      _logger = logger;
    }

    public static IEnumerable<Type> GetLoadableTypes(Assembly assembly) {
      try {
        return assembly.GetTypes();
      }
      catch (ReflectionTypeLoadException e) {
        return e.Types.Where(t => t != null);
      }
    }

    public void Test(IEnumerable<Assembly> targets) {
      var testMethods = targets.SelectMany(GetTests).ToList();
      _logger?.Info($"{testMethods.Count} tests found. Test start.");
      RunTestsWithConsoleOutput(testMethods);
    }

    private List<MethodInfo> GetTests(Assembly targetAssembly) {
      var testAttribute = typeof(FactAttribute);
      var testAttribute2 = typeof(UnityFact);
      var testMethods = new List<MethodInfo>();
      foreach (var type in GetLoadableTypes(targetAssembly)) {
        if (type.Namespace?.StartsWith("Xunit") == true) {
          continue;
        }

        try {
          foreach (var method in type.GetMethods()) {
            if (Attribute.IsDefined(method, testAttribute) || Attribute.IsDefined(method, testAttribute2)) {
              testMethods.Add(method);
            }
          }
        }
        catch (FileNotFoundException) {
          // xUnit repacked type maybe
        }
      }
      return testMethods;
    }

    private IEnumerable<TestResult> RunTests(IEnumerable<MethodInfo> tests) {
      object[]? parameters = new object[] { };

      var container = new DiContainer();
      if (_logger != null) {
        container.Bind<IPALogger>().FromInstance(_logger).AsSingle();
        container.Bind<ITestOutputHelper>().FromInstance(new XUnitLogger()).AsSingle();
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
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class UnityFact : Attribute { }
}
