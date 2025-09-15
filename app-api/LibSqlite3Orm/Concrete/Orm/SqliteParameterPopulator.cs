using System.ComponentModel;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteParameterPopulator : ISqliteParameterPopulator
{
    public void Populate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity)
    {
        switch (synthesisResult.SynthesisKind)
        {
            case SqliteDmlSqlSynthesisKind.Insert:
            case SqliteDmlSqlSynthesisKind.Update:
                PopulateForInsertOrUpdate(synthesisResult, parameterCollection, entity);
                break;
            case SqliteDmlSqlSynthesisKind.Select:
            case SqliteDmlSqlSynthesisKind.Delete:
                PopulateForSelectOrDelete(synthesisResult, parameterCollection);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(synthesisResult.SynthesisKind),
                    (int)synthesisResult.SynthesisKind,
                    typeof(SqliteDmlSqlSynthesisKind));
        }
    }

    private void PopulateForSelectOrDelete(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection)
    {
        if (synthesisResult.ExtractedParameters.Count > 0)
        {
            foreach (var parm in synthesisResult.ExtractedParameters.Values)
            {
                var col = synthesisResult.Table.Columns[parm.DbFieldName];
                Type converterType = null;
                if (!string.IsNullOrWhiteSpace(col.ConverterTypeName))
                    converterType = Type.GetType(col.ConverterTypeName);
                parameterCollection.Add(parm.Name, parm.Value, converterType);
            }
        }
    }

    private void PopulateForInsertOrUpdate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity)
    {
        var type = typeof(T);
        string skipColName = null;
        if (synthesisResult.SynthesisKind == SqliteDmlSqlSynthesisKind.Insert)
            skipColName = synthesisResult.Table.PrimaryKey?.AutoIncrement ?? false ? synthesisResult.Table.PrimaryKey.FieldName : null;
        var cols = synthesisResult.Table.Columns.Values.Where(x => !string.Equals(x.Name, skipColName)).OrderBy(x => x.Name).ToArray();
        foreach (var col in cols)
        {
            var member = type.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                Type converterType = null;
                if (!string.IsNullOrWhiteSpace(col.ConverterTypeName))
                    converterType = Type.GetType(col.ConverterTypeName);
                parameterCollection.Add(col.Name, member.GetValue(entity), converterType);
            }
            else
                throw new InvalidDataContractException(
                    $"Member {col.ModelFieldName} not found on type {type.AssemblyQualifiedName}.");
        }
    }
}