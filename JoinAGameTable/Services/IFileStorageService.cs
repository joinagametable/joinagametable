using System;
using System.IO;
using System.Threading.Tasks;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// All file storage implementations must extend this interface.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Name of the bucket to use.
        /// </summary>
        string BucketName { get; }

        /// <summary>
        /// Public access URL.
        /// </summary>
        string EndpointPublicUrl { get; }

        /// <summary>
        /// Stores the given file content on the storage.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentSize">Size of the file</param>
        /// <param name="contentStream">Stream containing the file content</param>
        /// <returns>A void Task</returns>
        Task StoreFileAsync(string fileName, string contentType, long contentSize, Stream contentStream);

        /// <summary>
        /// Retrieves a specific files from storage.
        /// </summary>
        /// <param name="fileName">Name of the file to retrieve</param>
        /// <param name="callback">A stream will be passed to the callback</param>
        /// <returns>A void Task</returns>
        Task RetrieveFileAsync(string fileName, Action<Stream> callback);

        /// <summary>
        /// Retrieves a specific file from the storage.
        /// </summary>
        /// <param name="fileName">Name of the file to retrieve</param>
        /// <returns>Stream to the file content</returns>
        Task<Stream> RetrieveFileAsync(string fileName);

        /// <summary>
        /// Deletes a specific file from the storage.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>A void Task</returns>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// Deletes a specific file from the storage.
        /// </summary>
        /// <param name="directory">The directory path</param>
        /// <param name="fileName">The file name</param>
        /// <returns>A void Task</returns>
        Task DeleteFileAsync(string directory, string fileName);

        /// <summary>
        /// Generate public URL for the given file.
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>Public URL to access file</returns>
        string GenerateFilePublicUrl(string fileName);
    }
}
