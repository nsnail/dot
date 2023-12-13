// ReSharper disable ClassNeverInstantiated.Global

#if NET8_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Guid;

[Description(nameof(Ln.GUID工具))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    protected override Task CoreAsync()
    {
        var guid = System.Guid.NewGuid().ToString();
        if (Opt.Upper) {
            guid = guid.ToUpper(CultureInfo.InvariantCulture);
        }

        Console.WriteLine($"{Ln.已复制到剪贴板}: {guid}");
        #if NET8_0_WINDOWS
        ClipboardService.SetText(guid);
        #endif
        return Task.CompletedTask;
    }
}