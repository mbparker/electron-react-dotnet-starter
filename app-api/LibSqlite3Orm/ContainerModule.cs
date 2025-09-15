using Autofac;
using Autofac.Core;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;
using LibSqlite3Orm.Types.ValueConverters;

namespace LibSqlite3Orm;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SqliteConnection>().As<ISqliteConnection>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteTransaction>().As<ISqliteTransaction>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteCommand>().As<ISqliteCommand>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameter>().As<ISqliteParameter>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteValueConverterCache>().As<ISqliteValueConverterCache>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameterCollection>().As<ISqliteParameterCollection>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameterPopulator>().As<ISqliteParameterPopulator>().SingleInstance();
        builder.RegisterType<SqliteEntityWriter>().As<ISqliteEntityWriter>().SingleInstance();
        
        builder.RegisterType<SqliteDataRow>().As<ISqliteDataRow>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteDataColumn>().As<ISqliteDataColumn>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteDataReader>().As<ISqliteDataReader>().InstancePerDependency().ExternallyOwned();

        builder.RegisterType<DateOnlyText>().SingleInstance();
        builder.RegisterType<DateTimeOffsetText>().SingleInstance();
        builder.RegisterType<DateTimeText>().SingleInstance();
        builder.RegisterType<DecimalText>().SingleInstance();
        builder.RegisterType<GuidText>().SingleInstance();
        builder.RegisterType<TimeOnlyText>().SingleInstance();
        builder.RegisterType<TimeSpanText>().SingleInstance();
        builder.RegisterType<BooleanLong>().SingleInstance();
        builder.RegisterType<CharText>().SingleInstance();

        builder.RegisterType<EntityCreator>().As<IEntityCreator>().InstancePerDependency();
        builder.RegisterType<EntityUpdater>().As<IEntityUpdater>().InstancePerDependency();
        builder.RegisterType<EntityUpserter>().As<IEntityUpserter>().InstancePerDependency();
        builder.RegisterType<EntityGetter>().As<IEntityGetter>().InstancePerDependency();
        builder.RegisterType<EntityDeleter>().As<IEntityDeleter>().InstancePerDependency();
        builder.RegisterType<EntityServices>().As<IEntityServices>().InstancePerDependency();
        
        builder.Register<Func<Type, ISqliteValueConverter>>(c =>
        {
            var ctx = c.Resolve<IComponentContext>();
            return (type) =>
            {
                if (ctx.Resolve(type) is not ISqliteValueConverter result)
                    throw new InvalidCastException($"Type {type} does not implement {nameof(ISqliteValueConverter)}.");
                return result;
            };
        });

        builder.RegisterType<SqliteDbSchemaBuilder>();
        builder.RegisterGeneric(typeof(SqliteObjectRelationalMapping<>)).As(typeof(ISqliteObjectRelationalMapping<>))
            .InstancePerDependency().ExternallyOwned();
        builder.RegisterGeneric(typeof(SqliteSchemaObjectRelationalMapping<>)).As(typeof(ISqliteSchemaObjectRelationalMapping<>))
            .InstancePerDependency();
        builder.RegisterType<SqliteOrmSchemaContext>().SingleInstance();
        builder.RegisterType<SqliteFileOperations>().As<ISqliteFileOperations>().SingleInstance();
        builder.RegisterType<SqliteUniqueIdGenerator>().As<ISqliteUniqueIdGenerator>().SingleInstance();
        builder.RegisterType<SqliteDbFactory>().As<ISqliteDbFactory>().SingleInstance();

        builder.RegisterType<SqliteWhereClauseBuilder>().As<ISqliteWhereClauseBuilder>().InstancePerDependency();

        builder.RegisterGeneric(typeof(SqliteDbSchemaMigrator<>)).As(typeof(ISqliteDbSchemaMigrator<>))
            .InstancePerDependency();
        
        builder.RegisterType<SqliteTableSqlSynthesizer>()
            .Keyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.TableOps).InstancePerDependency();
        builder.RegisterType<SqliteIndexSqlSynthesizer>()
            .Keyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.IndexOps).InstancePerDependency();
        builder.RegisterType<SqliteInsertSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Insert).InstancePerDependency();
        builder.RegisterType<SqliteUpdateSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Update).InstancePerDependency();
        builder.RegisterType<SqliteDeleteSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Delete).InstancePerDependency();
        builder.RegisterType<SqliteSelectSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Select).InstancePerDependency();   
        
        builder.Register<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (kind, schema) =>
            {
                var service = new KeyedService(kind, typeof(ISqliteDdlSqlSynthesizer));
                if (context.TryResolveService(service, [new TypedParameter(typeof(SqliteDbSchema), schema)], out object implementation))
                {
                    return implementation as ISqliteDdlSqlSynthesizer;
                }

                return null;
            };
        });
        
        builder.Register<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (kind, schema) =>
            {
                var service = new KeyedService(kind, typeof(ISqliteDmlSqlSynthesizer));
                if (context.TryResolveService(service, [new TypedParameter(typeof(SqliteDbSchema), schema)], out object implementation))
                {
                    return implementation as ISqliteDmlSqlSynthesizer;
                }

                return null;
            };
        });        
    }
}