using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.Tests;

public class SqliteFileOperationsTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly SqliteFileOperations _fileOperations;

    public SqliteFileOperationsTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid():N}.db");
        _fileOperations = new SqliteFileOperations();
    }

    [Fact]
    public void FileExists_ReturnsFalseForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid():N}.db");
        
        // Act
        var result = _fileOperations.FileExists(nonExistentPath);
        
        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_ReturnsTrueForExistingFile()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "test content");
        
        // Act
        var result = _fileOperations.FileExists(_testFilePath);
        
        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DeleteFile_RemovesExistingFile()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "test content");
        Assert.True(File.Exists(_testFilePath)); // Ensure file exists
        
        // Act
        _fileOperations.DeleteFile(_testFilePath);
        
        // Assert
        Assert.False(File.Exists(_testFilePath));
    }

    [Fact]
    public void DeleteFile_DoesNotThrowForNonExistentFile()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid():N}.db");
        
        // Act & Assert (should not throw)
        _fileOperations.DeleteFile(nonExistentPath);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}