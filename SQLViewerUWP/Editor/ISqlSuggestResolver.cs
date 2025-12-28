namespace SQLViewerUWP.Editor
{
    public interface ISqlSuggestResolver
    {
        Task<IReadOnlyList<SuggestItem>> ResolveAsync(SqlContext context);
    }
}
