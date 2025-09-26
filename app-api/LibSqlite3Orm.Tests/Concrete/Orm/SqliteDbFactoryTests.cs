using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Tests.Concrete.Orm;

[TestFixture]
public class SqliteDbFactoryTests
{
    private SqliteDbFactory _dbFactory;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteDdlSqlSynthesizer _mockSynthesizer;
    private Func<ISqliteConnection> _connectionFactory;
    private Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> _synthesizerFactory;
    private string _testDbPath;

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockCommand = Substitute.For<ISqliteCommand>();
        _mockSynthesizer = Substitute.For<ISqliteDdlSqlSynthesizer>();
        
        _connectionFactory = Substitute.For<Func<ISqliteConnection>>();
        _synthesizerFactory = Substitute.For<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>();

        _connectionFactory.Invoke().Returns(_mockConnection);
        _mockConnection.CreateCommand().Returns(_mockCommand);
        _synthesizerFactory.Invoke(Arg.Any<SqliteDdlSqlSynthesisKind>(), Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        _dbFactory = new SqliteDbFactory(_connectionFactory, _synthesizerFactory);
        
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }
        
        // Dispose mocks to satisfy NUnit analyzer
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => new SqliteDbFactory(_connectionFactory, _synthesizerFactory));
    }

    [Test]
    public void Constructor_WithNullConnectionFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SqliteDbFactory(null, _synthesizerFactory));
    }

    [Test]
    public void Constructor_WithNullSynthesizerFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SqliteDbFactory(_connectionFactory, null));
    }

    [Test]
    public void Create_WithValidParameters_CallsConnectionFactory()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _connectionFactory.Received(1).Invoke();
    }

    [Test]
    public void Create_WithValidParameters_OpensConnection()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _mockConnection.Received(1).Open(_testDbPath, false);
    }

    [Test]
    public void Create_WithValidParameters_CreatesCommand()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _mockConnection.Received(1).CreateCommand();
    }

    [Test]
    public void Create_WithValidParameters_ExecutesSqlCommand()
    {
        // Arrange
        var schema = CreateTestSchema();
        var expectedSql = "CREATE TABLE test (id INTEGER);";
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns(expectedSql);

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _mockCommand.Received(1).ExecuteNonQuery(Arg.Is<string>(sql => sql.Contains(expectedSql)));
    }

    [Test]
    public void Create_WithValidParameters_DisposesResources()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _mockCommand.Received(1).Dispose();
        _mockConnection.Received(1).Dispose();
    }

    [Test]
    public void Create_WithSchemaContainingMultipleTables_CallsSynthesizeCreateForEachTable()
    {
        // Arrange
        var schema = new SqliteDbSchema();
        var table1 = new SqliteDbSchemaTable { Name = "Table1" };
        var table2 = new SqliteDbSchemaTable { Name = "Table2" };
        schema.Tables.Add("Table1", table1);
        schema.Tables.Add("Table2", table2);

        _mockSynthesizer.SynthesizeCreate("Table1").Returns("CREATE TABLE Table1 (id INTEGER);");
        _mockSynthesizer.SynthesizeCreate("Table2").Returns("CREATE TABLE Table2 (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _mockSynthesizer.Received(1).SynthesizeCreate("Table1");
        _mockSynthesizer.Received(1).SynthesizeCreate("Table2");
    }

    [Test]
    public void Create_WithSchemaContainingIndexes_CallsSynthesizeCreateForEachIndex()
    {
        // Arrange
        var schema = new SqliteDbSchema();
        var table = new SqliteDbSchemaTable { Name = "TestTable" };
        schema.Tables.Add("TestTable", table);
        schema.Indexes.Add("Index1", new SqliteDbSchemaIndex { IndexName = "Index1" });
        schema.Indexes.Add("Index2", new SqliteDbSchemaIndex { IndexName = "Index2" });

        _mockSynthesizer.SynthesizeCreate("TestTable").Returns("CREATE TABLE TestTable (id INTEGER);");
        
        // Set up index synthesizer
        var indexSynthesizer = Substitute.For<ISqliteDdlSqlSynthesizer>();
        _synthesizerFactory.Invoke(SqliteDdlSqlSynthesisKind.IndexOps, schema).Returns(indexSynthesizer);
        indexSynthesizer.SynthesizeCreate("Index1").Returns("CREATE INDEX Index1 ON TestTable (column);");
        indexSynthesizer.SynthesizeCreate("Index2").Returns("CREATE INDEX Index2 ON TestTable (column);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        indexSynthesizer.Received(1).SynthesizeCreate("Index1");
        indexSynthesizer.Received(1).SynthesizeCreate("Index2");
    }

    [Test]
    public void Create_WithNullSchema_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dbFactory.Create(null, _testDbPath, false));
    }

    [Test]
    public void Create_WithNullDbFilename_ThrowsArgumentNullException()
    {
        // Arrange
        var schema = CreateTestSchema();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dbFactory.Create(schema, null, false));
    }

    [Test]
    public void Create_WithEmptyDbFilename_ThrowsArgumentException()
    {
        // Arrange
        var schema = CreateTestSchema();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _dbFactory.Create(schema, "", false));
    }

    [Test]
    public void Create_CallsSynthesizeFactoryWithCorrectKinds()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _testDbPath, false);

        // Assert
        _synthesizerFactory.Received().Invoke(SqliteDdlSqlSynthesisKind.TableOps, schema);
    }

    private SqliteDbSchema CreateTestSchema()
    {
        var schema = new SqliteDbSchema();
        var table = new SqliteDbSchemaTable { Name = "TestTable" };
        schema.Tables.Add("TestTable", table);
        return schema;
    }
}