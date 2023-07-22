using AutoGnome.Integrations;
using JetBrains.Annotations;
using Jint;
using Jint.Runtime;
using Quartz;

namespace AutoGnome.Services;

[UsedImplicitly]
[DisallowConcurrentExecution]
public class ScriptJob : IJob
{
    private readonly IEnumerable<ScriptIntegration> _integrations;
    private readonly ILoggerFactory _loggerFactory;
    public string SourcePath { get; set; } = null!;
    public string WorkspacePath { get; set; } = null!;

    public ScriptJob(IEnumerable<ScriptIntegration> integrations, ILoggerFactory loggerFactory)
    {
        _integrations = integrations;
        _loggerFactory = loggerFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var log = _loggerFactory.CreateLogger(SourcePath);
        var scriptContext = new ScriptContext
        {
            Logger = log
        };

        try
        {
            var source = await File.ReadAllTextAsync(SourcePath);
            var engine = new Engine(cfg =>
            {
                cfg.EnableModules(WorkspacePath);
                cfg.AllowClr();
                cfg.CancellationToken(context.CancellationToken);
            });
            foreach (var integration in _integrations)
            {
                integration.Context = scriptContext;
                engine.SetValue(integration.Name, integration);
            }

            engine.Execute(source);
        }
        catch (ExecutionCanceledException)
        {
            log.LogInformation("Script execution cancelled");
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(msg: "Script execution failed", refireImmediately: false, cause: ex);
        }
    }
}

public class ScriptContext
{
    public ILogger Logger { get; init; } = null!;
}