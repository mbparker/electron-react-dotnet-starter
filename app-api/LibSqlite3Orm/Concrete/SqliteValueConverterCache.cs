using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteValueConverterCache : ISqliteValueConverterCache
{
    private readonly Lock syncObj = new();
    private readonly Func<Type, ISqliteValueConverter> converterFactory;
    private readonly Dictionary<Type, ISqliteValueConverter> converters;
    
    public SqliteValueConverterCache(Func<Type, ISqliteValueConverter> converterFactory)
    {
        this.converterFactory = converterFactory;
        converters = new Dictionary<Type, ISqliteValueConverter>();
    }
    
    public ISqliteValueConverter this[Type type]
    {
        get
        {
            lock (syncObj)
            {
                if (!converters.TryGetValue(type, out var converter))
                {
                    converter = converterFactory(type);
                    converters.Add(type, converter);
                }

                return converter;
            }
        }
    }
}