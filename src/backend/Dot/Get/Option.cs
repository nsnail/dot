// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dot.Get;

internal sealed class Option : OptionBase
{
    [CommandOption("-b|--buffer-size")]
    [Description(nameof(Ln.缓冲区大小_千字节))]
    [Localization(typeof(Ln))]
    [DefaultValue(8096)]
    public int 缓冲区大小_千字节 { get; set; }

    [CommandArgument(0, "<URL>")]
    [Description(nameof(Ln.网络地址))]
    [Localization(typeof(Ln))]
    public string 网络地址 { get; set; }

    [CommandOption("-c|--chunk-number")]
    [Description(nameof(Ln.下载分块数))]
    [Localization(typeof(Ln))]
    [DefaultValue(5)]
    public int 下载分块数 { get; set; }

    [CommandOption("-m|--max-parallel")]
    [Description(nameof(Ln.最大并发数量))]
    [Localization(typeof(Ln))]
    [DefaultValue(5)]
    public int 最大并发数量 { get; set; }

    [CommandOption("-o|--output")]
    [Description(nameof(Ln.输出文件路径))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Output { get; set; }
}