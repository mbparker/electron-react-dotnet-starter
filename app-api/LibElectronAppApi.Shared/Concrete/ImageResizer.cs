using ImageMagick;
using LibElectronAppApi.Shared.Abstract;

namespace LibElectronAppApi.Shared.Concrete;

public class ImageResizer : IImageResizer
{
    public byte[] ResizeImageWithPad(byte[] inputBytes, uint width, uint height)
    {
        using (var image = new MagickImage(inputBytes))
        {
            var geom = new MagickGeometry(width, height);
            image.Resize(geom);
            image.BackgroundColor = MagickColors.Black;
            image.Extent(width, height, Gravity.Center);
            using (var stream = new MemoryStream())
            {
                image.Write(stream);
                return stream.GetBuffer();
            }
        }
    }
}