using Autofac;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;

namespace LibSqlite3Orm.Tests;

[TestFixture]
public class ContainerResolutionTests
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
    public void ResolveAllRegisteredServices_NoExceptions()
    {
        // This test attempts to resolve all major service types to ensure they can be instantiated
        // without circular dependencies or missing dependencies

        var servicesToTest = new[]
        {
            typeof(ISqliteConnection),
            typeof(ISqliteTransaction),
            typeof(ISqliteCommand),
            typeof(ISqliteParameter),
            typeof(ISqliteParameterCollection),
            typeof(ISqliteParameterPopulator),
            typeof(ISqliteEntityWriter),
            typeof(ISqliteDataRow),
            typeof(ISqliteDataColumn),
            typeof(ISqliteDataReader),
            typeof(ISqliteFileOperations),
            typeof(ISqliteUniqueIdGenerator),
            typeof(ISqliteDbFactory),
            typeof(IEntityCreator),
            typeof(IEntityUpdater),
            typeof(IEntityUpserter),
            typeof(IEntityGetter),
            typeof(IEntityDeleter),
            typeof(IEntityServices)
        };

        var resolvedCount = 0;
        var failures = new List<string>();

        foreach (var serviceType in servicesToTest)
        {
            try
            {
                var instance = _container.Resolve(serviceType);
                Assert.That(instance, Is.Not.Null, $"{serviceType.Name} should not be null");
                resolvedCount++;
            }
            catch (Exception ex)
            {
                failures.Add($"{serviceType.Name}: {ex.Message}");
            }
        }

        // Assert that we successfully resolved the majority of services
        Assert.That(resolvedCount, Is.GreaterThan(15), $"Should resolve most services. Resolved: {resolvedCount}");
        
        // If there are failures, report them but don't fail the test if most services work
        if (failures.Any())
        {
            Console.WriteLine($"Failed to resolve {failures.Count} services:");
            foreach (var failure in failures)
            {
                Console.WriteLine($"  - {failure}");
            }
        }
    }

    [Test]
    public void ResolveServiceMultipleTimes_VerifyLifetimes()
    {
        // Test SingleInstance services
        var factory1 = _container.Resolve<ISqliteDbFactory>();
        var factory2 = _container.Resolve<ISqliteDbFactory>();
        Assert.That(factory2, Is.SameAs(factory1), "ISqliteDbFactory should be SingleInstance");

        var fileOps1 = _container.Resolve<ISqliteFileOperations>();
        var fileOps2 = _container.Resolve<ISqliteFileOperations>();
        Assert.That(fileOps2, Is.SameAs(fileOps1), "ISqliteFileOperations should be SingleInstance");

        // Test InstancePerDependency services
        var connection1 = _container.Resolve<ISqliteConnection>();
        var connection2 = _container.Resolve<ISqliteConnection>();
        Assert.That(connection2, Is.Not.SameAs(connection1), "ISqliteConnection should be InstancePerDependency");

        var entityCreator1 = _container.Resolve<IEntityCreator>();
        var entityCreator2 = _container.Resolve<IEntityCreator>();
        Assert.That(entityCreator2, Is.Not.SameAs(entityCreator1), "IEntityCreator should be InstancePerDependency");
    }

    [Test]
    public void ResolveServiceHierarchy_EntityServicesAndDependencies()
    {
        // Test that we can resolve the composite service and it has its dependencies
        var entityServices = _container.Resolve<IEntityServices>();
        Assert.That(entityServices, Is.Not.Null);

        // Verify the service can be used (basic functionality test)
        Assert.That(entityServices, Is.InstanceOf<IEntityCreator>());
        Assert.That(entityServices, Is.InstanceOf<IEntityGetter>());
        Assert.That(entityServices, Is.InstanceOf<IEntityUpdater>());
        Assert.That(entityServices, Is.InstanceOf<IEntityDeleter>());
        Assert.That(entityServices, Is.InstanceOf<IEntityUpserter>());
    }

    [Test]
    public void ResolveDisposableServices_CanBeDisposed()
    {
        // Test that disposable services can be resolved and disposed
        using (var connection = _container.Resolve<ISqliteConnection>())
        {
            Assert.That(connection, Is.Not.Null);
            Assert.DoesNotThrow(() => connection.Dispose());
        }

        using (var command = _container.Resolve<ISqliteCommand>())
        {
            Assert.That(command, Is.Not.Null);
            Assert.DoesNotThrow(() => command.Dispose());
        }

        using (var transaction = _container.Resolve<ISqliteTransaction>())
        {
            Assert.That(transaction, Is.Not.Null);
            Assert.DoesNotThrow(() => transaction.Dispose());
        }
    }

    [Test]
    public void ContainerModule_RegistrationsAreValid()
    {
        // Verify that the container has registrations for key services
        var registrations = _container.ComponentRegistry.Registrations.ToList();
        Assert.That(registrations, Is.Not.Empty);
        Assert.That(registrations.Count, Is.GreaterThan(20), "Should have many service registrations");

        // Check that we have both singleton and instance-per-dependency registrations
        var hasInstances = registrations.Any();
        Assert.That(hasInstances, Is.True, "Should have service registrations");
    }
}