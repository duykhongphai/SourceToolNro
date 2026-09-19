using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DrawMap.Classes;

public class FastHttpClient : IDisposable
{
    private static readonly ConcurrentDictionary<string, IPAddress> DnsCache = new();
    private readonly List<HttpClient> _httpClients;
    private bool _disposed;

    public FastHttpClient()
    {
        _httpClients = new List<HttpClient>
        {
            CreateFastHttpClient(),
            CreateBypassHttpClient(),
            CreateRobustHttpClient()
        };
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var client in _httpClients)
            try
            {
                client?.Dispose();
            }
            catch
            {
            }

        _httpClients.Clear();
        _disposed = true;
    }

    private HttpClient CreateFastHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = 10,
            ConnectTimeout = TimeSpan.FromSeconds(5),
            ResponseDrainTimeout = TimeSpan.FromSeconds(3),
            UseProxy = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All,
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true
            }
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "curl/8.0.0");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        client.DefaultRequestHeaders.ConnectionClose = true;

        return client;
    }

    private HttpClient CreateBypassHttpClient()
    {
        var handler = new HttpClientHandler
        {
            UseProxy = false,
            UseCookies = false,
            UseDefaultCredentials = false,
            PreAuthenticate = false,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
            AutomaticDecompression = DecompressionMethods.All,
            MaxConnectionsPerServer = 1
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; FastClient/1.0)");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        client.DefaultRequestHeaders.ConnectionClose = true;

        return client;
    }

    private HttpClient CreateRobustHttpClient()
    {
        var handler = new HttpClientHandler
        {
            UseProxy = true,
            UseDefaultCredentials = true,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        client.DefaultRequestHeaders.Add("Accept-Language", "vi-VN,vi;q=0.9,en;q=0.8");

        return client;
    }

    public static string CreateFormData(NameValueCollection data)
    {
        var pairs = new List<string>();
        foreach (string key in data.Keys)
            if (data[key] != null)
                pairs.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(data[key])}");

        return string.Join("&", pairs);
    }

    public async Task<string> PostAsync(string url, NameValueCollection data)
    {
        var formData = CreateFormData(data);
        return await PostAsync(url, formData);
    }

    public async Task<string> PostAsync(string url, string formData)
    {
        await PreResolveDns(url);

        var exceptions = new List<Exception>();

        foreach (var client in _httpClients)
            try
            {
                var result = await TryPostAsync(client, url, formData);
                if (!string.IsNullOrEmpty(result)) return result;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

        try
        {
            var result = await TryPostWithWebClientAsync(url, formData);
            if (!string.IsNullOrEmpty(result)) return result;
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            var result = await TryPostWithRawSocketAsync(url, formData);
            if (!string.IsNullOrEmpty(result)) return result;
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            return await PostWithSystemCall(url, formData);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        throw new AggregateException("All connection methods failed", exceptions);
    }

    private async Task<string> TryPostAsync(HttpClient client, string url, string formData)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            using var content = new StringContent(formData, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(url, content, cts.Token);

            if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
            return null;
        }
        catch
        {
            return null;
        }
    }

    [Obsolete("Obsolete")]
    private async Task<string> TryPostWithWebClientAsync(string url, string formData)
    {
        try
        {
            using var webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "curl/8.0.0");
            webClient.Headers.Add("Accept", "*/*");
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            webClient.Proxy = null;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var task = Task.Run(() => webClient.UploadString(url, "POST", formData), cts.Token);

            if (await Task.WhenAny(task, Task.Delay(10000, cts.Token)) == task) return await task;

            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> TryPostWithRawSocketAsync(string url, string formData)
    {
        try
        {
            var uri = new Uri(url);
            var isHttps = uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
            var port = uri.Port == -1 ? isHttps ? 443 : 80 : uri.Port;

            using var client = new TcpClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await client.ConnectAsync(uri.Host, port, cts.Token);

            Stream stream = client.GetStream();

            if (isHttps)
            {
                var sslStream = new SslStream(stream, false,
                    (sender, cert, chain, errors) => true);
                await sslStream.AuthenticateAsClientAsync(uri.Host);
                stream = sslStream;
            }

            stream.ReadTimeout = 5000;
            stream.WriteTimeout = 5000;

            var request = $"POST {uri.PathAndQuery} HTTP/1.1\r\n" +
                          $"Host: {uri.Host}\r\n" +
                          $"Content-Type: application/x-www-form-urlencoded\r\n" +
                          $"Content-Length: {Encoding.UTF8.GetByteCount(formData)}\r\n" +
                          $"User-Agent: FastClient/1.0\r\n" +
                          $"Connection: close\r\n\r\n" +
                          formData;

            var requestBytes = Encoding.UTF8.GetBytes(request);
            await stream.WriteAsync(requestBytes, cts.Token);

            var buffer = new byte[8192];
            var response = new StringBuilder();

            while (!cts.Token.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0) break;
                response.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }

            var responseStr = response.ToString();
            var parts = responseStr.Split(["\r\n\r\n"], 2, StringSplitOptions.None);

            await stream.DisposeAsync();
            return parts.Length > 1 ? parts[1] : responseStr;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string> PostWithSystemCall(string url, string formData)
    {
        var tempFile = Path.GetTempFileName();

        await File.WriteAllTextAsync(tempFile, formData);

        var curlArgs = $"-X POST -d @\"{tempFile}\" " +
                       $"-H \"Content-Type: application/x-www-form-urlencoded\" " +
                       $"-H \"User-Agent: curl/8.0.0\" " +
                       $"-H \"Accept: */*\" " +
                       $"-H \"Accept-Encoding: gzip, deflate, br\" " +
                       $"--connect-timeout 10 --max-time 15 " +
                       $"--compressed -s -k " +
                       $"\"{url}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "curl",
                Arguments = curlArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(15000))
        {
            process.Kill();
            throw new TimeoutException("Curl timeout");
        }

        try
        {
            File.Delete(tempFile);
        }
        catch
        {
        }

        if (process.ExitCode == 0 && !string.IsNullOrEmpty(output)) return output;

        throw new Exception($"Curl failed: {error}");
    }

    private async Task PreResolveDns(string url)
    {
        try
        {
            var uri = new Uri(url);
            var host = uri.Host;

            if (DnsCache.ContainsKey(host)) return;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var dnsTask = Task.Run(async () =>
            {
                try
                {
                    var addresses = await Dns.GetHostAddressesAsync(host, cts.Token);
                    return addresses.FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            }, cts.Token);

            if (await Task.WhenAny(dnsTask, Task.Delay(2000, cts.Token)) == dnsTask)
            {
                var ip = await dnsTask;
                if (ip != null) DnsCache[host] = ip;
            }
        }
        catch
        {
        }
    }
}