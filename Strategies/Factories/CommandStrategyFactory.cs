namespace Wdrop.Strategies;

/// <summary>
/// Factory for creating command strategy instances
/// </summary>
public static class CommandStrategyFactory
{
    private static readonly Dictionary<string, ICommandStrategy> _strategies = new()
    {
        { Constants.HELP_SHORT_COMMAND, new HelpStrategy() },
        { Constants.HELP_COMMAND, new HelpStrategy() },
        { Constants.DEFAULT_UPLOAD_COMMAND, new DefaultUploadStrategy() },
        { Constants.DEFAULT_UPLOAD_SHORT_COMMAND, new DefaultUploadStrategy() },
    };

    /// <summary>
    /// Gets all available command strategies
    /// </summary>
    public static IEnumerable<string> AvailableStrategies => _strategies.Keys;

    /// <summary>
    /// Gets a strategy by name
    /// </summary>
    /// <param name="input">The name of the strategy</param>
    /// <returns>The command strategy or null</returns>
    public static ICommandStrategy? GetStrategy(string input)
    {
        string command = input.Substring(2);
        
        if (string.IsNullOrEmpty(command) || !_strategies.ContainsKey(command))
        {
            WConsole.Warn($"Unrecognized command '{input}'. Use --h to list all commands.");
            return null;
        }

        return _strategies[command];
    }
}