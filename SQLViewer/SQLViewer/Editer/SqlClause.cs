using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public enum SqlClause
    {
        Unknown,
        Select,
        From,
        Join,
        On,
        Where,
        GroupBy,
        OrderBy
    }
}
