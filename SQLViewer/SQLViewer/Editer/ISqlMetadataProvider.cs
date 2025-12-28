using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public interface ISqlMetadataProvider
    {
        Task<IReadOnlyList<string>> GetTablesAsync(string instanceName, string dbName);
        Task<IReadOnlyList<string>> GetColumnsAsync(string instanceName, string dbName, string tableName);
    }
}
