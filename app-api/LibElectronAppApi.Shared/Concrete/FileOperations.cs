using System.Runtime.InteropServices;
using System.Text;
using LibElectronAppApi.Shared.Abstract;

namespace LibElectronAppApi.Shared.Concrete;

public class FileOperations : IFileOperations
{
    private readonly IGuidGenerator guidGenerator;
    
    public FileOperations(IGuidGenerator guidGenerator)
    {
        this.guidGenerator = guidGenerator;
    }

    public FileInfo GetFileInfo(string filename)
    {
        return new FileInfo(filename);
    }

    public void CopyFile(string fromPath, string toPath, bool overwrite)
    {
        File.Copy(fromPath, toPath, overwrite);
    }

    public DirectoryInfo CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }

    public Stream CreateFileStream(string path, FileMode mode)
    {
        return new FileStream(path, mode);
    }

    public string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), guidGenerator.Create().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public string CreateTempFile()
    {
        return Path.GetTempFileName();
    }

    public void DeleteDirectory(string path, bool recursive)
    {
        Directory.Delete(path, recursive);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
            // Do nothing
        }
    }

    public void MoveFile(string fromPath, string toPath, bool overwrite)
    {
        var dir = Path.GetDirectoryName(toPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            if (!DirectoryExists(dir))
            {
                CreateDirectory(dir);
            }
        }

        if (overwrite)
        {
            if (FileExists(toPath))
            {
                DeleteFile(toPath);
            }
        }

        File.Move(fromPath, toPath);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public long GetFileSize(string filename)
    {
        if (FileExists(filename))
        {
            var info = new FileInfo(filename);
            return info.Length;
        }

        return 0;
    }

    public string[] GetFilesInDirectory(string path, string searchPattern, bool recurse = false)
    {
        return Directory.GetFiles(path, searchPattern,
            recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    public string[] GetSubDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    public string ReadAllText(string path, Encoding encoding)
    {
        return File.ReadAllText(path, encoding);
    }

    public void WriteAllBytesToFile(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
    }

    public void WriteAllTextToFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public string GetNameOfDirectoryNoPath(string path)
    {
        return new DirectoryInfo(path).Name;
    }

    public string GetLocalAppDataPathForCurrentPlatform()
    {
        var userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appDataSubPath = GetPlatformSpecificAppDataPath();
        var result = Path.Combine(userProfilePath, appDataSubPath);
        if (!DirectoryExists(result)) CreateDirectory(result);
        return result;
    }
    
    private string GetPlatformSpecificAppDataPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "AppData\\Local";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "Library/Application Support";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "local/etc"; 
        throw new PlatformNotSupportedException();
    }    
}