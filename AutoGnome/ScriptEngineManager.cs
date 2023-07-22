using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;

namespace AutoGnome;

public class ExecutableScript
{
    public Engine Engine { get; }
    public string SourcePath { get; }

    public ExecutableScript(Engine engine, string sourcePath)
    {
        Engine = engine;
        SourcePath = sourcePath;
    }
}

public class ScriptEngineManager
{
    private readonly Dictionary<Guid, FunctionInstance> _callbacks = new();

    private readonly ILogger _logger;

    public ScriptEngineManager(ILogger<ScriptEngineManager> logger)
    {
        _logger = logger;
    }

    public async Task RunScriptSource(ExecutableScript script, CancellationToken ct)
    {
        var source = await File.ReadAllTextAsync(script.SourcePath, ct);
        //var engine = CreateEngine(workspacePath, sourcePath, ct);

        try
        {
            lock (script.Engine)
            {
                _logger.LogInformation($"Start Execution of {script.SourcePath}");
                script.Engine.Execute(source);
                _logger.LogInformation($"Finished Execution of {script.SourcePath}");
            }
        }
        catch (ExecutionCanceledException)
        {
            _logger.LogInformation($"Script execution cancelled for {script.SourcePath}");
        }
    }

    public Guid AddCallback(FunctionInstance callback)
    {
        var id = Guid.NewGuid();
        _callbacks.Add(id, callback);
        return id;
    }

    public bool RemoveCallback(Guid callbackId)
    {
        return _callbacks.Remove(callbackId);
    }

    public bool TryInvokeCallback(Guid callbackId, out JsValue? result, params object?[] arguments)
    {
        if (_callbacks.TryGetValue(callbackId, out var callback))
        {
            try
            {
                lock (callback.Engine)
                {
                    _logger.LogInformation($"Start Execution of Callback {callbackId}");
                    result = callback.Engine.Invoke(callback, arguments);
                    _logger.LogInformation($"Finished Execution of Callback {callbackId}");
                    return true;
                }
            }
            catch (ExecutionCanceledException)
            {
                _logger.LogInformation($"Script execution cancelled for callback {callbackId}");
            }
        }

        result = null;
        return false;
    }
}