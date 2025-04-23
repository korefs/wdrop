using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy for uploading files via P2P
    /// </summary>
    public class P2PUploadStrategy : IUploadStrategy
    {
        public string Name => "p2p";
        
        public async Task UploadAsync(UploadFile file)
        {
            await P2P.StartP2PServer(file);
        }
    }
}