using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.Tests;

public class SqliteTableSqlSynthesizerTests
{
    private readonly SqliteDbSchema _schema;
    private readonly SqliteTableSqlSynthesizer _synthesizer;

    public SqliteTableSqlSynthesizerTests()
    {
        _schema = new SqliteDbSchema();
        _synthesizer = new SqliteTableSqlSynthesizer(_schema);
    }

    [Fact]
    public void SynthesizeDrop_ReturnsCorrectDropStatement()
    {
        // Act
        var result = _synthesizer.SynthesizeDrop("TestTable");

        // Assert
        Assert.Equal("DROP TABLE IF EXISTS TestTable;", result);
    }

    [Fact]
    public void SynthesizeCreate_WithSimpleTable_ReturnsCorrectCreateStatement()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            Name = "TestTable",
            Columns = new Dictionary<string, SqliteDbSchemaTableColumn>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Id", new SqliteDbSchemaTableColumn
                    {
                        Name = "Id",
                        DbFieldTypeAffinity = SqliteColType.Integer,
                        IsNotNull = true
                    }
                },
                {
                    "Name", new SqliteDbSchemaTableColumn
                    {
                        Name = "Name",
                        DbFieldTypeAffinity = SqliteColType.Text,
                        IsNotNull = false
                    }
                }
            }
        };

        _schema.Tables["TestTable"] = table;

        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.Contains("CREATE TABLE IF NOT EXISTS TestTable", result);
        Assert.Contains("Id INTEGER", result);
        Assert.Contains("Name TEXT", result);
    }

    [Fact]
    public void SynthesizeCreate_WithNewObjectName_UsesNewName()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            Name = "OriginalTable",
            Columns = new Dictionary<string, SqliteDbSchemaTableColumn>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Id", new SqliteDbSchemaTableColumn
                    {
                        Name = "Id",
                        DbFieldTypeAffinity = SqliteColType.Integer
                    }
                }
            }
        };

        _schema.Tables["OriginalTable"] = table;

        // Act
        var result = _synthesizer.SynthesizeCreate("OriginalTable", "NewTable");

        // Assert
        Assert.Contains("CREATE TABLE IF NOT EXISTS NewTable", result);
        Assert.DoesNotContain("OriginalTable", result.Replace("OriginalTable", "NewTable"));
    }

    [Fact]
    public void SynthesizeCreate_WithUniqueColumn_IncludesUniqueConstraint()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            Name = "TestTable",
            Columns = new Dictionary<string, SqliteDbSchemaTableColumn>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Email", new SqliteDbSchemaTableColumn
                    {
                        Name = "Email",
                        DbFieldTypeAffinity = SqliteColType.Text,
                        IsUnique = true
                    }
                }
            }
        };

        _schema.Tables["TestTable"] = table;

        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.Contains("Email TEXT UNIQUE", result);
    }

    [Fact]
    public void SynthesizeCreate_WithMultipleColumns_GeneratesCorrectSql()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            Name = "TestTable",
            Columns = new Dictionary<string, SqliteDbSchemaTableColumn>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "Id", new SqliteDbSchemaTableColumn
                    {
                        Name = "Id",
                        DbFieldTypeAffinity = SqliteColType.Integer,
                        IsNotNull = true
                    }
                },
                {
                    "Score", new SqliteDbSchemaTableColumn
                    {
                        Name = "Score",
                        DbFieldTypeAffinity = SqliteColType.Float
                    }
                },
                {
                    "Data", new SqliteDbSchemaTableColumn
                    {
                        Name = "Data",
                        DbFieldTypeAffinity = SqliteColType.Blob
                    }
                }
            }
        };

        _schema.Tables["TestTable"] = table;

        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.Contains("Id INTEGER", result);
        Assert.Contains("Score REAL", result);  // Float maps to REAL in SQLite
        Assert.Contains("Data BLOB", result);
        // Should contain commas between columns
        var commaCount = result.Count(c => c == ',');
        Assert.Equal(2, commaCount); // 2 commas for 3 columns
    }
}