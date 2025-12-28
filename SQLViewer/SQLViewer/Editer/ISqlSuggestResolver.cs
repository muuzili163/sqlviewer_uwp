using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public interface ISqlSuggestResolver
    {
        Task<IReadOnlyList<SuggestItem>> ResolveAsync(SqlContext context);
    }
}
