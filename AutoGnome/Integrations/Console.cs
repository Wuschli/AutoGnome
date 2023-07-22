namespace AutoGnome.Integrations;

public class Console : ScriptIntegration
{
    public override string Name => "console";

    public void Log(object value)
    {
        Context.Logger.LogInformation(value.ToString());
    }
}