using System.Text;

namespace LibElectronAppApi.Shared.Abstract;

public interface IFileOperations
{
    FileInfo GetFileInfo(string filename);
    void CopyFile(string fromPath, string toPath, bool overwrite);

    DirectoryInfo CreateDirectory(string path);

    Stream CreateFileStream(string path, FileMode mode);

    string CreateTempDirectory();

    string CreateTempFile();

    void DeleteDirectory(string path, bool recursive);

    void DeleteFile(string path);
        
    void TryDeleteFile(string path);

    void MoveFile(string fromPath, string toPath, bool overwrite);

    bool DirectoryExists(string path);

    bool FileExists(string path);

    long GetFileSize(string filename);

    string[] GetFilesInDirectory(string path, string searchPattern, bool recurse = false);

    string[] GetSubDirectories(string path);

    byte[] ReadAllBytes(string path);

    string ReadAllText(string path);

    string ReadAllText(string path, Encoding encoding);

    void WriteAllBytesToFile(string path, byte[] bytes);

    void WriteAllTextToFile(string path, string content);

    string GetNameOfDirectoryNoPath(string path);
    string GetLocalAppDataPathForCurrentPlatform();
}