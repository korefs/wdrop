using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy for uploading files locally
    /// </summary>
    public class LocalUploadStrategy : IUploadStrategy
    {
        public string Name => "local";
        
        public async Task UploadAsync(UploadFile file)
        {
            await Local.UploadLocal(file);
        }
    }
}