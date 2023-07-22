using AutoGnome;
using AutoGnome.Integrations;
using Jint;
using Newtonsoft.Json;
using Quartz;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("config.yaml", optional: false);

builder.Services.AddQuartz(opts =>
{
    opts.UseMicrosoftDependencyInjectionJobFactory();
    opts.InterruptJobsOnShutdown = true;
    opts.InterruptJobsOnShutdownWithWait = true;
    opts.SetProperty("quartz.plugin.triggerHistory.type", "Quartz.Plugin.History.LoggingTriggerHistoryPlugin, Quartz.Plugins");
    opts.SetProperty("quartz.plugin.jobHistory.type", "Quartz.Plugin.History.LoggingJobHistoryPlugin, Quartz.Plugins");
});
builder.Services.AddQuartzServer(opts =>
{
    opts.AwaitApplicationStarted = true;
    opts.WaitForJobsToComplete = false;
});

builder.Services.AddSingleton<EngineFactory>();
builder.Services.AddSingleton<ScriptEngineManager>();
builder.Services.AddAllImplementations<ScriptIntegration>(new[] { typeof(ScriptIntegration).Assembly });
builder.Services.AddHostedService<ScriptLauncher>();


if (builder.Environment.IsDevelopment())
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.MapGet("/jobs", async (ISchedulerFactory schedulerFactory) =>
{
    var scheduler = await schedulerFactory.GetScheduler();
    var jobs = await scheduler.GetCurrentlyExecutingJobs();
    return JsonConvert.SerializeObject(jobs.Select(job => job.JobDetail));
});

app.Run();