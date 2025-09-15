namespace LibElectronAppApi.Abstract;

public interface IImageResizer
{
    byte[] ResizeImageWithPad(byte[] inputBytes, uint width, uint height);
}