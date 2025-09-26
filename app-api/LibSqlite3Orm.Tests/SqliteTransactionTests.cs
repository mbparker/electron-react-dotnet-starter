using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Concrete;
using Moq;

namespace LibSqlite3Orm.Tests;

public class SqliteTransactionTests
{
    private readonly Mock<ISqliteConnection> _mockConnection;
    private readonly Mock<ISqliteUniqueIdGenerator> _mockIdGenerator;
    private readonly Mock<ISqliteCommand> _mockCommand;

    public SqliteTransactionTests()
    {
        _mockConnection = new Mock<ISqliteConnection>();
        _mockIdGenerator = new Mock<ISqliteUniqueIdGenerator>();
        _mockCommand = new Mock<ISqliteCommand>();

        _mockConnection.Setup(c => c.CreateCommand()).Returns(_mockCommand.Object);
        _mockIdGenerator.Setup(g => g.NewUniqueId()).Returns("test-transaction-id");
    }

    [Fact]
    public void Constructor_CreatesTransactionWithUniqueId()
    {
        // Act
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);

        // Assert
        Assert.Equal("test-transaction-id", transaction.Name);
        Assert.Same(_mockConnection.Object, transaction.Connection);
    }

    [Fact]
    public void Constructor_ExecutesSavepointCommand()
    {
        // Act
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);

        // Assert
        _mockConnection.Verify(c => c.CreateCommand(), Times.Once);
        _mockCommand.Verify(c => c.ExecuteNonQuery("SAVEPOINT 'test-transaction-id';"), Times.Once);
        _mockCommand.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Commit_ExecutesReleaseSavepointCommand()
    {
        // Arrange - Use fresh mocks to avoid constructor interference
        var mockConnection = new Mock<ISqliteConnection>();
        var mockIdGenerator = new Mock<ISqliteUniqueIdGenerator>();
        var constructorCommand = new Mock<ISqliteCommand>();
        var commitCommand = new Mock<ISqliteCommand>();

        // Constructor will use constructorCommand, Commit will use commitCommand
        mockConnection.SetupSequence(c => c.CreateCommand())
            .Returns(constructorCommand.Object)
            .Returns(commitCommand.Object);
        mockIdGenerator.Setup(g => g.NewUniqueId()).Returns("test-transaction-id");
        
        var transaction = new SqliteTransaction(mockConnection.Object, mockIdGenerator.Object);

        // Act
        transaction.Commit();

        // Assert - Verify only the commit command was called
        mockConnection.Verify(c => c.CreateCommand(), Times.Exactly(2)); // Constructor + Commit
        commitCommand.Verify(c => c.ExecuteNonQuery("RELEASE SAVEPOINT 'test-transaction-id';"), Times.Once);
        commitCommand.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Commit_SetsConnectionToNull()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);

        // Act
        transaction.Commit();

        // Assert
        Assert.Null(transaction.Connection);
    }

    [Fact]
    public void Commit_RaisesCommittedEvent()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);
        var eventRaised = false;
        transaction.Committed += (sender, args) => eventRaised = true;

        // Act
        transaction.Commit();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Commit_ThrowsWhenConnectionIsNull()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);
        transaction.Commit(); // First commit sets Connection to null

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Commit());
        Assert.Equal("Transaction has already been disposed.", exception.Message);
    }

    [Fact]
    public void Rollback_ExecutesRollbackCommand()
    {
        // Arrange - Use fresh mocks to avoid constructor interference
        var mockConnection = new Mock<ISqliteConnection>();
        var mockIdGenerator = new Mock<ISqliteUniqueIdGenerator>();
        var constructorCommand = new Mock<ISqliteCommand>();
        var rollbackCommand = new Mock<ISqliteCommand>();

        // Constructor will use constructorCommand, Rollback will use rollbackCommand
        mockConnection.SetupSequence(c => c.CreateCommand())
            .Returns(constructorCommand.Object)
            .Returns(rollbackCommand.Object);
        mockIdGenerator.Setup(g => g.NewUniqueId()).Returns("test-transaction-id");
        
        var transaction = new SqliteTransaction(mockConnection.Object, mockIdGenerator.Object);

        // Act
        transaction.Rollback();

        // Assert - Verify only the rollback command was called
        mockConnection.Verify(c => c.CreateCommand(), Times.Exactly(2)); // Constructor + Rollback
        rollbackCommand.Verify(c => c.ExecuteNonQuery("ROLLBACK TRANSACTION TO SAVEPOINT 'test-transaction-id';"), Times.Once);
        rollbackCommand.Verify(c => c.Dispose(), Times.Once);
    }

    [Fact]
    public void Rollback_SetsConnectionToNull()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);

        // Act
        transaction.Rollback();

        // Assert
        Assert.Null(transaction.Connection);
    }

    [Fact]
    public void Rollback_RaisesRolledBackEvent()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);
        var eventRaised = false;
        transaction.RolledBack += (sender, args) => eventRaised = true;

        // Act
        transaction.Rollback();

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Rollback_ThrowsWhenConnectionIsNull()
    {
        // Arrange
        var transaction = new SqliteTransaction(_mockConnection.Object, _mockIdGenerator.Object);
        transaction.Rollback(); // First rollback sets Connection to null

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => transaction.Rollback());
        Assert.Equal("Transaction has already been disposed.", exception.Message);
    }

    [Fact]
    public void Dispose_CallsRollbackWhenConnectionNotNull()
    {
        // Arrange - Use fresh mocks to avoid constructor interference
        var mockConnection = new Mock<ISqliteConnection>();
        var mockIdGenerator = new Mock<ISqliteUniqueIdGenerator>();
        var constructorCommand = new Mock<ISqliteCommand>();
        var disposeCommand = new Mock<ISqliteCommand>();

        // Constructor will use constructorCommand, Dispose will use disposeCommand
        mockConnection.SetupSequence(c => c.CreateCommand())
            .Returns(constructorCommand.Object)
            .Returns(disposeCommand.Object);
        mockIdGenerator.Setup(g => g.NewUniqueId()).Returns("test-transaction-id");
        
        var transaction = new SqliteTransaction(mockConnection.Object, mockIdGenerator.Object);

        // Act
        transaction.Dispose();

        // Assert - Verify only the dispose command was called
        mockConnection.Verify(c => c.CreateCommand(), Times.Exactly(2)); // Constructor + Dispose
        disposeCommand.Verify(c => c.ExecuteNonQuery("ROLLBACK TRANSACTION TO SAVEPOINT 'test-transaction-id';"), Times.Once);
    }

    [Fact]
    public void Dispose_DoesNotRollbackWhenConnectionIsNull()
    {
        // Arrange - Use fresh mocks
        var mockConnection = new Mock<ISqliteConnection>();
        var mockIdGenerator = new Mock<ISqliteUniqueIdGenerator>();
        var constructorCommand = new Mock<ISqliteCommand>();
        var commitCommand = new Mock<ISqliteCommand>();

        // Constructor will use constructorCommand, Commit will use commitCommand
        mockConnection.SetupSequence(c => c.CreateCommand())
            .Returns(constructorCommand.Object)
            .Returns(commitCommand.Object);
        mockIdGenerator.Setup(g => g.NewUniqueId()).Returns("test-transaction-id");
        
        var transaction = new SqliteTransaction(mockConnection.Object, mockIdGenerator.Object);
        transaction.Commit(); // Sets Connection to null
        
        // Reset to only track calls after commit
        mockConnection.Reset();

        // Act
        transaction.Dispose();

        // Assert
        mockConnection.Verify(c => c.CreateCommand(), Times.Never);
    }
}