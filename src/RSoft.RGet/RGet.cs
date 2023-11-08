using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace RSoft.RGet;

public class RGet
{
    private readonly RGetConsole _console;
    private readonly RGetContext _context;

    private HttpClient _httpClient;
    static Regex _illegalFileNameChars = new Regex(@"[^a-zA-Z0-9\.\-_]");

    public RGet(RGetContext context, RGetConsole console)
    {
        _context = context;
        _console = console;
    }

    private HttpClient GetHttpClient()
    {
        if(_httpClient == null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _context.UserAgent);
            if (_context.Timeout.HasValue)
                _httpClient.Timeout = TimeSpan.FromSeconds(_context.Timeout.Value);
            if (_context.BaseUri != null)
                _httpClient.BaseAddress = new Uri(_context.BaseUri);
        }
        return _httpClient;
    }

    public async Task SendRequest(string url, string? outputFile = null)
    {

        var client = GetHttpClient();
        var uri = client.BaseAddress == null 
            ? new Uri(url) 
            : new Uri(client.BaseAddress, url);

        _console.LogTime();
        _console.WriteLine($"Getting {uri}");

        var getTask = client.SendAsync(GetRequest(GetMethod(), uri));
        await _console.ProgressBar(getTask);
        var response = getTask.Result;
        

        outputFile = GetOutputFile(outputFile, response, uri);
        _console.WriteLine($"Response: {(int)response.StatusCode} {response.StatusCode}");

        if (!_context.SuccessOnly || response.IsSuccessStatusCode)
        {
            _console.WriteLine($"Writing output to {outputFile}");
            WriteFile(response, outputFile);
        }

        _console.WriteLine();
    }

    private HttpRequestMessage GetRequest(HttpMethod httpMethod, Uri uri)
    {
        var req = new HttpRequestMessage(httpMethod, uri);
        var defaultMediaType = "text/plain";
        if(_context.Body != null)
        {
            req.Content = new ByteArrayContent(_context.Body);
        }
        if(_context.BodyStr != null)
        {
            req.Content = new StringContent(_context.BodyStr);
        }
        if(req.Content != null)
        {
            var headerValue = new MediaTypeHeaderValue(_context.MediaType ?? defaultMediaType);
            req.Content.Headers.ContentType = headerValue;
        }
        return req;
    }

    private HttpMethod GetMethod()
    {
        return _context.Method ?? HttpMethod.Get;
    }

    private string GetOutputFile(string? outputFile, HttpResponseMessage response, Uri uri)
    {
        var filenameRegex = new Regex("^attachment;(?:\\s*)filename=");

        var responseFilename = response.Headers.FirstOrDefault(h => h.Key == "content-disposition").Value?.FirstOrDefault();
        responseFilename = responseFilename == null ? null : filenameRegex.Replace(responseFilename, "");
        outputFile ??= responseFilename ?? _illegalFileNameChars.Replace(uri.ToString().Split('/').Last(p => p != ""), "_");
        return outputFile;
    }

    protected void WriteFile(HttpResponseMessage response, string outputFile)
    {
        const int L = 1500;

        var totalLength = response.Content.Headers.ContentLength!;
        var totalRead = 0;
        var buffer = new byte[L];

        using (var file = File.OpenWrite(outputFile))
        using (var input = response.Content.ReadAsStream())
        {
            _console.WriteLine();

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, L)) > 0)
            {
                file.Write(buffer, 0, bytesRead);
                totalRead += bytesRead;
                _console.ProgressBar(totalRead / (double)totalLength);
            }
            _console.ClearProgressBar();
        }
    }
}
