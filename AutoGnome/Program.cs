using AutoGnome;
using AutoGnome.Integrations;
using AutoGnome.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddYamlFile("config.yaml", optional: false);

builder.Services.AddQuartz(opts =>
{
    opts.UseMicrosoftDependencyInjectionJobFactory();
    opts.InterruptJobsOnShutdown = true;
    opts.InterruptJobsOnShutdownWithWait = true;
});
builder.Services.AddQuartzHostedService(opts =>
{
    opts.AwaitApplicationStarted = true;
    opts.WaitForJobsToComplete = false;
});

builder.Services.AddAllImplementations<ScriptIntegration>(new[] { typeof(ScriptIntegration).Assembly });
builder.Services.AddHostedService<ScriptLauncher>();


if (builder.Environment.IsDevelopment())
    builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();


app.Run();