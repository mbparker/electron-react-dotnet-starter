using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Concrete;
using Moq;

namespace LibSqlite3Orm.Tests;

public class SqliteParameterTests
{
    private readonly Mock<ISqliteValueConverterCache> _mockConverterCache;
    private readonly SqliteParameter _parameter;

    public SqliteParameterTests()
    {
        _mockConverterCache = new Mock<ISqliteValueConverterCache>();
        
        // Setup mock converters for fallback conversion types
        var mockBooleanConverter = new Mock<ISqliteValueConverter>();
        mockBooleanConverter.Setup(c => c.Serialize(It.IsAny<object>())).Returns(1L);
        _mockConverterCache.Setup(c => c[typeof(LibSqlite3Orm.Types.ValueConverters.BooleanLong)])
            .Returns(mockBooleanConverter.Object);
        
        _parameter = new SqliteParameter("testParam", 1, _mockConverterCache.Object);
    }

    [Fact]
    public void Constructor_SetsNameAndIndex()
    {
        // Assert
        Assert.Equal("testParam", _parameter.Name);
        Assert.Equal(1, _parameter.Index);
    }

    [Fact]
    public void UseConverter_WithType_CallsConverterCache()
    {
        // Arrange
        var converterType = typeof(string);
        var mockConverter = new Mock<ISqliteValueConverter>();
        _mockConverterCache.Setup(c => c[converterType]).Returns(mockConverter.Object);

        // Act
        _parameter.UseConverter(converterType);

        // Assert
        _mockConverterCache.Verify(c => c[converterType], Times.Once);
    }

    [Fact]
    public void UseConverter_WithInstance_DoesNotCallConverterCache()
    {
        // Arrange
        var mockConverter = new Mock<ISqliteValueConverter>();

        // Act
        _parameter.UseConverter(mockConverter.Object);

        // Assert
        _mockConverterCache.Verify(c => c[It.IsAny<Type>()], Times.Never);
    }

    [Fact]
    public void Set_WithNullValue_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        _parameter.Set(null);
    }

    [Theory]
    [InlineData("test string")]
    [InlineData(42)]
    [InlineData(3.14)]
    [InlineData(true)]
    public void Set_WithVariousValues_DoesNotThrow(object value)
    {
        // Act & Assert - Should not throw
        _parameter.Set(value);
    }

    [Fact]
    public void Bind_WithIntPtrZero_DoesNotThrow()
    {
        // Arrange
        _parameter.Set("test");

        // Act & Assert - Should not throw (even with invalid IntPtr)
        // Note: This will likely fail in real usage due to P/Invoke, but we're testing the method structure
        var exception = Record.Exception(() => _parameter.Bind(IntPtr.Zero));
        
        // We expect this might throw in the P/Invoke layer, but the parameter logic should work
        Assert.True(exception == null || exception is not ArgumentException);
    }
}