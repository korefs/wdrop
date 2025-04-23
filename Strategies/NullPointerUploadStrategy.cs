using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy for uploading files to 0x0.st
    /// </summary>
    public class NullPointerUploadStrategy : IUploadStrategy
    {
        public string Name => "0x0.st";
        
        public async Task UploadAsync(UploadFile file)
        {
            await NullPointerUpload.UploadAsync(file);
        }
    }
}