namespace Wdrop;

public class Flows
{
    public static void DefaultUpload(string provider)
    {
        if (string.IsNullOrEmpty(provider))
        {
            WConsole.Info($"Current Default upload destination is '{Context.Config["uploadto"]}'");
            return;
        }

        string uploadDestinationArg = provider;

        if (!Context.uploadOptions.Contains(uploadDestinationArg))
        {
            WConsole.Error($"Error: Unknown destination '{uploadDestinationArg}'");
            WConsole.Info("Available destination providers:");
            WConsole.Info($"  {string.Join(", ", Context.uploadOptions)}");
            return;
        }

        Config.SetConfig("uploadto", provider);
        WConsole.Info($"Default upload destination set to '{provider}'");

    }
}