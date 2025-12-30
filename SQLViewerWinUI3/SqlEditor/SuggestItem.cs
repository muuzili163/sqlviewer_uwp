namespace SQLViewerWinUI3.SqlEditor
{
    public class SuggestItem
    {
        public string Text { get; set; } = "";
        public SuggestType Type { get; set; }
        public string? Description { get; set; }

        public override string ToString() => Text;
    }
}
