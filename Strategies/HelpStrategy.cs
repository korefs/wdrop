using System.Text;
using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy for uploading files locally
    /// </summary>
    public class HelpStrategy : ICommandStrategy
    {
        public string Name => "help";
        
        public string[] Aliases => ["h"];

        public async Task Run(string[] args)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("--h, --help -- Show command list.");
            stringBuilder.AppendLine("--defaultupload, --du -- Set default upload \"provider\"");

            WConsole.Info(stringBuilder.ToString());
        }
    }
}