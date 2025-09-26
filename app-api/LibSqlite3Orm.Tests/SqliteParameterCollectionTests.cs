using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Concrete;
using Moq;

namespace LibSqlite3Orm.Tests;

public class SqliteParameterCollectionTests
{
    private readonly Mock<ISqliteParameter> _mockParameter;
    private readonly Mock<ISqliteValueConverterCache> _mockConverterCache;
    private readonly SqliteParameterCollection _collection;

    public SqliteParameterCollectionTests()
    {
        _mockParameter = new Mock<ISqliteParameter>();
        _mockConverterCache = new Mock<ISqliteValueConverterCache>();
        
        // Create a parameter factory that returns our mock
        Func<string, int, ISqliteParameter> parameterFactory = (name, index) =>
        {
            var mockParam = new Mock<ISqliteParameter>();
            mockParam.Setup(p => p.Name).Returns(name);
            mockParam.Setup(p => p.Index).Returns(index);
            return mockParam.Object;
        };

        _collection = new SqliteParameterCollection(parameterFactory);
    }

    [Fact]
    public void Count_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _collection.Count);
    }

    [Fact]
    public void Add_WithNameAndValue_IncreasesCount()
    {
        // Act
        _collection.Add("testParam", "testValue");

        // Assert
        Assert.Equal(1, _collection.Count);
    }

    [Fact]
    public void Add_WithNameAndValue_ReturnsParameter()
    {
        // Act
        var result = _collection.Add("testParam", "testValue");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testParam", result.Name);
        Assert.Equal(1, result.Index);
    }

    [Fact]
    public void Add_MultipleParameters_CorrectIndexAssignment()
    {
        // Act
        var param1 = _collection.Add("param1", "value1");
        var param2 = _collection.Add("param2", "value2");

        // Assert
        Assert.Equal(1, param1.Index);
        Assert.Equal(2, param2.Index);
        Assert.Equal(2, _collection.Count);
    }

    [Fact]
    public void IndexerByInt_ReturnsCorrectParameter()
    {
        // Arrange
        var param = _collection.Add("testParam", "testValue");

        // Act
        var result = _collection[0];

        // Assert
        Assert.Same(param, result);
    }

    [Fact]
    public void IndexerByName_ReturnsCorrectParameter()
    {
        // Arrange
        var param = _collection.Add("testParam", "testValue");

        // Act
        var result = _collection["testParam"];

        // Assert
        Assert.Same(param, result);
    }

    [Fact]
    public void IndexerByName_ReturnsNullForNonExistentName()
    {
        // Act
        var result = _collection["nonExistentParam"];

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Clear_RemovesAllParameters()
    {
        // Arrange
        _collection.Add("param1", "value1");
        _collection.Add("param2", "value2");
        Assert.Equal(2, _collection.Count);

        // Act
        _collection.Clear();

        // Assert
        Assert.Equal(0, _collection.Count);
    }

    [Fact]
    public void GetEnumerator_ReturnsAllParameters()
    {
        // Arrange
        var param1 = _collection.Add("param1", "value1");
        var param2 = _collection.Add("param2", "value2");

        // Act
        var parameters = _collection.ToList();

        // Assert
        Assert.Equal(2, parameters.Count);
        Assert.Contains(param1, parameters);
        Assert.Contains(param2, parameters);
    }

    [Fact]
    public void BindAll_CallsBindOnAllParameters()
    {
        // Arrange
        var mockParam1 = new Mock<ISqliteParameter>();
        var mockParam2 = new Mock<ISqliteParameter>();
        
        Func<string, int, ISqliteParameter> parameterFactory = (name, index) =>
            index == 1 ? mockParam1.Object : mockParam2.Object;

        var collection = new SqliteParameterCollection(parameterFactory);
        collection.Add("param1", "value1");
        collection.Add("param2", "value2");

        var statement = new IntPtr(123); // Mock statement handle

        // Act
        collection.BindAll(statement);

        // Assert
        mockParam1.Verify(p => p.Bind(statement), Times.Once);
        mockParam2.Verify(p => p.Bind(statement), Times.Once);
    }
}