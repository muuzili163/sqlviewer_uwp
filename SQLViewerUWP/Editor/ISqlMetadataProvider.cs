namespace SQLViewerUWP.Editor
{
    public interface ISqlMetadataProvider
    {
        Task<IReadOnlyList<string>> GetTablesAsync(string instanceName, string dbName);
        Task<IReadOnlyList<string>> GetColumnsAsync(string instanceName, string dbName, string tableName);
    }
}
