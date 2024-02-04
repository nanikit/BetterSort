using System;
using System.Reflection;

namespace BetterSort.Test.Common {

  public class TestResult {

    public TestResult(MethodInfo method, Exception? exception) {
      Method = method;
      Exception = exception;
    }

    public Exception? Exception { get; set; }
    public MethodInfo Method { get; set; }
  }
}
