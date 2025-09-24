using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;
using Newtonsoft.Json;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteDbSchemaMigrator<TContext> : ISqliteDbSchemaMigrator<TContext> 
    where TContext : ISqliteOrmDatabaseContext
{
    private readonly ISqliteSchemaObjectRelationalMapping<SqliteOrmSchemaContext> schemaOrm;
    private readonly ISqliteSchemaObjectRelationalMapping<TContext> modelOrm;
    private readonly Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory;
    private readonly ISqliteDbFactory dbFactory;
    private bool initialized;

    public SqliteDbSchemaMigrator(ISqliteSchemaObjectRelationalMapping<SqliteOrmSchemaContext> schemaOrm,
        ISqliteSchemaObjectRelationalMapping<TContext> modelOrm,
        Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory,
        ISqliteDbFactory dbFactory)
    {
        this.schemaOrm = schemaOrm;
        this.modelOrm = modelOrm;
        this.ddlSqlSynthesizerFactory = ddlSqlSynthesizerFactory;
        this.dbFactory  = dbFactory;
    }

    public void CreateInitialMigration()
    {
        EnsureInitialized();
        
        dbFactory.Create(schemaOrm.Context.Schema, schemaOrm.Context.Filename, true);
        
        if (GetMostRecentMigration() is null)
        {
            var schemaJson = JsonConvert.SerializeObject(modelOrm.Context.Schema, Formatting.Indented);
            var migration = new SchemaMigration { Timestamp = DateTime.UtcNow, Schema = schemaJson };
            schemaOrm.Insert(migration);
        }
    }
    
    public SqliteDbSchemaChanges CheckForSchemaChanges()
    {
        EnsureInitialized();
        
        var previousSchema = JsonConvert.DeserializeObject<SqliteDbSchema>(GetMostRecentMigration().Schema);
        var newTables = new List<SqliteDbSchemaTable>();
        var removedTables = new List<SqliteDbSchemaTable>();
        var renamedTables = new List<RenamedTable>();
        var changedTables = new List<AlteredTable>();
        var alteredColumnFullNames = new List<NonMigratableAlteredColumn>();
        
        foreach(var table in modelOrm.Context.Schema.Tables)
            if (!previousSchema.Tables.ContainsKey(table.Key))
                newTables.Add(table.Value);
        
        foreach(var table in previousSchema.Tables)
            if (!modelOrm.Context.Schema.Tables.ContainsKey(table.Key))
                removedTables.Add(table.Value);

        if (newTables.Count > 0 && removedTables.Count > 0)
        {
            foreach (var newTable in newTables)
            {
                foreach (var removedTable in removedTables)
                {
                    if (newTable.Columns.Count == removedTable.Columns.Count)
                    {
                        var newCols = newTable.Columns.Values.OrderBy(x => x.Name).Select(x => x.Name).ToArray();
                        var removedCols = removedTable.Columns.Values.OrderBy(x => x.Name).Select(x => x.Name).ToArray();
                        if (newCols.SequenceEqual(removedCols, StringComparer.OrdinalIgnoreCase))
                        {
                            renamedTables.Add(new RenamedTable(removedTable.Name, newTable.Name));
                            break;
                        }
                    }
                }
            }

            if (renamedTables.Count > 0)
            {
                foreach (var item in renamedTables)
                {
                    newTables.RemoveAll(x => x.Name == item.NewName);
                    removedTables.RemoveAll(x => x.Name == item.OldName);
                }
            }
        }

        foreach (var table in modelOrm.Context.Schema.Tables)
        {
            var newCols = new List<string>();
            var removedCols = new List<string>();            
            if (previousSchema.Tables.TryGetValue(table.Key, out var previousTable))
            {
                foreach (var col in table.Value.Columns)
                    if (!previousTable.Columns.ContainsKey(col.Key))
                        newCols.Add(col.Key);
                foreach (var col in previousTable.Columns)
                    if (!table.Value.Columns.ContainsKey(col.Key))
                        removedCols.Add(col.Key);
                if (newCols.Count > 0 || removedCols.Count > 0)
                    changedTables.Add(new AlteredTable(table.Value, previousTable, newCols, removedCols));

                foreach (var col in table.Value.Columns)
                {
                    if (previousTable.Columns.TryGetValue(col.Key, out var colPrev))
                    {
                        if (col.Value.ModelFieldTypeName != colPrev.ModelFieldTypeName)
                            alteredColumnFullNames.Add(new NonMigratableAlteredColumn($"{table.Key}", $"{col.Key}", "The runtime data type has changed."));
                        if (col.Value.SerializedFieldTypeName != colPrev.SerializedFieldTypeName)
                            alteredColumnFullNames.Add(new NonMigratableAlteredColumn($"{table.Key}", $"{col.Key}", "The db data type has changed."));
                        if (col.Value.DbFieldTypeAffinity != colPrev.DbFieldTypeAffinity)
                            alteredColumnFullNames.Add(new NonMigratableAlteredColumn($"{table.Key}", $"{col.Key}", "The db column type affinity has changed."));
                        if (col.Value.ConverterTypeName != colPrev.ConverterTypeName)
                            alteredColumnFullNames.Add(new NonMigratableAlteredColumn($"{table.Key}", $"{col.Key}", "The value converter type has changed."));
                    }
                }
            }
        }
        
        return new SqliteDbSchemaChanges(previousSchema, newTables, removedTables, renamedTables, changedTables, alteredColumnFullNames);
    }

    public void Migrate(SqliteDbSchemaChanges changes)
    {
        EnsureInitialized();

        schemaOrm.BeginTransaction();
        try
        {
            schemaOrm.ExecuteNonQuery("PRAGMA foreign_keys = off;");
            AddNewTables(changes.NewTables);
            DropRemovedTables(changes.RemovedTables);
            RenameTables(changes.RenamedTables);
            AlterTables(changes.AlteredTables, changes.PreviousSchema);
            schemaOrm.ExecuteNonQuery("PRAGMA foreign_keys = on;");

            var schemaJson = JsonConvert.SerializeObject(modelOrm.Context.Schema, Formatting.Indented);
            var migration = new SchemaMigration { Timestamp = DateTime.UtcNow, Schema = schemaJson };
            schemaOrm.Insert(migration);
            
            schemaOrm.CommitTransaction();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            schemaOrm.RollbackTransaction();
            throw;
        }
    }
    
    private void AddNewTables(IReadOnlyList<SqliteDbSchemaTable> changesNewTables)
    {
        var sb = new StringBuilder();
        var synthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, modelOrm.Context.Schema);
        foreach (var table in changesNewTables)
        {
            var sql = synthesizer.SynthesizeCreate(table.Name);
            sb.AppendLine(sql);
                
        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void DropRemovedTables(IReadOnlyList<SqliteDbSchemaTable> changesRemovedTables)
    {
        var sb = new StringBuilder();
        var synthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, modelOrm.Context.Schema);
        foreach (var table in changesRemovedTables)
        {
            var sql = synthesizer.SynthesizeDrop(table.Name);
            sb.AppendLine(sql);
                
        }
        
        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void RenameTables(IReadOnlyList<RenamedTable> changesRenamedTables)
    {
        var sb = new StringBuilder();
        var synthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, modelOrm.Context.Schema);
        RenameTablesCreateNewTables(changesRenamedTables, synthesizer, sb);
        RenameTablesCopyRecordsToNewTables(changesRenamedTables, sb);
        RenameTablesDropOldTables(changesRenamedTables, sb, synthesizer);
    }

    private void RenameTablesCreateNewTables(IReadOnlyList<RenamedTable> changesRenamedTables,
        ISqliteDdlSqlSynthesizer synthesizer, StringBuilder sb)
    {
        foreach (var table in changesRenamedTables)
        {
            var sql = synthesizer.SynthesizeCreate(table.NewName);
            sb.AppendLine(sql);

        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }
    
    private void RenameTablesCopyRecordsToNewTables(IReadOnlyList<RenamedTable> changesRenamedTables, StringBuilder sb)
    {
        foreach (var table in changesRenamedTables)
        {
            CopyRecordsToOtherTable(table.OldName, table.OldName, table.NewName, modelOrm.Context.Schema, [], sb);
        }
    }
    
    private void RenameTablesDropOldTables(IReadOnlyList<RenamedTable> changesRenamedTables, StringBuilder sb,
        ISqliteDdlSqlSynthesizer synthesizer)
    {
        sb.Clear();
        foreach (var table in changesRenamedTables)
        {
            var sql = synthesizer.SynthesizeDrop(table.OldName);
            sb.AppendLine(sql);
        }
        
        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void AlterTables(IReadOnlyList<AlteredTable> changesAlteredTables, SqliteDbSchema previousSchema)
    {
        var sb = new StringBuilder();
        var synthesizerPreviousSchema = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, previousSchema);
        AlterTablesCreateTempTables(changesAlteredTables, synthesizerPreviousSchema, sb);
        AlterTablesCopyRecordsToTempTables(changesAlteredTables, previousSchema, sb);
        AlterTablesDropOriginalTables(changesAlteredTables, sb, synthesizerPreviousSchema);
        AlterTablesCreateAlteredTables(changesAlteredTables, sb);
        AlterTablesCopyRecordsToAlteredTables(changesAlteredTables, previousSchema, sb);
        AlterTablesDropTempTables(changesAlteredTables, synthesizerPreviousSchema, sb);
    }
    
    private void AlterTablesCreateTempTables(IReadOnlyList<AlteredTable> changesAlteredTables,
        ISqliteDdlSqlSynthesizer synthesizer, StringBuilder sb)
    {
        foreach (var table in changesAlteredTables)
        {
            var sql = synthesizer.SynthesizeCreate(table.OldTableSchema.Name, $"{table.OldTableSchema.Name}_TEMP");
            sb.AppendLine(sql);
        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void AlterTablesCopyRecordsToTempTables(IReadOnlyList<AlteredTable> changesAlteredTables,
        SqliteDbSchema previousSchema, StringBuilder sb)
    {
        foreach (var table in changesAlteredTables)
        {
            var tempTableName = $"{table.OldTableSchema.Name}_TEMP";
            CopyRecordsToOtherTable(tempTableName, table.OldTableSchema.Name, table.NewTableSchema.Name, previousSchema,
                [], sb);
        }
    }

    private void AlterTablesDropOriginalTables(IReadOnlyList<AlteredTable> changesAlteredTables,
        StringBuilder sb, ISqliteDdlSqlSynthesizer synthesizerPreviousSchema)
    {
        sb.Clear();
        foreach (var table in changesAlteredTables)
        {
            var sql = synthesizerPreviousSchema.SynthesizeDrop(table.OldTableSchema.Name);
            sb.AppendLine(sql);
        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void AlterTablesCreateAlteredTables(IReadOnlyList<AlteredTable> changesAlteredTables, StringBuilder sb)
    {
        sb.Clear();
        var synthesizerNewSchema = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, modelOrm.Context.Schema);
        foreach (var table in changesAlteredTables)
        {
            var sql = synthesizerNewSchema.SynthesizeCreate(table.NewTableSchema.Name);
            sb.AppendLine(sql);
        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }

    private void AlterTablesCopyRecordsToAlteredTables(IReadOnlyList<AlteredTable> changesAlteredTables,
        SqliteDbSchema previousSchema, StringBuilder sb)
    {
        foreach (var table in changesAlteredTables)
        {
            var tempTableName = $"{table.OldTableSchema.Name}_TEMP";
            CopyRecordsToOtherTable(tempTableName, table.OldTableSchema.Name, table.NewTableSchema.Name, previousSchema,
                table.RemovedColumnNames, sb);
        }
    }

    private void AlterTablesDropTempTables(IReadOnlyList<AlteredTable> changesAlteredTables,
        ISqliteDdlSqlSynthesizer synthesizerPreviousSchema, StringBuilder sb)
    {
        sb.Clear();
        foreach (var table in changesAlteredTables)
        {
            var sql = synthesizerPreviousSchema.SynthesizeDrop($"{table.OldTableSchema.Name}_TEMP");
            sb.AppendLine(sql);
        }

        schemaOrm.ExecuteNonQuery(sb.ToString());
    }
    
    private void CopyRecordsToOtherTable(string tempTableName, string fromTableName,
        string toTableName, SqliteDbSchema fromTableSchema, IReadOnlyList<string> doNotCopyFieldNames, StringBuilder sb)
    {
        var rows = schemaOrm.ExecuteQuery($"SELECT * FROM {tempTableName}").AsEnumerable();
        foreach (var row in rows)
        {
            using (var insertCmd = schemaOrm.CurrentTransactionConnection.CreateCommand())
            {
                foreach (var col in row)
                {
                    if (!doNotCopyFieldNames.Contains(col.Name))
                    {
                        var previousCol = fromTableSchema.Tables[fromTableName].Columns[col.Name];
                        var valType = Type.GetType(previousCol.ModelFieldTypeName);
                        var convType = !string.IsNullOrWhiteSpace(previousCol.ConverterTypeName)
                            ? Type.GetType(previousCol.ConverterTypeName)
                            : null;
                        insertCmd.Parameters.Add(col.Name, col.ValueAs(valType), convType);
                    }
                }
                
                sb.Clear();
                sb.Append($"INSERT INTO {toTableName} (");
                var fieldNames = insertCmd.Parameters.Select(x => x.Name).ToList();
                sb.Append($"{string.Join(",", fieldNames)}) VALUES (");
                var paramNames = insertCmd.Parameters.Select(x => $":{x.Name}").ToList();
                sb.Append($"{string.Join(",", paramNames)});");
                insertCmd.ExecuteNonQuery(sb.ToString());
            }
        }
    }

    private SchemaMigration GetMostRecentMigration()
    {
        return schemaOrm.Get<SchemaMigration>()
            .OrderByDescending(x => x.Timestamp)
            .Take(1)
            .AsEnumerable()
            .SingleOrDefault();
    }
    
    private void EnsureInitialized()
    {
        if (!initialized)
        {
            if (string.IsNullOrWhiteSpace(modelOrm.Context.Filename))
                throw new InvalidOperationException(
                    "The model ORM context filename must be set prior to working with migrations.");
            schemaOrm.Context.Filename = modelOrm.Context.Filename;
            initialized = true;
        }
    }
}