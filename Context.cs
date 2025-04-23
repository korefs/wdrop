using System.Collections.Generic;
using Wdrop.Strategies;

namespace Wdrop
{
    public static class Context
    {
        public static Dictionary<string, string> Config { get; set; } = new ();
        
        // Use the available strategies from the factory
        public static IEnumerable<string> uploadOptions => UploadStrategyFactory.AvailableStrategies;
    }
}