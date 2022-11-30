namespace Dot.Convert2Lf;

[Verb("convert-lf", HelpText = "换行符转换为lf")]
public class Option : IOption
{
    [Option('f', "filter", Required = false, HelpText = "文件通配符", Default = "*.*")]
    public string Filter { get; set; } //normal options here

    [Option('p', "path", Required = false, HelpText = "要处理的目录路径", Default = ".")]
    public string Path { get; set; }
}