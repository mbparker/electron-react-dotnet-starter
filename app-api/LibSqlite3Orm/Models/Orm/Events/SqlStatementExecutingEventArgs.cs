using System.Text;
using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Models.Orm.Events;

public class SqlStatementExecutingEventArgs : GenerativeLogicTraceEventArgs
{
    public SqlStatementExecutingEventArgs(string sqlStatement, ISqliteParameterCollectionDebug parameters)
        : base(RenderSqlLogString(sqlStatement, parameters))
    {
        SqlStatement = sqlStatement;
        Parameters = parameters;
    }
    
    public string SqlStatement { get; }
    public ISqliteParameterCollectionDebug Parameters { get; }

    private static Lazy<string> RenderSqlLogString(string sqlStatement, ISqliteParameterCollectionDebug parameters)
    {
        return new Lazy<string>(() =>
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(sqlStatement))
            {
                sb.Append($"Executing SQL:  {sqlStatement}\n");
                sb.Append("\tParameters:\n");
                if (parameters?.Count > 0)
                {
                    foreach (var p in parameters)
                    {
                        sb.Append($"\t\t{p.Name} = {p.GetDebugValue()}\n");
                    }
                }
                else
                {
                    sb.Append("\t\tNone\n");
                }
            }

            return sb.ToString();
        });
    }
}