using Autofac;
using Autofac.Core;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;
using LibSqlite3Orm.Types.Orm.FieldConverters;
using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.Tests;

// Test context for testing generic registrations
public class TestOrmContext : SqliteOrmDatabaseContext
{
    public TestOrmContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory)
        : base(schemaBuilderFactory)
    {
    }

    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        // Empty test schema
    }
}

[TestFixture]
public class ContainerModuleTests
{
    private IContainer _container;

    [SetUp]
    public void SetUp()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        _container = builder.Build();
    }

    [TearDown]
    public void TearDown()
    {
        _container?.Dispose();
    }

    [Test]
    public void Load_RegistersAllRequiredServices()
    {
        // Assert - Test that all major services can be resolved
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteConnection>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteTransaction>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteCommand>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteParameter>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteFieldValueSerialization>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteParameterCollection>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteParameterPopulator>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteEntityWriter>());
    }

    [Test]
    public void Load_RegistersEntityServices()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<IEntityCreator>());
        Assert.DoesNotThrow(() => _container.Resolve<IEntityUpdater>());
        Assert.DoesNotThrow(() => _container.Resolve<IEntityUpserter>());
        Assert.DoesNotThrow(() => _container.Resolve<IEntityGetter>());
        Assert.DoesNotThrow(() => _container.Resolve<IEntityDeleter>());
        Assert.DoesNotThrow(() => _container.Resolve<IEntityServices>());
    }

    [Test]
    public void Load_RegistersOrmServices()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<SqliteDbSchemaBuilder>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteObjectRelationalMapping<TestOrmContext>>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteSchemaObjectRelationalMapping<TestOrmContext>>());
        Assert.DoesNotThrow(() => _container.Resolve<SqliteOrmSchemaContext>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteFileOperations>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteUniqueIdGenerator>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteDbFactory>());
    }

    [Test]
    public void Load_RegistersDataServices()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteDataRow>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteDataColumn>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteDataReader>());
        Assert.DoesNotThrow(() => _container.Resolve<IOrmGenerativeLogicTracer>());
    }

    [Test]
    public void Load_RegistersFieldSerializers()
    {
        // Act
        var serializers = _container.Resolve<IEnumerable<ISqliteFieldSerializer>>();

        // Assert
        Assert.That(serializers, Is.Not.Null);
        Assert.That(serializers.Count(), Is.GreaterThan(0));
        
        // Check specific serializers are registered
        Assert.That(serializers.Any(s => s.GetType().Name.Contains("DateOnly")), Is.True);
        Assert.That(serializers.Any(s => s.GetType().Name.Contains("DateTime")), Is.True);
        Assert.That(serializers.Any(s => s.GetType().Name.Contains("Decimal")), Is.True);
        Assert.That(serializers.Any(s => s.GetType().Name.Contains("Guid")), Is.True);
        Assert.That(serializers.Any(s => s.GetType().Name.Contains("Boolean")), Is.True);
    }

    [Test]
    public void Load_RegistersFieldConverters()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteFieldConversion>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteFailoverFieldConverter>());
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteEnumFieldSerializer>());
    }

    [Test]
    public void Load_RegistersSqlSynthesizers_AsKeyedServices()
    {
        // Assert - Test that SQL synthesizers are registered as keyed services
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.TableOps));
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.IndexOps));
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Insert));
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Update));
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Delete));
        Assert.DoesNotThrow(() => _container.ResolveKeyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Select));
    }

    [Test]
    public void Load_RegistersSqlSynthesizerFactories()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>());
        Assert.DoesNotThrow(() => _container.Resolve<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>());
    }

    [Test]
    public void Load_RegistersWhereClauseBuilder()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteWhereClauseBuilder>());
    }

    [Test]
    public void Load_RegistersDbSchemaMigrator()
    {
        // Assert
        Assert.DoesNotThrow(() => _container.Resolve<ISqliteDbSchemaMigrator<TestOrmContext>>());
    }

    [Test]
    public void DdlSqlSynthesizerFactory_ResolvesCorrectSynthesizer()
    {
        // Arrange
        var factory = _container.Resolve<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>();
        var schema = Substitute.For<SqliteDbSchema>();

        // Act & Assert
        var tableSynthesizer = factory(SqliteDdlSqlSynthesisKind.TableOps, schema);
        Assert.That(tableSynthesizer, Is.Not.Null);
        Assert.That(tableSynthesizer.GetType().Name, Is.EqualTo("SqliteTableSqlSynthesizer"));

        var indexSynthesizer = factory(SqliteDdlSqlSynthesisKind.IndexOps, schema);
        Assert.That(indexSynthesizer, Is.Not.Null);
        Assert.That(indexSynthesizer.GetType().Name, Is.EqualTo("SqliteIndexSqlSynthesizer"));
    }

    [Test]
    public void DmlSqlSynthesizerFactory_ResolvesCorrectSynthesizer()
    {
        // Arrange
        var factory = _container.Resolve<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();
        var schema = Substitute.For<SqliteDbSchema>();

        // Act & Assert
        var insertSynthesizer = factory(SqliteDmlSqlSynthesisKind.Insert, schema);
        Assert.That(insertSynthesizer, Is.Not.Null);
        Assert.That(insertSynthesizer.GetType().Name, Is.EqualTo("SqliteInsertSqlSynthesizer"));

        var updateSynthesizer = factory(SqliteDmlSqlSynthesisKind.Update, schema);
        Assert.That(updateSynthesizer, Is.Not.Null);
        Assert.That(updateSynthesizer.GetType().Name, Is.EqualTo("SqliteUpdateSqlSynthesizer"));

        var deleteSynthesizer = factory(SqliteDmlSqlSynthesisKind.Delete, schema);
        Assert.That(deleteSynthesizer, Is.Not.Null);
        Assert.That(deleteSynthesizer.GetType().Name, Is.EqualTo("SqliteDeleteSqlSynthesizer"));

        var selectSynthesizer = factory(SqliteDmlSqlSynthesisKind.Select, schema);
        Assert.That(selectSynthesizer, Is.Not.Null);
        Assert.That(selectSynthesizer.GetType().Name, Is.EqualTo("SqliteSelectSqlSynthesizer"));
    }

    [Test]
    public void DependencyInjection_AllRegisteredServicesCanBeResolved()
    {
        // This test ensures that key registered services can be resolved
        // without circular dependencies or missing dependencies

        var resolvedServices = new List<object>();
        var failures = new List<string>();

        var servicesToTest = new[]
        {
            typeof(ISqliteConnection),
            typeof(ISqliteFileOperations),
            typeof(ISqliteDbFactory),
            typeof(IEntityServices),
            typeof(ISqliteParameterPopulator),
            typeof(ISqliteEntityWriter),
            typeof(SqliteDbSchemaBuilder)
        };

        foreach (var serviceType in servicesToTest)
        {
            try
            {
                var resolved = _container.Resolve(serviceType);
                resolvedServices.Add(resolved);
            }
            catch (Exception ex)
            {
                failures.Add($"Failed to resolve {serviceType.Name}: {ex.Message}");
            }
        }

        // Assert that we resolved many services and had no failures
        Assert.That(resolvedServices.Count, Is.GreaterThan(5), "Should resolve multiple services");
        Assert.That(failures, Is.Empty, $"Resolution failures: {string.Join(", ", failures)}");
    }

    [Test]
    public void Load_ServicesHaveCorrectLifetimes()
    {
        // Test SingleInstance services return the same instance
        var dbFactory1 = _container.Resolve<ISqliteDbFactory>();
        var dbFactory2 = _container.Resolve<ISqliteDbFactory>();
        Assert.That(dbFactory2, Is.SameAs(dbFactory1), "SqliteDbFactory should be SingleInstance");

        var fileOps1 = _container.Resolve<ISqliteFileOperations>();
        var fileOps2 = _container.Resolve<ISqliteFileOperations>();
        Assert.That(fileOps2, Is.SameAs(fileOps1), "SqliteFileOperations should be SingleInstance");

        // Test InstancePerDependency services return different instances
        var connection1 = _container.Resolve<ISqliteConnection>();
        var connection2 = _container.Resolve<ISqliteConnection>();
        Assert.That(connection2, Is.Not.SameAs(connection1), "SqliteConnection should be InstancePerDependency");
    }
}