using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;

namespace RSoft.RGet;
public class Program
{
    static RGetContext ctx = null!;
    static RGetConsole console = null!;
    static RGet rget = null!;
    static async Task<int> Main(string[] args)
    {
        ctx = new RGetContext();
        console = new RGetConsole(ctx);
        rget = new RGet(ctx, console);



        var urlArgument = new Argument<string>("url", "The URL to download");
        urlArgument.SetDefaultValue("");

        var fileOption = new Option<FileInfo?>("--input-file", "A file containing URLs in lines. Lines starting with \"-\" are ignored. (ignored if url argument is set)");
        fileOption.AddAlias("-i");

        var timeoutOption = new Option<int?>("--timeout", "Timeout in seconds");
        timeoutOption.AddAlias("-t");

        var quietOption = new Option<bool>("--quiet", "Quiet mode");
        quietOption.AddAlias("-q");

        var outputOption = new Option<string?>("--output-document", "File name to save response to (ignored if using input-file option)");
        outputOption.AddAlias("-O");

        var successOnlyOption = new Option<bool>("--success-only", "Only save result if response is HTTP 200");
        successOnlyOption.AddAlias("-s");

        var logOption = new Option<FileInfo?>("--output-file", "Log to file instead of console");
        logOption.AddAlias("-o");

        var userAgentOption = new Option<string>("--user-agent", "The User agent string to use for the HTTP request");
        userAgentOption.AddAlias("-U");
        userAgentOption.SetDefaultValue($"RGet/{Assembly.GetExecutingAssembly().GetName().Version} ({Environment.OSVersion})");

        var baseAddressOption = new Option<string?>("--base", "The base address for the URL");
        baseAddressOption.AddAlias("-B");

        var waitOption = new Option<int?>("--wait", "Wait X seconds between requests");
        waitOption.AddAlias("-w");

        var backgroundOption = new Option<bool>("--background", "Start the download in the background");
        backgroundOption.AddAlias("-b");


        var rootCommand = new RootCommand("A dumb wget alternative for windows");
        rootCommand.AddArgument(urlArgument);
        rootCommand.AddOption(fileOption);
        rootCommand.AddOption(timeoutOption);
        rootCommand.AddOption(quietOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(successOnlyOption);
        rootCommand.AddOption(logOption);
        rootCommand.AddOption(userAgentOption);
        rootCommand.AddOption(baseAddressOption);
        rootCommand.AddOption(waitOption);
        rootCommand.AddOption(backgroundOption);


        rootCommand.SetHandler(async (InvocationContext ictx) =>
        {
            var pr = ictx.ParseResult;

            if(pr.GetValueForOption(backgroundOption))
            {
                var exeArgs = pr.Tokens.Select(t => t.Value).Where(t => !backgroundOption.Aliases.Contains(t)).ToArray();
                StartNewInstance(exeArgs);
                return;
            }

            ctx.Quiet = pr.GetValueForOption(quietOption);
            ctx.SuccessOnly = pr.GetValueForOption(successOnlyOption);
            ctx.Timeout = pr.GetValueForOption(timeoutOption);
            ctx.LogFile = pr.GetValueForOption(logOption);
            ctx.BaseUri = pr.GetValueForOption(baseAddressOption);
            ctx.Wait = pr.GetValueForOption(waitOption);
            ctx.UserAgent = pr.GetValueForOption(userAgentOption) ?? "RGet";

            var url = pr.GetValueForArgument(urlArgument);
            var file = pr.GetValueForOption(fileOption);
            var output = pr.GetValueForOption(outputOption);

            if (!string.IsNullOrEmpty(url))
            {
                await rget.GetUrl(url, output);
            }
            else if (file != null) {
                if (!file.Exists)
                {
                    console.WriteLine($"File {file.Name} doesn't exist!");
                }
                else
                {
                    var lines = File.ReadAllLines(file.FullName).Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("-")).ToArray();

                    for(var i = 0; i < lines.Length; i++)
                    {
                        await rget.GetUrl(lines[i]);
                        if (ctx.Wait.HasValue && i < lines.Length - 1)
                        {
                            console.WriteLine($"Waiting {ctx.Wait} seconds before next request");
                            await Task.Delay(ctx.Wait.Value * 1000);
                        }
                    }
                }
            }
        });

        return await rootCommand.InvokeAsync(args);
    }

    static void StartNewInstance(string[] args)
    {
        var process = Process.GetCurrentProcess();
        var exe = process.MainModule!.FileName!;
        var exeArgs = string.Join(" ", args);

        console.LogTime();
        console.WriteLine($"Starting background process: {exe} {exeArgs}");

        var psi = new ProcessStartInfo(exe, exeArgs)
        {
            CreateNoWindow = true,
        };
        Process.Start(psi);
        return;
    }
}



