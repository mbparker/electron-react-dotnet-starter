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
            HashSet<string> otherTablesReferenced = new(StringComparer.OrdinalIgnoreCase);
            
            var sb = new StringBuilder();

            var selectArgs = args.GetArgs<SynthesizeSelectSqlArgs>();

            if (selectArgs.LoadNavigationProps)
            {
                foreach (var detProp in table.DetailProperties)
                {
                    otherTablesReferenced.Add(detProp.DetailTableName);
                }
            }

            // Compute Filter
            string whereClause = null;
            if (selectArgs.FilterExpr is not null)
            {
                var wcb = whereClauseBuilderFactory(Schema);
                whereClause = wcb.Build(entityType, selectArgs.FilterExpr);
                extractedParams = wcb.ExtractedParameters;
            }
            
            // Compute Sort
            var sortFields = new List<string>();
            if (selectArgs.SortSpecs?.Any() ?? false)
            {
                foreach (var sortSpec in selectArgs.SortSpecs)
                {
                    var dir = sortSpec.Descending ? "DESC" : "ASC";
                    var fieldName = $"{sortSpec.TableName}.{sortSpec.FieldName}";
                    sortFields.Add($"{fieldName} {dir}");
                }
            }
            
            // Field selection
            var cols = table.Columns.Values.OrderBy(x => x.Name).Select(x => $"{table.Name}.{x.Name} AS {table.Name}{x.Name}").ToList();
            if (otherTablesReferenced.Any())
            {
                foreach (var otherTable in otherTablesReferenced)
                {
                    cols.AddRange(Schema.Tables[otherTable].Columns.Values.OrderBy(x => x.Name)
                        .Select(x => $"{otherTable}.{x.Name} AS {otherTable}{x.Name}").ToList());
                }
            }
            sb.Append($"SELECT {string.Join(", ", cols)} FROM {table.Name}");

            // Join any additional tables
            if (otherTablesReferenced.Any())
            {
                foreach (var otherTable in otherTablesReferenced)
                {
                    var fk = table.ForeignKeys.SingleOrDefault(x => x.ForeignTableName == otherTable);
                    if (fk is not null)
                    {
                        if (fk.FieldNames.Length == fk.ForeignTableFields.Length)
                        {
                            var joinOnSb = new StringBuilder();
                            for (var i = 0; i < fk.FieldNames.Length; i++)
                            {
                                if (i > 0)
                                    joinOnSb.Append(" AND ");
                                joinOnSb.Append(
                                    $"{table.Name}.{fk.FieldNames[i]} = {otherTable}.{fk.ForeignTableFields[i]}");
                            }
                            
                            sb.Append($" INNER JOIN {otherTable} ON {joinOnSb}");
                        }
                    }
                }
            }
            
            // Filter
            if (!string.IsNullOrWhiteSpace(whereClause))
                sb.Append($" WHERE {whereClause}");
            
            // Sort
            if (sortFields.Any())
            {
                sb.Append(" ORDER BY ");
                sb.Append(string.Join(", ", sortFields));
            }

            // Take
            if (selectArgs.TakeCount.HasValue)
                sb.Append($" LIMIT {selectArgs.TakeCount.Value}");
            
            // Skip
            if (selectArgs.SkipCount.HasValue)
                sb.Append($" OFFSET {selectArgs.SkipCount.Value}");

            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Select, Schema, table,
                Schema.Tables.Values.Where(x => otherTablesReferenced.Contains(x.Name)).ToArray(), 
                sb.ToString(), extractedParams);
        }

        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}