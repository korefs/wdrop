using System.Text;
using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy for commands
    /// </summary>
    public class DefaultUploadStrategy : ICommandStrategy
    {
        public string Name => "defaultupload";
        
        public string[] Aliases => ["du"];

        public async Task Run(string[] args)
        {
            Flows.DefaultUpload(args[0]);
        }
    }
}