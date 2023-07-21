using AutoGnome;
using AutoGnome.Services;
using AutoGnome.Integrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

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

var host = builder.Build();
await host.RunAsync();