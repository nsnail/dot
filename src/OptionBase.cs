// ReSharper disable UnusedAutoPropertyAccessor.Global

global using Spectre.Console;
global using Spectre.Console.Cli;

namespace Dot;

internal abstract class OptionBase : CommandSettings, IOption
{
    [CommandOption("-k|--keep--session")]
    [Description(nameof(Ln.KeepSession))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool KeepSession { get; set; }
}