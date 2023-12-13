// ReSharper disable UnusedAutoPropertyAccessor.Global

global using Spectre.Console;
global using Spectre.Console.Cli;

namespace Dot;

internal abstract class OptionBase : CommandSettings, IOption
{
    [CommandOption("-k|--keep--session")]
    [Description(nameof(Ln.执行命令后保留会话))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool 执行命令后保留会话 { get; set; }
}