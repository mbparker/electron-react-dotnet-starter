using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Models.Orm;

public class DmlSqlSynthesisResult
{
    public DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind synthesisKind, SqliteDbSchema schema, SqliteDbSchemaTable table,
        string sqlText, IReadOnlyDictionary<string, ExtractedParameter> extractedParameters)
    {
        SynthesisKind = synthesisKind;
        Schema = schema;
        Table = table;
        SqlText = sqlText;
        ExtractedParameters = extractedParameters ?? new Dictionary<string, ExtractedParameter>();
    }

    public SqliteDmlSqlSynthesisKind SynthesisKind { get; }
    public SqliteDbSchema Schema { get; }
    public SqliteDbSchemaTable Table { get; }
    public string SqlText { get; }
    public IReadOnlyDictionary<string, ExtractedParameter> ExtractedParameters { get; }
}

public class ExtractedParameter
{
    public ExtractedParameter(string name, object value, string dbFieldName)
    {
        Name = name;
        Value = value;
        DbFieldName = dbFieldName;
    }
    
    public string Name { get; }
    public object Value { get; }
    public string DbFieldName { get; }
}