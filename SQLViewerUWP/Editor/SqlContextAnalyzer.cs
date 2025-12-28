using System.Text.RegularExpressions;

namespace SQLViewerUWP.Editor
{
    public sealed class SqlContextAnalyzer
    {
        private static readonly Regex TokenRegex =
        new Regex(@"[A-Za-z_][A-Za-z0-9_]*\.?|[A-Za-z_][A-Za-z0-9_]*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HashSet<string> SqlKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // DML
            "select", "from", "where", "join", "on",
            "insert", "update", "delete",

            // JOIN types
            "left", "right", "inner", "outer", "full",

            // GROUP / ORDER
            "group", "by", "order", "having",

            // Others
            "as", "and", "or", "not",
            "distinct", "limit", "offset"
        };

        public SqlContext Analyze(string sql, int caretIndex)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new SqlContext();

            caretIndex = Math.Clamp(caretIndex, 0, sql.Length);

            string before = sql.Substring(0, caretIndex);

            var tokens = ExtractTokens(before);

            var clause = DetectClause(tokens);
            var (tables, aliases) = ParseTablesAndAliases(tokens);

            var currentToken = GetCurrentToken(before);
            var (isAliasAccess, alias) = ParseAliasAccess(currentToken);

            var (tokenStart, tokenLength, token) = ParseToken(sql, caretIndex);

            return new SqlContext
            {
                CaretIndex = caretIndex,
                TokenStartIndex = tokenStart,
                TokenLength = tokenLength,
                Clause = clause,
                CurrentToken = currentToken,
                IsAliasMemberAccess = isAliasAccess,
                CurrentAlias = alias,
                Tables = tables,
                Aliases = aliases
            };
        }

        private static bool IsTokenChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == '.';
        }

        private static (int start, int length, string token) ParseToken(string sql, int caret)
        {
            if (caret == 0)
                return (caret, 0, "");

            int start = caret - 1;
            while (start >= 0 && IsTokenChar(sql[start]))
                start--;

            start++;

            int length = caret - start;
            string token = sql.Substring(start, length);

            return (start, length, token);
        }

        private static List<string> ExtractTokens(string sql)
        {
            return TokenRegex.Matches(sql)
                .Select(m => m.Value)
                .ToList();
        }

        private static SqlClause DetectClause(List<string> tokens)
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                switch (tokens[i].ToLowerInvariant())
                {
                    case "select": return SqlClause.Select;
                    case "from": return SqlClause.From;
                    case "join": return SqlClause.Join;
                    case "on": return SqlClause.On;
                    case "where": return SqlClause.Where;
                    case "group": return SqlClause.GroupBy;
                    case "order": return SqlClause.OrderBy;
                }
            }
            return SqlClause.Unknown;
        }

        private static (List<string>, Dictionary<string, string>) ParseTablesAndAliases(List<string> tokens)
        {
            var tables = new List<string>();
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                if (IsTableKeyword(tokens[i]))
                {
                    string table = tokens[i + 1];
                    tables.Add(table);

                    // Check for alias
                    if (i + 2 < tokens.Count && !IsSqlKeyword(tokens[i + 2]))
                    {
                        aliases[tokens[i + 2]] = table;
                    }
                }
            }

            return (tables, aliases);
        }

        private static bool IsSqlKeyword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            return SqlKeywords.Contains(token);
        }

        private static bool IsTableKeyword(string token)
        {
            return token.Equals("from", StringComparison.OrdinalIgnoreCase)
                || token.Equals("join", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetCurrentToken(string sqlBeforeCaret)
        {
            var match = Regex.Match(sqlBeforeCaret, @"([A-Za-z_][A-Za-z0-9_\.]*)$");
            return match.Success ? match.Value : string.Empty;
        }

        private static (bool, string?) ParseAliasAccess(string token)
        {
            if (token.EndsWith("."))
                return (true, token.TrimEnd('.'));

            if (token.Contains('.'))
            {
                var parts = token.Split('.');
                return (true, parts[0]);
            }

            return (false, null);
        }
    }
}
