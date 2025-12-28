using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public class DefaultSqlMetadataProvider : ISqlMetadataProvider
    {

        private static DefaultSqlMetadataProvider providerInstance;

        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _tableCache = new();
        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _columnCache = new();

        public static DefaultSqlMetadataProvider getProviderInstance() { 
            if(providerInstance == null)
            {
                providerInstance = new DefaultSqlMetadataProvider();
            }
            return providerInstance;
        }

        private static string TableKey(string instance, string db)
            => $"{instance}|{db}";

        private static string ColumnKey(string instance, string db, string table)
            => $"{instance}|{db}|{table}";

        public async Task<IReadOnlyList<string>> GetTablesAsync(string instanceName, string dbName)
        {
            var key = TableKey(instanceName, dbName);

            if (_tableCache.TryGetValue(key, out var cached))
                return cached;

            var tables = await Task.Run(() =>
                (IReadOnlyList<string>)HttpUtils.queryTable(instanceName, dbName)
            );

            _tableCache[key] = tables;
            return tables;
        }

        public async Task<IReadOnlyList<string>> GetColumnsAsync(
            string instanceName,
            string dbName,
            string tableName)
        {
            var key = ColumnKey(instanceName, dbName, tableName);

            if (_columnCache.TryGetValue(key, out var cached))
                return cached;

            var columns = await Task.Run(() =>
                (IReadOnlyList<string>)HttpUtils.queryColumn(instanceName, dbName, tableName)
            );

            _columnCache[key] = columns;
            return columns;
        }
    }
}
