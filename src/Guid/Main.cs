// ReSharper disable ClassNeverInstantiated.Global

#if NET7_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Guid;

[Description(nameof(Str.GuidTool))]
[Localization(typeof(Str))]
internal sealed class Main : ToolBase<Option>
{
    protected override Task Core()
    {
        var guid = System.Guid.NewGuid().ToString();
        if (Opt.Upper) {
            guid = guid.ToUpper(CultureInfo.InvariantCulture);
        }

        Console.WriteLine(Str.Copied, guid);
        #if NET7_0_WINDOWS
        ClipboardService.SetText(guid);
        #endif
        return Task.CompletedTask;
    }
}