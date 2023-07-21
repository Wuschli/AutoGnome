using AutoGnome.Services;

namespace AutoGnome.Integrations;

public abstract class ScriptIntegration
{
    public abstract string Name { get; }
    public ScriptContext Context { get; set; }
}