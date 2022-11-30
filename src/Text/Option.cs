namespace Dot.Text;

[Verb("text", HelpText = "文本编码工具")]
public class Option : IOption
{
    [Value(0, MetaName = "文本", HelpText = "要处理的文本，不指定此参数：取剪贴板值")]
    public string Text { get; set; }
}