namespace Dot.RemoveTrailingWhiteSpace;

[Verb("remove-whitespace", HelpText = "移除文件尾部换行和空格")]
public class Option : IOption
{
    [Option('f', "filter", Required = false, HelpText = "文件通配符", Default = "*.*")]
    public string Filter { get; set; } //normal options here

    [Option('p', "path", Required = false, HelpText = "要处理的目录路径", Default = ".")]
    public string Path { get; set; }
}