using JetBrains.Annotations;
using Jint.Native.Function;
using Quartz;

namespace AutoGnome.Integrations;

[UsedImplicitly]
public class SchedulerIntegration : ScriptIntegration
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ScriptEngineManager _callbacks;
    public override string Name => "scheduler";

    public SchedulerIntegration(ISchedulerFactory schedulerFactory, ScriptEngineManager callbacks)
    {
        _schedulerFactory = schedulerFactory;
        _callbacks = callbacks;
    }

    [UsedImplicitly]
    public async Task Delay(int milliseconds, FunctionInstance callback)
    {
        Context.Logger.LogDebug($"Delay {milliseconds}ms");
        var scheduler = await _schedulerFactory.GetScheduler();

        var callbackId = _callbacks.AddCallback(callback);

        var job = JobBuilder.Create<CallbackJob>()
            .WithIdentity("Delay", Context.SourceFile)
            .UsingJobData("callbackId", callbackId)
            .UsingJobData("sourcePath", Context.SourceFile)
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity("Delay", Context.SourceFile)
            .StartAt(DateTimeOffset.Now.AddMilliseconds(milliseconds))
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}