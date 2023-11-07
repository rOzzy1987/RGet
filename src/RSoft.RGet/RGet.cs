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

    public async Task GetUrl(string url, string? outputFile = null)
    {

        var client = GetHttpClient();
        var uri = client.BaseAddress == null 
            ? new Uri(url) 
            : new Uri(client.BaseAddress, url);

        _console.LogTime();
        _console.WriteLine($"Getting {uri}");

        var getTask = client.GetAsync(uri);

        await Progress(getTask);

        var response = getTask.Result;
        var filenameRegex = new Regex("^attachment;(?:\\s*)filename=");

        var responseFilename = response.Headers.FirstOrDefault(h => h.Key == "content-disposition").Value?.FirstOrDefault();
        responseFilename = responseFilename == null ? null : filenameRegex.Replace(responseFilename, "");
        outputFile ??= responseFilename ?? _illegalFileNameChars.Replace(uri.ToString().Split('/').Last(p => p != ""), "_");

        _console.WriteLine($"Completed: {(int)response.StatusCode}");


        if (!_context.SuccessOnly || response.IsSuccessStatusCode)
        {
            _console.WriteLine($"Writing output to {outputFile}");
            File.WriteAllBytes(outputFile, response.Content.ReadAsByteArrayAsync().Result);
        }

        _console.WriteLine();

    }

    protected async Task Progress(Task task, int c = 10)
    {
        _console.Write(new string('-', c), true);
        var i = 0;
        while (!(new[] { TaskStatus.Canceled, TaskStatus.RanToCompletion, TaskStatus.Faulted }).Contains(task.Status))
        {
            _console.Jump(column: -c);
            for (var j = 0; j < c; j++)
            {
                _console.Write(i % c == j ? "O" : "-", true);
            }
            i++;
            await Task.Delay(100);
        }
        _console.Jump(column: -c);
        _console.Write(new string(' ', c), true);
        _console.Jump(column: -c);
    }

    
}
