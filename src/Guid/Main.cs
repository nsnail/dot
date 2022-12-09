#if NET7_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Guid;

public sealed class Main : ToolBase<Option>
{
    public Main(Option opt) : base(opt) { }


    protected override Task Core()
    {
        var guid            = System.Guid.NewGuid().ToString();
        if (Opt.Upper) guid = guid.ToUpper();
        Console.WriteLine(Str.Copied, guid);
        #if NET7_0_WINDOWS
        ClipboardService.SetText(guid);
        #endif
        return Task.CompletedTask;
    }
}