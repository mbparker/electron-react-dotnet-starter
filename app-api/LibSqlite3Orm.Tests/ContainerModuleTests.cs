using Autofac;
using LibSqlite3Orm;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;

namespace LibSqlite3Orm.Tests;

public class ContainerModuleTests
{
    [Fact]
    public void ContainerModule_RegistersAllExpectedServices()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();

        // Act
        var container = builder.Build();

        // Assert - Test key service registrations
        Assert.True(container.IsRegistered<ISqliteUniqueIdGenerator>());
        Assert.True(container.IsRegistered<ISqliteFileOperations>());
        Assert.True(container.IsRegistered<ISqliteConnection>());
        Assert.True(container.IsRegistered<ISqliteCommand>());
        Assert.True(container.IsRegistered<ISqliteTransaction>());
        Assert.True(container.IsRegistered<ISqliteParameter>());
        Assert.True(container.IsRegistered<ISqliteParameterCollection>());
        Assert.True(container.IsRegistered<ISqliteValueConverterCache>());
    }

    [Fact]
    public void ContainerModule_CanBeBuilt()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();

        // Act & Assert - The main test is that this doesn't throw
        var container = builder.Build();
        Assert.NotNull(container);
        
        // Verify we can at least build the container and it has some expected services
        Assert.True(container.ComponentRegistry.Registrations.Any());
    }

    [Fact]
    public void ContainerModule_ResolvesUniqueIdGenerator()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        var container = builder.Build();

        // Act
        var service = container.Resolve<ISqliteUniqueIdGenerator>();

        // Assert
        Assert.NotNull(service);
        var id = service.NewUniqueId();
        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public void ContainerModule_ResolvesFileOperations()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        var container = builder.Build();

        // Act
        var service = container.Resolve<ISqliteFileOperations>();

        // Assert
        Assert.NotNull(service);
        // Test a simple operation
        Assert.False(service.FileExists("non-existent-file-path"));
    }

    [Fact]
    public void ContainerModule_ResolvesSingletonServices()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        var container = builder.Build();

        // Act - Resolve singleton services multiple times
        var uniqueIdGen1 = container.Resolve<ISqliteUniqueIdGenerator>();
        var uniqueIdGen2 = container.Resolve<ISqliteUniqueIdGenerator>();
        
        var fileOps1 = container.Resolve<ISqliteFileOperations>();
        var fileOps2 = container.Resolve<ISqliteFileOperations>();

        // Assert - Should be same instance for singleton registrations
        Assert.Same(uniqueIdGen1, uniqueIdGen2);
        Assert.Same(fileOps1, fileOps2);
    }

    [Fact]
    public void ContainerModule_ResolvesInstancePerDependencyServices()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterModule<ContainerModule>();
        var container = builder.Build();

        // Act - Resolve instance-per-dependency services multiple times
        var connection1 = container.Resolve<ISqliteConnection>();
        var connection2 = container.Resolve<ISqliteConnection>();
        
        var command1 = container.Resolve<ISqliteCommand>();
        var command2 = container.Resolve<ISqliteCommand>();

        // Assert - Should be different instances for InstancePerDependency registrations
        Assert.NotSame(connection1, connection2);
        Assert.NotSame(command1, command2);
    }
}