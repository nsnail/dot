// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Guid;

[Description(nameof(Ln.GuidTool))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    protected override Task CoreAsync()
    {
        var guid = System.Guid.NewGuid().ToString();
        if (Opt.Upper) {
            guid = guid.ToUpper(CultureInfo.InvariantCulture);
        }

        Console.WriteLine(Ln.Copied, guid);
        #if NET7_0_WINDOWS
        ClipboardService.SetText(guid);
        #endif
        return Task.CompletedTask;
    }
}