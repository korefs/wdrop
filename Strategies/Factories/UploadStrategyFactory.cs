using System;
using System.Collections.Generic;
using System.Linq;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Factory for creating upload strategy instances
    /// </summary>
    public static class UploadStrategyFactory
    {
        private static readonly Dictionary<string, IUploadStrategy> _strategies = new()
        {
            { "0x0.st", new NullPointerUploadStrategy() },
            { "p2p", new P2PUploadStrategy() },
            { "local", new LocalUploadStrategy() }
        };

        /// <summary>
        /// Gets all available upload strategies
        /// </summary>
        public static IEnumerable<string> AvailableStrategies => _strategies.Keys;

        /// <summary>
        /// Gets a strategy by name
        /// </summary>
        /// <param name="name">The name of the strategy</param>
        /// <returns>The upload strategy or the default (local) strategy if not found</returns>
        public static IUploadStrategy GetStrategy(string name)
        {
            if (string.IsNullOrEmpty(name) || !_strategies.ContainsKey(name))
            {
                WConsole.Warn($"Invalid upload destination '{name}'. Using 'local' instead.");
                return _strategies["local"];
            }

            return _strategies[name];
        }
    }
}