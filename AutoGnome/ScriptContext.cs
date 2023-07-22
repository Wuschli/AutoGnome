using Jint;

namespace AutoGnome;

public class ScriptContext
{
    public string SourceFile { get; init; } = null!;
    public ILogger Logger { get; init; } = null!;
    public Engine Engine { get; init; } = null!;
}