using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.Tests.Concrete.Orm;

[TestFixture]
public class OrmGenerativeLogicTracerTests
{
    private OrmGenerativeLogicTracer _tracer;
    private List<GenerativeLogicTraceEventArgs> _sqlStatementEvents;
    private List<GenerativeLogicTraceEventArgs> _whereClauseEvents;

    [SetUp]
    public void SetUp()
    {
        _tracer = new OrmGenerativeLogicTracer();
        _sqlStatementEvents = new List<GenerativeLogicTraceEventArgs>();
        _whereClauseEvents = new List<GenerativeLogicTraceEventArgs>();

        _tracer.SqlStatementExecuting += (sender, args) => _sqlStatementEvents.Add(args);
        _tracer.WhereClauseBuilderVisit += (sender, args) => _whereClauseEvents.Add(args);
    }

    [Test]
    public void NotifySqlStatementExecuting_WithSqlStatement_RaisesEvent()
    {
        // Arrange
        var sqlStatement = "SELECT * FROM Users";

        // Act
        _tracer.NotifySqlStatementExecuting(sqlStatement);

        // Assert
        Assert.That(_sqlStatementEvents.Count, Is.EqualTo(1));
        Assert.That(_sqlStatementEvents[0].Message, Is.EqualTo(sqlStatement));
    }

    [Test]
    public void NotifySqlStatementExecuting_WithNullStatement_RaisesEventWithNull()
    {
        // Act
        _tracer.NotifySqlStatementExecuting(null);

        // Assert
        Assert.That(_sqlStatementEvents.Count, Is.EqualTo(1));
        Assert.That(_sqlStatementEvents[0].Message, Is.Null);
    }

    [Test]
    public void NotifySqlStatementExecuting_WithEmptyStatement_RaisesEventWithEmptyString()
    {
        // Act
        _tracer.NotifySqlStatementExecuting("");

        // Assert
        Assert.That(_sqlStatementEvents.Count, Is.EqualTo(1));
        Assert.That(_sqlStatementEvents[0].Message, Is.EqualTo(""));
    }

    [Test]
    public void NotifyWhereClauseBuilderVisit_WithMessage_RaisesEvent()
    {
        // Arrange
        var message = "Processing WHERE condition";

        // Act
        _tracer.NotifyWhereClauseBuilderVisit(message);

        // Assert
        Assert.That(_whereClauseEvents.Count, Is.EqualTo(1));
        Assert.That(_whereClauseEvents[0].Message, Is.EqualTo(message));
    }

    [Test]
    public void NotifyWhereClauseBuilderVisit_WithNullMessage_RaisesEventWithNull()
    {
        // Act
        _tracer.NotifyWhereClauseBuilderVisit(null);

        // Assert
        Assert.That(_whereClauseEvents.Count, Is.EqualTo(1));
        Assert.That(_whereClauseEvents[0].Message, Is.Null);
    }

    [Test]
    public void NotifyWhereClauseBuilderVisit_WithEmptyMessage_RaisesEventWithEmptyString()
    {
        // Act
        _tracer.NotifyWhereClauseBuilderVisit("");

        // Assert
        Assert.That(_whereClauseEvents.Count, Is.EqualTo(1));
        Assert.That(_whereClauseEvents[0].Message, Is.EqualTo(""));
    }

    [Test]
    public void MultipleNotifications_RaiseMultipleEvents()
    {
        // Act
        _tracer.NotifySqlStatementExecuting("SQL 1");
        _tracer.NotifySqlStatementExecuting("SQL 2");
        _tracer.NotifyWhereClauseBuilderVisit("WHERE 1");
        _tracer.NotifyWhereClauseBuilderVisit("WHERE 2");

        // Assert
        Assert.That(_sqlStatementEvents.Count, Is.EqualTo(2));
        Assert.That(_whereClauseEvents.Count, Is.EqualTo(2));
        
        Assert.That(_sqlStatementEvents[0].Message, Is.EqualTo("SQL 1"));
        Assert.That(_sqlStatementEvents[1].Message, Is.EqualTo("SQL 2"));
        Assert.That(_whereClauseEvents[0].Message, Is.EqualTo("WHERE 1"));
        Assert.That(_whereClauseEvents[1].Message, Is.EqualTo("WHERE 2"));
    }

    [Test]
    public void Events_WithoutSubscribers_DoNotThrow()
    {
        // Arrange
        var tracerWithoutSubscribers = new OrmGenerativeLogicTracer();

        // Act & Assert
        Assert.DoesNotThrow(() => tracerWithoutSubscribers.NotifySqlStatementExecuting("SQL"));
        Assert.DoesNotThrow(() => tracerWithoutSubscribers.NotifyWhereClauseBuilderVisit("WHERE"));
    }

    [Test]
    public void EventArgs_SenderIsTracer()
    {
        // Arrange
        object capturedSender = null;
        _tracer.SqlStatementExecuting += (sender, args) => capturedSender = sender;

        // Act
        _tracer.NotifySqlStatementExecuting("Test SQL");

        // Assert
        Assert.That(capturedSender, Is.SameAs(_tracer));
    }

    [Test]
    public void MultipleSusbscribers_AllReceiveEvents()
    {
        // Arrange
        var events1 = new List<string>();
        var events2 = new List<string>();

        _tracer.SqlStatementExecuting += (sender, args) => events1.Add(args.Message);
        _tracer.SqlStatementExecuting += (sender, args) => events2.Add(args.Message);

        // Act
        _tracer.NotifySqlStatementExecuting("Test SQL");

        // Assert
        Assert.That(events1.Count, Is.EqualTo(1));
        Assert.That(events2.Count, Is.EqualTo(1));
        Assert.That(events1[0], Is.EqualTo("Test SQL"));
        Assert.That(events2[0], Is.EqualTo("Test SQL"));
    }
}