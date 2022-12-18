using System.Text;
using System.Text.Json;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => services.AddGitHubActionServices())
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
{
    using CancellationTokenSource tokenSource = new();

    Console.CancelKeyPress += delegate
    {
        tokenSource.Cancel();
    };

    var analyser = Get<JsonFileAnalyzer>(host);

    var result = await analyser.AnalyzeAsunc(inputs.JSonFilePath, inputs.Properties.Split(';'), tokenSource.Token);
    var dataResult = JsonSerializer.Serialize(result);
    // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
    // ::set-output deprecated as mentioned in https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/
    var githubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT", EnvironmentVariableTarget.Process);
    if (!string.IsNullOrWhiteSpace(githubOutputFile))
    {
        using (var textWriter = new StreamWriter(githubOutputFile!, true, Encoding.UTF8)) 
            textWriter.WriteLine($"json-values={dataResult}");  
    }
    else
    {
        Console.WriteLine($"::set-output name=json-values::{dataResult}"); 
    }

    Environment.Exit(0);
}

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("JsonFileExtractor.GitHubAction.Program")
            .LogError(
                string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartAnalysisAsync(options, host));
await host.RunAsync();
