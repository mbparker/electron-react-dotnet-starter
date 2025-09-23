using System.Collections;
using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

// This is basically just a data collector for deferred SQL execution and results enumeration.
// We can't generate the SQL until we know filtering and sorting - which get invoked on the result of the ORM Get call.
// Therefore, we can't actually query until we have that information. So the trigger becomes the invocation of GetEnumerator.
public class SqliteOrderedQueryable<T> : ISqliteQueryable<T>, ISqliteOrderedQueryable<T>, IEnumerable<T>
{
    private readonly Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc;
    private readonly Func<ISqliteDataRow, T> modelDeserializerFunc;
    private readonly List<SqliteSortSpec> sortSpecs;
    private Expression<Func<T, bool>> wherePredicate;
    private int? skipCount;
    private int? takeCount;
    private bool disposeConnection;

    public SqliteOrderedQueryable(
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, bool disposeConnection)
        : this(executeFunc, modelDeserializerFunc, null, null, disposeConnection, null, null)
    {
    }

    private SqliteOrderedQueryable(
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, Expression<Func<T, bool>> wherePredicate, SqliteSortSpec newSpec,
        bool disposeConnection, int? skipCount, int? takeCount)
        : this(executeFunc, modelDeserializerFunc, wherePredicate, [], skipCount, takeCount, newSpec, disposeConnection)
    {
    }

    private SqliteOrderedQueryable(
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, Expression<Func<T, bool>> wherePredicate,
        List<SqliteSortSpec> sortSpecs, int? skipCount, int? takeCount, SqliteSortSpec newSpec, bool disposeConnection)
    {
        this.executeFunc = executeFunc;
        this.modelDeserializerFunc = modelDeserializerFunc;
        this.wherePredicate = wherePredicate;
        this.sortSpecs = sortSpecs;
        this.skipCount = skipCount;
        this.takeCount = takeCount;        
        if (newSpec is not null)
            this.sortSpecs.Add(newSpec);
        this.disposeConnection = disposeConnection;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new SqliteOrderedEnumerator(
            executeFunc.Invoke(new SynthesizeSelectSqlArgs(wherePredicate, sortSpecs.ToArray(), skipCount, takeCount)),
            modelDeserializerFunc, disposeConnection);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerable<T> ISqliteEnumerable<T>.AsEnumerable()
    {
        return this;
    }

    public ISqliteEnumerable<T> Skip(int count)
    {
        skipCount = count;
        return this;
    }

    public ISqliteEnumerable<T> Take(int count)
    {
        takeCount = count;
        return this;
    }

    public ISqliteQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (wherePredicate is not null)
            wherePredicate = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(wherePredicate.Body, predicate.Body),
                predicate.Parameters);
        else
            wherePredicate = predicate;
        return this;
    }

    public ISqliteOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }

    public ISqliteOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }

    public ISqliteOrderedQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }

    public ISqliteOrderedQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }

    private ISqliteOrderedQueryable<T> New<TKey>(Expression<Func<T, TKey>> keySelectorExpr, bool descending)
    {
        return new SqliteOrderedQueryable<T>(executeFunc, modelDeserializerFunc, wherePredicate, sortSpecs, skipCount,
            takeCount, new SqliteSortSpec(keySelectorExpr, descending), disposeConnection);
    }

    private class SqliteOrderedEnumerator : IEnumerator<T>
    {
        private readonly ISqliteDataReader dataReader;
        private readonly Func<ISqliteDataRow, T> modelDeserializerFunc;
        private IEnumerator<ISqliteDataRow> enumerator;
        private T current;
        private bool disposed;
        private bool disposeConnection;

        internal SqliteOrderedEnumerator(ISqliteDataReader dataReader, Func<ISqliteDataRow, T> modelDeserializerFunc, bool disposeConnection)
        {
            this.dataReader = dataReader;
            this.modelDeserializerFunc = modelDeserializerFunc;
            this.disposeConnection = disposeConnection;
        }

        public bool MoveNext()
        {
            enumerator ??= dataReader.GetEnumerator();
            var result = enumerator.MoveNext();
            if (result)
                current = modelDeserializerFunc.Invoke(enumerator.Current);
            else
                current = default;
            return result;
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        T IEnumerator<T>.Current => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
            if (!disposed)
            {
                current = default;
                var conn = dataReader.Connection;
                enumerator.Dispose();
                dataReader.Dispose();
                if (disposeConnection)
                {
                    // Must dispose of the connection due to the deferred execution - the Execute callback which allocates it must pass ownership to here.
                    conn.Dispose();
                }

                disposed = true;
            }
        }
    }
}