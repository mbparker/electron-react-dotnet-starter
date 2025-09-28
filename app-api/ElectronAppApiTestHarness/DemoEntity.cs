using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;

namespace ElectronAppApiTestHarness;

public enum Entitykind
{
    Kind1,
    Kind2
}

public class BaseEntity
{
    public long Id { get; set; }
}

public class DemoEntity : BaseEntity
{
    public Entitykind EnumValue { get; set; }
    public string StringValue { get; set; }
    public bool BoolValue { get; set; }
    public double? DoubleValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public long LongValue { get; set; }
    public ulong? ULongValue { get; set; }
    public Guid? GuidValue { get; set; }
    public DateOnly? DateOnlyValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public TimeOnly? TimeOnlyValue { get; set; }
    public TimeSpan? TimeSpanValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public byte[] BlobValue { get; set; }
    [NotMapped]
    public ISqliteQueryable<CustomTagLink> Tags { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Id: {Id}");
        sb.AppendLine($"EnumValue: {EnumValue}");
        sb.AppendLine($"StringValue: {StringValue}");
        sb.AppendLine($"BoolValue: {BoolValue}");
        sb.AppendLine($"DoubleValue: {DoubleValue}");
        sb.AppendLine($"DecimalValue: {DecimalValue}");
        sb.AppendLine($"LongValue: {LongValue}");
        sb.AppendLine($"ULongValue: {ULongValue}");
        sb.AppendLine($"GuidValue: {GuidValue}");
        sb.AppendLine($"DateOnlyValue: {DateOnlyValue}");
        sb.AppendLine($"DateTimeValue: {DateTimeValue}");
        sb.AppendLine($"TimeOnlyValue: {TimeOnlyValue}");
        sb.AppendLine($"TimeSpanValue: {TimeSpanValue}");
        sb.AppendLine($"DateTimeOffsetValue: {DateTimeOffsetValue}");
        if (BlobValue is  not null)
            sb.AppendLine($"BlobValue: {Convert.ToHexString(BlobValue)}");
        else
            sb.AppendLine($"BlobValue: null");
        sb.AppendLine("Custom Tag Links:");
        if (Tags is not null)
        {
            var links = Tags.AsEnumerable().ToArray();
            if (links.Length != 0)
            {
                foreach (var link in links)
                {
                    sb.AppendLine($"{link}");
                }
            }
            else
            {
                sb.AppendLine("\tNo links");    
            }
        }
        else
        {
            sb.AppendLine("\tNo links");
        }

        return sb.ToString();
    }
}

public class CustomTag : BaseEntity
{
    public string TagValue { get; set; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\t\tId: {Id}");
        sb.AppendLine($"\t\tTagValue: {TagValue}");
        return sb.ToString();
    }
}

public class CustomTagLink : BaseEntity
{
    public long TagId { get; set; }
    [NotMapped]
    public CustomTag Tag { get; set; }
    public long EntityId { get; set; }
    [NotMapped]
    public DemoEntity Entity { get; set; }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\tId: {Id}");
        sb.AppendLine($"\tTagId: {TagId}");
        sb.AppendLine($"\tEntityId: {EntityId}");
        if (Tag is not null)
            sb.Append($"\tTag:\n{Tag}");
        else
            sb.AppendLine("\tTag entity is null");
        sb.AppendLine("\tEntity:");
        if (Entity is not null)
        {
            sb.AppendLine($"\t\tEntity.Id: {Entity.Id}");
            sb.AppendLine($"\t\tEntity.StringValue: {Entity.StringValue}");
        }
        else
            sb.AppendLine("\t\tEntity is null");
        return sb.ToString();
    }
}