using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public sealed class SqlContext
    {

        public int CaretIndex { get; init; }
        public int TokenStartIndex { get; init; }
        public int TokenLength { get; init; }

        public SqlClause Clause { get; init; }

        public string CurrentToken { get; init; } = string.Empty;

        public bool IsAliasMemberAccess { get; init; }   // 是否 u.
        public string? CurrentAlias { get; init; }       // u

        public IReadOnlyList<string> Tables { get; init; } =
            Array.Empty<string>();

        public IReadOnlyDictionary<string, string> Aliases { get; init; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
