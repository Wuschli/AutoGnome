using JetBrains.Annotations;
using Quartz;

namespace AutoGnome;

[UsedImplicitly]
[DisallowConcurrentExecution]
public class ScriptJob : IJob
{
    private readonly ScriptEngineManager _scriptEngineManager;
    private readonly EngineFactory _engineFactory;
    public string SourcePath { get; set; } = null!;
    public string WorkspacePath { get; set; } = null!;

    public ScriptJob(ScriptEngineManager scriptEngineManager, EngineFactory engineFactory)
    {
        _scriptEngineManager = scriptEngineManager;
        _engineFactory = engineFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var engine = _engineFactory.Build(WorkspacePath, SourcePath, context.CancellationToken);
            var script = new ExecutableScript(engine, SourcePath);
            await _scriptEngineManager.RunScriptSource(script, context.CancellationToken);
        }
        catch (Exception ex)
        {
            throw new JobExecutionException(msg: "Script execution failed", refireImmediately: false, cause: ex);
        }
    }
}