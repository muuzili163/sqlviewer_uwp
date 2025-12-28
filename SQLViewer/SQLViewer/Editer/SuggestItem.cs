using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public class SuggestItem
    {
        public string Text { get; set; } = "";
        public SuggestType Type { get; set; }
        public string? Description { get; set; }

        public override string ToString() => Text;
    }
}
