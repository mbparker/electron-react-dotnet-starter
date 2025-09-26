using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Tests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityDeleterTests
{
    private EntityDeleter _deleter;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
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
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = Substitute.For<SqliteDbSchema>();
        _mockContext.Schema.Returns(mockSchema);
        _mockContext.Filename.Returns("test.db");

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);

        _connectionFactory = Substitute.For<Func<ISqliteConnection>>();
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();

        _connectionFactory.Invoke().Returns(_mockConnection);
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Delete, Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Delete, mockSchema, null, "DELETE FROM Test WHERE Id = :Id", null);
        _mockSynthesizer.Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);

        _deleter = new EntityDeleter(
            _connectionFactory,
            _synthesizerFactory,
            _mockParameterPopulator,
            _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Delete_WithPredicate_CallsSynthesizerAndExecutesCommand()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(2);

        // Act
        var result = _deleter.Delete(predicate);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockConnection.Received(1).Open("test.db", true);
        _mockSynthesizer.Received(1).Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), _mockParameters, Arg.Any<object>());
        _mockCommand.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Delete_WithNullPredicate_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _deleter.Delete<TestEntity>(null));
    }

    [Test]
    public void Delete_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _deleter.Delete(connection, predicate);

        // Assert
        Assert.That(result, Is.EqualTo(1));
        connection.DidNotReceive().Open(Arg.Any<string>(), Arg.Any<bool>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), parameters, Arg.Any<object>());
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Delete_WithConnectionAndNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var connection = Substitute.For<ISqliteConnection>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _deleter.Delete<TestEntity>(connection, null));
    }

    [Test]
    public void DeleteAll_WithoutPredicate_DeletesAllRecords()
    {
        // Arrange
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(5);

        // Act
        var result = _deleter.DeleteAll<TestEntity>();

        // Assert
        Assert.That(result, Is.EqualTo(5));
        _mockConnection.Received(1).Open("test.db", true);
        _mockSynthesizer.Received(1).Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>());
        _mockCommand.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void DeleteAll_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(3);

        // Act
        var result = _deleter.DeleteAll<TestEntity>(connection);

        // Assert
        Assert.That(result, Is.EqualTo(3));
        connection.DidNotReceive().Open(Arg.Any<string>(), Arg.Any<bool>());
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Delete_WhenNoRecordsAffected_ReturnsZero()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 999;
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(0);

        // Act
        var result = _deleter.Delete(predicate);

        // Assert
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_InitializesAllDependencies()
    {
        // Act & Assert - Constructor was called in SetUp
        Assert.That(_deleter, Is.Not.Null);
    }

    [Test]
    public void Delete_DisposesCommandAfterUse()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        _deleter.Delete(predicate);

        // Assert
        _mockCommand.Received(1).Dispose();
    }

    [Test]
    public void Delete_DisposesConnectionAfterUse()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        _deleter.Delete(predicate);

        // Assert
        _mockConnection.Received(1).Dispose();
    }

    [Test]
    public void Delete_WithComplexPredicate_HandlesCorrectly()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id > 10 && e.Name.Contains("test");
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(3);

        // Act
        var result = _deleter.Delete(predicate);

        // Assert
        Assert.That(result, Is.EqualTo(3));
        _mockSynthesizer.Received(1).Synthesize(typeof(TestEntity), Arg.Any<SqliteDmlSqlSynthesisArgs>());
    }

    [Test]
    public void DeleteAll_ForDifferentEntityType_UsesCorrectType()
    {
        // Arrange
        _mockCommand.ExecuteNonQuery(Arg.Any<string>()).Returns(2);

        // Act
        var result = _deleter.DeleteAll<string>();

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockSynthesizer.Received(1).Synthesize(typeof(string), Arg.Any<SqliteDmlSqlSynthesisArgs>());
    }
}