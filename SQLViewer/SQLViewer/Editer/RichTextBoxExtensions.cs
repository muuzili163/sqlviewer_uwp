using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer.Editer
{
    public static class RichTextBoxExtensions
    {
        public static void ReplaceToken(
            this RichTextBox rtb,
            int tokenStart,
            int tokenLength,
            string insertText)
        {
            rtb.SuspendLayout();

            rtb.SelectionStart = tokenStart;
            rtb.SelectionLength = tokenLength;
            rtb.SelectedText = insertText;

            rtb.SelectionStart = tokenStart + insertText.Length;
            rtb.SelectionLength = 0;

            rtb.ResumeLayout();
        }
    }
}
