using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Tests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityCreatorTests
{
    private EntityCreator _creator;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
    private ISqliteEntityWriter _mockEntityWriter;
    private ISqliteOrmDatabaseContext _mockContext;
    private Func<ISqliteConnection> _connectionFactory;
    private Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> _synthesizerFactory;

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockCommand = Substitute.For<ISqliteCommand>();
        _mockParameters = Substitute.For<ISqliteParameterCollection>();
        _mockSynthesizer = Substitute.For<ISqliteDmlSqlSynthesizer>();
        _mockParameterPopulator = Substitute.For<ISqliteParameterPopulator>();
        _mockEntityWriter = Substitute.For<ISqliteEntityWriter>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = Substitute.For<SqliteDbSchema>();
        _mockContext.Schema.Returns(mockSchema);
        _mockContext.Filename.Returns("test.db");

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);

        _connectionFactory = Substitute.For<Func<ISqliteConnection>>();
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();

        _connectionFactory.Invoke().Returns(_mockConnection);
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Insert, Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, mockSchema, null, "INSERT INTO Test VALUES (1)", null);
        _mockSynthesizer.Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);

        _creator = new EntityCreator(
            _connectionFactory,
            _synthesizerFactory,
            _mockParameterPopulator,
            _mockEntityWriter,
            _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Insert_WithEntity_CallsSynthesizerAndExecutesCommand()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.Insert(entity);

        // Assert
        Assert.That(result, Is.True);
        _mockConnection.Received(1).Open("test.db", true);
        _mockSynthesizer.Received(1).Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), _mockParameters, entity);
        _mockCommand.Received(1).ExecuteNonQuery(Arg.Any<string>());
        _mockEntityWriter.Received(1).SetGeneratedKeyOnEntityIfNeeded(Arg.Any<SqliteDbSchema>(), _mockConnection, entity);
    }

    [Test]
    public void Insert_WhenCommandReturnsZero_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(0);

        // Act
        var result = _creator.Insert(entity);

        // Assert
        Assert.That(result, Is.False);
        _mockEntityWriter.DidNotReceive().SetGeneratedKeyOnEntityIfNeeded(Arg.Any<SqliteDbSchema>(), Arg.Any<ISqliteConnection>(), Arg.Any<object>());
    }

    [Test]
    public void Insert_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.Insert(connection, entity);

        // Assert
        Assert.That(result, Is.True);
        connection.DidNotReceive().Open(Arg.Any<string>(), Arg.Any<bool>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), parameters, entity);
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Insert_WithConnectionAndSynthesisResult_UsesProvidedResult()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, _mockContext.Schema, null, "INSERT INTO Test VALUES (:Name)", null);
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery("INSERT INTO Test VALUES (:Name)").Returns(1);

        // Act
        var result = _creator.Insert(connection, synthesisResult, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockParameterPopulator.Received(1).Populate(synthesisResult, parameters, entity);
        command.Received(1).ExecuteNonQuery("INSERT INTO Test VALUES (:Name)");
        _mockEntityWriter.Received(1).SetGeneratedKeyOnEntityIfNeeded(_mockContext.Schema, connection, entity);
    }

    [Test]
    public void InsertMany_WithEntities_InsertsAllEntities()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.InsertMany(entities);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockConnection.Received(1).Open("test.db", true);
        _mockCommand.Received(2).ExecuteNonQuery(Arg.Any<string>());
        _mockEntityWriter.Received(2).SetGeneratedKeyOnEntityIfNeeded(Arg.Any<SqliteDbSchema>(), _mockConnection, Arg.Any<TestEntity>());
    }

    [Test]
    public void InsertMany_WithConnection_UsesProvidedConnection()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.InsertMany(connection, entities);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        connection.DidNotReceive().Open(Arg.Any<string>(), Arg.Any<bool>());
        command.Received(2).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void InsertMany_WithPartialFailures_ReturnsSuccessCount()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" },
            new TestEntity { Id = 3, Name = "Test3" }
        };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1, 0, 1); // Success, Failure, Success

        // Act
        var result = _creator.InsertMany(entities);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockCommand.Received(3).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Constructor_InitializesAllDependencies()
    {
        // Act & Assert - Constructor was called in SetUp
        Assert.That(_creator, Is.Not.Null);
        
        // Verify synthesizer factory was called during setup for synthesis
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);
        _creator.Insert(entity);
        
        _synthesizerFactory.Received().Invoke(SqliteDmlSqlSynthesisKind.Insert, Arg.Any<SqliteDbSchema>());
    }

    [Test]
    public void Insert_DisposesCommandAfterUse()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        _creator.Insert(entity);

        // Assert
        _mockCommand.Received(1).Dispose();
    }

    [Test]
    public void Insert_DisposesConnectionAfterUse()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        _creator.Insert(entity);

        // Assert
        _mockConnection.Received(1).Dispose();
    }
}