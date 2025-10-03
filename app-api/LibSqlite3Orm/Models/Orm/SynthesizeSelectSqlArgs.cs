using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SynthesizeSelectSqlArgs
{
    public SynthesizeSelectSqlArgs(bool loadNavigationProps, Expression filterExpr,
        IReadOnlyList<SqliteSortSpec> sortSpecs, int? skipCount, int? takeCount)
    {
        LoadNavigationProps = loadNavigationProps;
        FilterExpr = filterExpr;
        SortSpecs = sortSpecs;
        SkipCount = skipCount;
        TakeCount = takeCount;
    }
    
    public bool LoadNavigationProps { get; }
    public Expression FilterExpr { get; }
    public IReadOnlyList<SqliteSortSpec> SortSpecs { get; }
    public int? SkipCount { get; }
    public int? TakeCount { get; }
}