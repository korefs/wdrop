using System.Threading.Tasks;
using Wdrop;
using Wdrop.Connections;

namespace Wdrop.Strategies
{
    /// <summary>
    /// Strategy interface for file upload methods
    /// </summary>
    public interface IUploadStrategy
    {
        /// <summary>
        /// Upload a file using the specific strategy
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <returns>A task representing the asynchronous upload operation</returns>
        Task UploadAsync(UploadFile file);
        
        /// <summary>
        /// Gets the name of the strategy
        /// </summary>
        string Name { get; }
    }
}