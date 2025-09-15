using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteSelectSqlSynthesizer : SqliteDmlSqlSynthesizerBase
{
    private readonly Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory;
    
    public SqliteSelectSqlSynthesizer(SqliteDbSchema schema, Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory) 
        : base(schema)
    {
        this.whereClauseBuilderFactory = whereClauseBuilderFactory;
    }

    public override DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args)
    {
        var entityTypeName = entityType.AssemblyQualifiedName;
        var table = Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            IReadOnlyDictionary<string, ExtractedParameter> extractedParams = null;
            var sb = new StringBuilder();

            // Field selection
            var cols = table.Columns.Values.OrderBy(x => x.Name).ToArray();
            var colNames = cols.Select(x => x.Name).ToArray();
            sb.Append($"SELECT {string.Join(", ", colNames)} FROM {table.Name}");

            var selectArgs = args.GetArgs<SynthesizeSelectSqlArgs>();

            // Filter
            if (selectArgs.FilterExpr is not null)
            {
                var wcb = whereClauseBuilderFactory(Schema);
                var wc = wcb.Build(entityType, selectArgs.FilterExpr);
                sb.Append($" WHERE {wc}");
                extractedParams = wcb.ExtractedParameters;
            }

            // Sort
            if (selectArgs.SortSpecs?.Any() ?? false)
            {
                sb.Append(" ORDER BY ");
                var fields = new List<string>();
                foreach (var sortSpec in selectArgs.SortSpecs)
                {
                    var dir = sortSpec.Descending ? "DESC" : "ASC";
                    var fieldName = cols.Single(x => x.ModelFieldName == sortSpec.ModelMemberName).Name;
                    fields.Add($"{fieldName} {dir}");
                }

                sb.Append(string.Join(", ", fields));
            }

            // Take
            if (selectArgs.TakeCount.HasValue)
                sb.Append($" LIMIT {selectArgs.TakeCount.Value}");
            
            // Skip
            if (selectArgs.SkipCount.HasValue)
                sb.Append($" OFFSET {selectArgs.SkipCount.Value}");

            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Select, Schema, table, sb.ToString(),
                extractedParams);
        }

        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}