namespace BetterSort.Test.Common {

  using System;
  using System.Reflection;

  public class TestResult {

    public TestResult(MethodInfo method, Exception? exception) {
      Method = method;
      Exception = exception;
    }

    public Exception? Exception { get; set; }
    public MethodInfo Method { get; set; }
  }
}
