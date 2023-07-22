using JetBrains.Annotations;
using Quartz;

namespace AutoGnome;

[UsedImplicitly]
public class CallbackJob : IJob
{
    private readonly ScriptEngineManager _scriptEngineManager;
    public Guid CallbackId { get; set; } = Guid.Empty;

    public CallbackJob(ScriptEngineManager scriptEngineManager)
    {
        _scriptEngineManager = scriptEngineManager;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _scriptEngineManager.TryInvokeCallback(CallbackId, out var result);
        return Task.CompletedTask;
    }
}