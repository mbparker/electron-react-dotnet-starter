using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteFieldValueSerialization : ISqliteFieldValueSerialization
{
    private readonly Lock syncLock = new ();
    private readonly Dictionary<Type, ISqliteFieldSerializer> serializers;
    private readonly Func<Type, ISqliteEnumFieldSerializer> enumSerializerFactory;

    public SqliteFieldValueSerialization(IEnumerable<ISqliteFieldSerializer> serializers,
        Func<Type, ISqliteEnumFieldSerializer> enumSerializerFactory)
    {
        this.serializers = serializers.ToDictionary(k => k.RuntimeType, v => v);
        this.enumSerializerFactory = enumSerializerFactory;
    }

    public bool IsSerializerRegisteredForModelType(Type modelType)
    {
        lock (syncLock)
        {
            return serializers.ContainsKey(modelType);
        }
    }

    public void RegisterSerializer(ISqliteFieldSerializer serializer)
    {
        lock (syncLock)
        {
            if (IsSerializerRegisteredForModelType(serializer.RuntimeType))
                throw new InvalidOperationException($"Serializer for type {serializer.RuntimeType.Name} is already registered");
            serializers.Add(serializer.RuntimeType, serializer);
        }
    }
    
    public void ReplaceSerializer(Type modelType, ISqliteFieldSerializer newSerializer)
    {
        lock (syncLock)
        {
            if (newSerializer.RuntimeType != modelType)
                throw new ArgumentException($"Serializer runtime type {newSerializer.RuntimeType.Name} does not match model type {modelType.Name}");
            if (!IsSerializerRegisteredForModelType(modelType))
                throw new InvalidOperationException($"There is no serializer for type {modelType.Name} registered");
            var currentSerializer = serializers[modelType];
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (currentSerializer is IDisposable disposable) disposable.Dispose();
            serializers[modelType] = newSerializer;
        }
    }    

    public ISqliteFieldSerializer this[Type modelType]
    {
        get
        {
            lock (syncLock)
            {
                serializers.TryGetValue(modelType, out var serializer);
                if (serializer is null && modelType.IsEnum)
                {
                    serializer = enumSerializerFactory(modelType);
                    serializers.Add(modelType, serializer);
                }

                return serializer;
            }
        }
    }
}