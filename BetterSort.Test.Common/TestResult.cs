namespace BetterSort.Test.Common {
  using System;
  using System.Reflection;

  public class TestResult {
    public MethodInfo Method { get; set; }
    public Exception? Exception { get; set; }

    public TestResult(MethodInfo method, Exception? exception) {
      Method = method;
      Exception = exception;
    }
  }
}
