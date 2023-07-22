using AutoGnome.Integrations;
using Jint;

namespace AutoGnome;

public class EngineFactory
{
    private readonly IEnumerable<ScriptIntegration> _integrations;
    private readonly ILoggerFactory _loggerFactory;

    public EngineFactory(IEnumerable<ScriptIntegration> integrations, ILoggerFactory loggerFactory)
    {
        _integrations = integrations;
        _loggerFactory = loggerFactory;
    }

    public Engine Build(string? workspacePath, string sourcePath, CancellationToken ct)
    {
        var relativeSourcePath = Path.GetRelativePath(workspacePath ?? ".", sourcePath);
        var logger = _loggerFactory.CreateLogger(relativeSourcePath);

        var engine = new Engine(cfg =>
        {
            if (!string.IsNullOrEmpty(workspacePath))
                cfg.EnableModules(workspacePath);
            cfg.AllowClr();
            cfg.CancellationToken(ct);
        });

        var scriptContext = new ScriptContext
        {
            SourceFile = relativeSourcePath,
            Engine = engine,
            Logger = logger
        };

        foreach (var integration in _integrations)
        {
            integration.Context = scriptContext;
            engine.SetValue(integration.Name, integration);
        }

        return engine;
    }
}