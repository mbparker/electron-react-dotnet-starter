using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.Tests;

public class SqliteUniqueIdGeneratorTests
{
    [Fact]
    public void NewUniqueId_ReturnsNonEmptyString()
    {
        // Arrange
        var generator = new SqliteUniqueIdGenerator();
        
        // Act
        var result = generator.NewUniqueId();
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void NewUniqueId_ReturnsValidGuidFormat()
    {
        // Arrange
        var generator = new SqliteUniqueIdGenerator();
        
        // Act
        var result = generator.NewUniqueId();
        
        // Assert
        Assert.Equal(32, result.Length); // GUID without hyphens
        Assert.All(result, c => Assert.True(char.IsAsciiHexDigit(c)));
    }

    [Fact]
    public void NewUniqueId_ReturnsDifferentValuesOnConsecutiveCalls()
    {
        // Arrange
        var generator = new SqliteUniqueIdGenerator();
        
        // Act
        var result1 = generator.NewUniqueId();
        var result2 = generator.NewUniqueId();
        
        // Assert
        Assert.NotEqual(result1, result2);
    }
}
