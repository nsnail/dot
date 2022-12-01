namespace Dot.TrimUtf8Bom;

[Verb("trim-utf8-bom", HelpText = "移除文件的uf8 bom")]
public class Option : IOption
{
    [Option('f', "filter", Required = false, HelpText = "文件通配符", Default = "*.*")]
    public string Filter { get; set; } //normal options here

    [Option('p', "path", Required = false, HelpText = "要处理的目录路径", Default = ".")]
    public string Path { get; set; }
}