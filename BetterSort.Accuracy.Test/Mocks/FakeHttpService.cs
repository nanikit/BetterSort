using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSort.Accuracy.Test.Mocks {

  public class FakeHttpService : IHttpService {
    private readonly HttpClient _client = new();
    private readonly SiraLog _logger;

    public FakeHttpService(SiraLog logger) {
      _logger = logger;
    }

    public string? BaseURL {
      get => null; set { }
    }

    public IDictionary<string, string> Headers => throw new NotImplementedException();
    public string? Token { set { } }

    public string? UserAgent {
      get => null; set { }
    }

    public Task<IHttpResponse> DeleteAsync(string url, CancellationToken? cancellationToken = null) {
      throw new NotImplementedException();
    }

    public async Task<IHttpResponse> GetAsync(string url, IProgress<float>? progress = null, CancellationToken? cancellationToken = null) {
      string hash = GetHashString(url);
      string path = $"{hash}.json";
      if (File.Exists(path)) {
        string content = File.ReadAllText(path);
        return new MockHttpResponse() {
          Code = 200,
          Response = content,
          Successful = true,
        };
      }
      else {
        _logger.Debug($"Download {url}");
        var response = await _client.GetAsync(url, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        File.WriteAllText(path, json);
        return new MockHttpResponse() {
          Code = (int)response.StatusCode,
          Response = json,
          Successful = true,
        };
      }
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

    private static byte[] GetHash(string inputString) {
      using HashAlgorithm algorithm = SHA256.Create();
      return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static string GetHashString(string inputString) {
      var sb = new StringBuilder();
      foreach (byte b in GetHash(inputString))
        sb.Append(b.ToString("x2"));

      return sb.ToString();
    }
  }

  public class MockHttpResponse : IHttpResponse {
    public int Code { get; set; }

    public string? ErrorString { get; set; }
    public string? Response { get; set; }
    public bool Successful { get; set; }

    public Dictionary<string, string> Headers => throw new NotImplementedException();

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
