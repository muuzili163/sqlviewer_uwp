namespace SQLViewerUWP.Editor
{
    public sealed class SqlContext
    {
        public int CaretIndex { get; init; }
        public int TokenStartIndex { get; init; }
        public int TokenLength { get; init; }

        public SqlClause Clause { get; init; }

        public string CurrentToken { get; init; } = string.Empty;

        public bool IsAliasMemberAccess { get; init; }
        public string? CurrentAlias { get; init; }

        public IReadOnlyList<string> Tables { get; init; } = Array.Empty<string>();

        public IReadOnlyDictionary<string, string> Aliases { get; init; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
