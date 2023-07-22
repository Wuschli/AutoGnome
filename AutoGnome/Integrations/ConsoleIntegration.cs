using JetBrains.Annotations;

namespace AutoGnome.Integrations;

[UsedImplicitly]
public class ConsoleIntegration : ScriptIntegration
{
    public override string Name => "console";

    [UsedImplicitly]
    public void Log(object value)
    {
        Context.Logger.LogInformation(value.ToString());
    }
}