using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace JoinAGameTable.Services
{
    /// <summary>
    /// Image manipulation service.
    /// </summary>
    public class ImageManipulationService : IImageManipulationService
    {
        public Task ResizeImageAsync(Image<Rgba32> sourceImage, Size resize)
        {
            sourceImage.Mutate(x => x.Resize(resize));
            return Task.CompletedTask;
        }

        public Task CropAndResizeImageAsync(Image<Rgba32> sourceImage, Rectangle crop, Size resize)
        {
            sourceImage.Mutate(x => x.Crop(crop).Resize(resize));
            return Task.CompletedTask;
        }

        public Task SaveAsJpegAsync(Image<Rgba32> sourceImage, Stream stream)
        {
            sourceImage.SaveAsJpeg(stream, new JpegEncoder
            {
                Quality = 90,
            });
            stream.Position = 0;
            return Task.CompletedTask;
        }

        public Task SaveAsPngAsync(Image<Rgba32> sourceImage, Stream stream)
        {
            sourceImage.SaveAsPng(stream, new PngEncoder
            {
                CompressionLevel = 9,
            });
            stream.Position = 0;
            return Task.CompletedTask;
        }
    }
}
