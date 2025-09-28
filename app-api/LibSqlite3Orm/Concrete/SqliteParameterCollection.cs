using System.Collections;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteParameterCollection : ISqliteParameterCollection
{
    private readonly List<ISqliteParameter> parameters = new();
    private readonly Func<string, int, ISqliteParameter> parameterFactory;
    
    public SqliteParameterCollection(Func<string, int, ISqliteParameter> parameterFactory)
    {
        this.parameterFactory = parameterFactory;
    }
    
    public int Count => parameters.Count;

    public ISqliteParameter this[int index] => parameters[index];

    public ISqliteParameter this[string name] => parameters.FirstOrDefault(x => x.Name == name);
    
    public ISqliteParameter Add(string name, object value)
    {
        var result = parameterFactory(name, parameters.Count + 1);
        result.Set(value);
        parameters.Add(result);
        return result;
    }

    public ISqliteParameter Add(string name, object value, Type modelType)
    {
        var result = parameterFactory(name, parameters.Count + 1);
        if (modelType is not null)
            result.UseSerializer(modelType);
        result.Set(value);
        parameters.Add(result);
        return result;
    }

    public ISqliteParameter Add(string name, object value, ISqliteFieldSerializer serializer)
    {
        var result = parameterFactory(name, parameters.Count + 1);
        if (serializer is not null)
            result.UseSerializer(serializer);
        result.Set(value);
        parameters.Add(result);
        return result;
    }

    public void BindAll(IntPtr statement)
    {
        foreach (var param in parameters)
            param.Bind(statement);
    }

    public IEnumerator<ISqliteParameter> GetEnumerator()
    {
        return parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}