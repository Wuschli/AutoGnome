using Quartz;
using Quartz.Impl.Matchers;

namespace AutoGnome;

public class ScriptLauncher : IHostedService
{
    private readonly IConfiguration _config;
    private readonly ILogger _log;
    private readonly ISchedulerFactory _schedulerFactory;
    private FileSystemWatcher _watcher = null!;
    private string _workspacePath = null!;

    public ScriptLauncher(IConfiguration config, ILogger<ScriptLauncher> log, ISchedulerFactory schedulerFactory)
    {
        _config = config;
        _log = log;
        _schedulerFactory = schedulerFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _workspacePath = _config.GetValue("workspace", ".")!;
        _watcher = new FileSystemWatcher(_workspacePath);

        _watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        _watcher.Changed += OnChanged;
        _watcher.Created += OnCreated;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;

        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
        _watcher.Filter = "*.js";

        Directory.GetFiles(_workspacePath, "*.js", SearchOption.AllDirectories);
        foreach (var scriptFullPath in Directory.EnumerateFiles(_workspacePath, "*.js", SearchOption.AllDirectories))
        {
            await ScheduleScriptJob(scriptFullPath, scriptFullPath, cancellationToken);
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        _log.LogDebug($"Changed: {e.FullPath}");
        _ = ScheduleScriptJob(e.FullPath, e.FullPath, CancellationToken.None);
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        _log.LogDebug($"Created: {e.FullPath}");
        _ = ScheduleScriptJob(e.FullPath, e.FullPath, CancellationToken.None);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        _log.LogDebug($"Deleted: {e.FullPath}");
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        _log.LogDebug($"Renamed:");
        _log.LogDebug($"    Old: {e.OldFullPath}");
        _log.LogDebug($"    New: {e.FullPath}");
        _ = ScheduleScriptJob(e.FullPath, e.OldFullPath, CancellationToken.None);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        PrintException(e.GetException());
    }

    private void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            _log.LogDebug($"Message: {ex.Message}");
            _log.LogDebug("Stacktrace:");
            _log.LogDebug(ex.StackTrace);
            PrintException(ex.InnerException);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher.Dispose();
        return Task.CompletedTask;
    }

    private async Task ScheduleScriptJob(string fullPath, string oldFullPath, CancellationToken ct = default)
    {
        var group = Path.GetRelativePath(_workspacePath, fullPath);
        var oldGroup = Path.GetRelativePath(_workspacePath, oldFullPath);

        var filename = Path.GetFileName(fullPath);
        var job = JobBuilder.Create<ScriptJob>()
            .WithIdentity(filename, group)
            .UsingJobData("sourcePath", fullPath)
            .UsingJobData("workspacePath", _workspacePath)
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity(filename, group)
            .StartAt(DateTimeOffset.Now.AddMilliseconds(200))
            .Build();

        var scheduler = await _schedulerFactory.GetScheduler(ct);

        var oldJobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(oldGroup), ct);
        foreach (var key in oldJobKeys)
            await scheduler.Interrupt(key, ct);
        await scheduler.DeleteJobs(oldJobKeys, ct);

        await scheduler.ScheduleJob(job, trigger, ct);
        var executingJobs = await scheduler.GetCurrentlyExecutingJobs(ct);
        _log.LogInformation($"Job for {fullPath} scheduled. {executingJobs.Count} jobs executing.");
    }
}