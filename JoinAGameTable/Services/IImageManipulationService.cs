using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// All image manipulation implementations must extend this interface.
    /// </summary>
    public interface IImageManipulationService
    {
        /// <summary>
        /// Resize image. The given image is directly modified.
        /// </summary>
        /// <param name="sourceImage">The image to modify</param>
        /// <param name="resize">Image size</param>
        /// <returns>A void Task</returns>
        Task ResizeImageAsync(Image<Rgba32> sourceImage, Size resize);

        /// <summary>
        /// Crop and resize image. The given image is directly modified.
        /// </summary>
        /// <param name="sourceImage">The image to modify</param>
        /// <param name="crop">Portion of the image object to retain.</param>
        /// <param name="resize">Image size</param>
        /// <returns>A void Task</returns>
        Task CropAndResizeImageAsync(Image<Rgba32> sourceImage, Rectangle crop, Size resize);

        /// <summary>
        /// Save image on the given stream as JPEG.
        /// </summary>
        /// <param name="sourceImage">The image to save</param>
        /// <param name="stream">Destination stream</param>
        /// <returns>A void Task</returns>
        Task SaveAsJpegAsync(Image<Rgba32> sourceImage, Stream stream);

        /// <summary>
        /// Save image on the given stream as PNG.
        /// </summary>
        /// <param name="sourceImage">The image to save</param>
        /// <param name="stream">Destination stream</param>
        /// <returns>A void Task</returns>
        Task SaveAsPngAsync(Image<Rgba32> sourceImage, Stream stream);
    }
}
