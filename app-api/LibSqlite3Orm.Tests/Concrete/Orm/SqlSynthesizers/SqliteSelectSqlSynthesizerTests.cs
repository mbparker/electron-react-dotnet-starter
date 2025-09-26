using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.Orm;
using NSubstitute;

namespace LibSqlite3Orm.Tests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteSelectSqlSynthesizerTests
{
    private SqliteSelectSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaTable _testTable;
    private Func<SqliteDbSchema, ISqliteWhereClauseBuilder> _whereClauseBuilderFactory;
    private ISqliteWhereClauseBuilder _mockWhereClauseBuilder;

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        _schema = new SqliteDbSchema();
        _testTable = new SqliteDbSchemaTable
        {
            Name = "TestTable",
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName
        };

        _testTable.Columns.Add("Id", new SqliteDbSchemaTableColumn
        {
            Name = "Id",
            ModelFieldName = "Id",
            DbFieldTypeAffinity = SqliteColType.Integer
        });

        _testTable.Columns.Add("Name", new SqliteDbSchemaTableColumn
        {
            Name = "Name",
            ModelFieldName = "Name",
            DbFieldTypeAffinity = SqliteColType.Text
        });

        _testTable.Columns.Add("CreatedDate", new SqliteDbSchemaTableColumn
        {
            Name = "CreatedDate",
            ModelFieldName = "CreatedDate",
            DbFieldTypeAffinity = SqliteColType.Text
        });

        _schema.Tables.Add("TestTable", _testTable);

        _mockWhereClauseBuilder = Substitute.For<ISqliteWhereClauseBuilder>();
        _whereClauseBuilderFactory = Substitute.For<Func<SqliteDbSchema, ISqliteWhereClauseBuilder>>();
        _whereClauseBuilderFactory.Invoke(Arg.Any<SqliteDbSchema>()).Returns(_mockWhereClauseBuilder);

        _synthesizer = new SqliteSelectSqlSynthesizer(_schema, _whereClauseBuilderFactory);
    }

    [TearDown] 
    public void TearDown()
    {
        // No cleanup needed for NSubstitute mocks
    }

    private SqliteSortSpec CreateSortSpec<TKey>(Expression<Func<TestEntity, TKey>> keySelector, bool descending)
    {
        // Use reflection to create SqliteSortSpec with internal constructor
        var constructor = typeof(SqliteSortSpec).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance, 
            null, 
            new[] { typeof(Expression), typeof(bool) }, 
            null);
        
        return (SqliteSortSpec)constructor.Invoke(new object[] { keySelector, descending });
    }

    [Test]
    public void Synthesize_WithBasicSelectArgs_GeneratesCorrectSelectSql()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable"));
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Select));
        Assert.That(result.Schema, Is.EqualTo(_schema));
        Assert.That(result.Table, Is.EqualTo(_testTable));
    }

    [Test]
    public void Synthesize_WithFilterExpression_IncludesWhereClause()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> filterExpr = x => x.Id > 5;
        var args = new SynthesizeSelectSqlArgs(filterExpr, null, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        _mockWhereClauseBuilder.Build(typeof(TestEntity), filterExpr).Returns("Id > @p1");
        var extractedParams = new Dictionary<string, ExtractedParameter>
        {
            { "@p1", new ExtractedParameter("@p1", 5, "Id") }
        };
        _mockWhereClauseBuilder.ExtractedParameters.Returns(extractedParams);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable WHERE Id > @p1"));
        Assert.That(result.ExtractedParameters, Is.EqualTo(extractedParams));
        _whereClauseBuilderFactory.Received(1).Invoke(_schema);
        _mockWhereClauseBuilder.Received(1).Build(typeof(TestEntity), filterExpr);
    }

    [Test]
    public void Synthesize_WithSingleSortSpec_IncludesOrderByClause()
    {
        // Arrange
        Expression<Func<TestEntity, string>> keySelector = x => x.Name;
        var sortSpec = CreateSortSpec(keySelector, false);
        var sortSpecs = new List<SqliteSortSpec> { sortSpec };
        var args = new SynthesizeSelectSqlArgs(null, sortSpecs, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable ORDER BY Name ASC"));
    }

    [Test]
    public void Synthesize_WithMultipleSortSpecs_IncludesMultipleOrderByFields()
    {
        // Arrange
        Expression<Func<TestEntity, string>> keySelector1 = x => x.Name;
        Expression<Func<TestEntity, DateTime>> keySelector2 = x => x.CreatedDate;
        var sortSpec1 = CreateSortSpec(keySelector1, false);
        var sortSpec2 = CreateSortSpec(keySelector2, true);
        var sortSpecs = new List<SqliteSortSpec> { sortSpec1, sortSpec2 };
        var args = new SynthesizeSelectSqlArgs(null, sortSpecs, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable ORDER BY Name ASC, CreatedDate DESC"));
    }

    [Test]
    public void Synthesize_WithDescendingSortSpec_GeneratesDescendingOrder()
    {
        // Arrange
        Expression<Func<TestEntity, int>> keySelector = x => x.Id;
        var sortSpec = CreateSortSpec(keySelector, true);
        var sortSpecs = new List<SqliteSortSpec> { sortSpec };
        var args = new SynthesizeSelectSqlArgs(null, sortSpecs, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable ORDER BY Id DESC"));
    }

    [Test]
    public void Synthesize_WithTakeCount_IncludesLimitClause()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, null, 10);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable LIMIT 10"));
    }

    [Test]
    public void Synthesize_WithSkipCount_IncludesOffsetClause()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, 5, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable OFFSET 5"));
    }

    [Test]
    public void Synthesize_WithSkipAndTake_IncludesBothOffsetAndLimit()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, 5, 10);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable LIMIT 10 OFFSET 5"));
    }

    [Test]
    public void Synthesize_WithAllFeatures_GeneratesCompleteSelectSql()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> filterExpr = x => x.Id > 0;
        Expression<Func<TestEntity, string>> keySelector = x => x.Name;
        var sortSpec = CreateSortSpec(keySelector, true);
        var sortSpecs = new List<SqliteSortSpec> { sortSpec };
        var args = new SynthesizeSelectSqlArgs(filterExpr, sortSpecs, 10, 20);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        _mockWhereClauseBuilder.Build(typeof(TestEntity), filterExpr).Returns("Id > @p1");
        var extractedParams = new Dictionary<string, ExtractedParameter>
        {
            { "@p1", new ExtractedParameter("@p1", 0, "Id") }
        };
        _mockWhereClauseBuilder.ExtractedParameters.Returns(extractedParams);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable WHERE Id > @p1 ORDER BY Name DESC LIMIT 20 OFFSET 10"));
        Assert.That(result.ExtractedParameters, Is.EqualTo(extractedParams));
    }

    [Test]
    public void Synthesize_WithUnmappedEntityType_ThrowsInvalidDataContractException()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act & Assert
        var ex = Assert.Throws<InvalidDataContractException>(() => _synthesizer.Synthesize(typeof(string), sqlArgs));
        Assert.That(ex.Message, Does.Contain("is not mapped in the schema"));
    }

    [Test]
    public void Synthesize_WithEmptySortSpecs_DoesNotIncludeOrderBy()
    {
        // Arrange
        var emptySortSpecs = new List<SqliteSortSpec>();
        var args = new SynthesizeSelectSqlArgs(null, emptySortSpecs, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable"));
    }

    [Test]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Act & Assert
        Assert.That(_synthesizer, Is.Not.Null);
        Assert.DoesNotThrow(() => new SqliteSelectSqlSynthesizer(_schema, _whereClauseBuilderFactory));
    }

    [Test]
    public void Synthesize_WithNullFilterExpression_DoesNotCallWhereClauseBuilder()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        _whereClauseBuilderFactory.DidNotReceive().Invoke(Arg.Any<SqliteDbSchema>());
    }

    [Test]
    public void Synthesize_ColumnsOrderedAlphabetically_ReturnsConsistentColumnOrder()
    {
        // Arrange - Columns are already ordered alphabetically in setup: CreatedDate, Id, Name
        var args = new SynthesizeSelectSqlArgs(null, null, null, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Does.StartWith("SELECT CreatedDate, Id, Name"));
    }

    [Test]
    public void Synthesize_WithZeroTakeCount_IncludesLimitZero()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, null, 0);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable LIMIT 0"));
    }

    [Test]
    public void Synthesize_WithZeroSkipCount_IncludesOffsetZero()
    {
        // Arrange
        var args = new SynthesizeSelectSqlArgs(null, null, 0, null);
        var sqlArgs = new SqliteDmlSqlSynthesisArgs(args);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), sqlArgs);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("SELECT CreatedDate, Id, Name FROM TestTable OFFSET 0"));
    }
}