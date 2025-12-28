namespace SQLViewerUWP.Editor
{
    public class SqlSuggestResolver : ISqlSuggestResolver
    {
        private readonly ISqlMetadataProvider _metadata;

        private static readonly string[] Keywords =
        {
            "SELECT","FROM","WHERE","JOIN","LEFT","RIGHT","INNER",
            "ON","GROUP","BY","ORDER","INSERT","UPDATE","DELETE"
        };

        private string _serverName;
        private string _dbName;

        public SqlSuggestResolver(ISqlMetadataProvider metadata, string instanceName, string dbName)
        {
            _metadata = metadata;
            _serverName = instanceName;
            _dbName = dbName;
        }

        public async Task<IReadOnlyList<SuggestItem>> ResolveAsync(SqlContext context)
        {
            return context.Clause switch
            {
                SqlClause.Select => await ResolveSelectAsync(context),
                SqlClause.From => await ResolveFromAsync(context),
                SqlClause.Join => await ResolveJoinAsync(context),
                SqlClause.Where => await ResolveWhereAsync(context),
                SqlClause.On => await ResolveOnAsync(context),
                _ => ResolveKeywordFallback(context)
            };
        }

        private async Task<IReadOnlyList<SuggestItem>> ResolveSelectAsync(SqlContext ctx)
        {
            var result = new List<SuggestItem>();

            foreach (var table in ctx.Tables)
            {
                var columns = await _metadata.GetColumnsAsync(_serverName, _dbName, table);
                result.AddRange(columns.Select(c => new SuggestItem
                {
                    Text = $"{table}.{c}",
                    Type = SuggestType.Column
                }));
            }

            return Filter(result, ctx.CurrentToken);
        }

        private async Task<IReadOnlyList<SuggestItem>> ResolveFromAsync(SqlContext ctx)
        {
            var tables = await _metadata.GetTablesAsync(_serverName, _dbName);

            return Filter(
                tables.Select(t => new SuggestItem
                {
                    Text = t,
                    Type = SuggestType.Table
                }).ToList(),
                ctx.CurrentToken
            );
        }

        private async Task<IReadOnlyList<SuggestItem>> ResolveJoinAsync(SqlContext ctx)
        {
            return await ResolveFromAsync(ctx);
        }

        private async Task<IReadOnlyList<SuggestItem>> ResolveWhereAsync(SqlContext ctx)
        {
            var result = new List<SuggestItem>();

            foreach (var (alias, table) in ctx.Aliases)
            {
                var columns = await _metadata.GetColumnsAsync(_serverName, _dbName, table);
                result.AddRange(columns.Select(c => new SuggestItem
                {
                    Text = $"{alias}.{c}",
                    Type = SuggestType.Column
                }));
            }

            return Filter(result, ctx.CurrentToken);
        }

        private async Task<IReadOnlyList<SuggestItem>> ResolveOnAsync(SqlContext ctx)
        {
            var result = new List<SuggestItem>();

            foreach (var (alias, table) in ctx.Aliases)
            {
                var columns = await _metadata.GetColumnsAsync(_serverName, _dbName, table);
                result.AddRange(columns.Select(c => new SuggestItem
                {
                    Text = $"{alias}.{c}",
                    Type = SuggestType.Column
                }));
            }

            return Filter(result, ctx.CurrentToken);
        }

        private IReadOnlyList<SuggestItem> ResolveKeywordFallback(SqlContext ctx)
        {
            return Filter(
                Keywords.Select(k => new SuggestItem
                {
                    Text = k,
                    Type = SuggestType.Keyword
                }).ToList(),
                ctx.CurrentToken
            );
        }

        private static IReadOnlyList<SuggestItem> Filter(
            List<SuggestItem> source,
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return source;

            return source
                .Where(s => s.Text.StartsWith(token, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
