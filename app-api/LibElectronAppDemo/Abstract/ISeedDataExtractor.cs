using LibElectronAppDemo.Models;

namespace LibElectronAppDemo.Abstract;

public interface ISeedDataExtractor
{
    Discography LoadDiscography();
    AlbumArtwork LoadAlbumArtwork(int number);
}