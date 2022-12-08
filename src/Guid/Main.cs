using TextCopy;

namespace Dot.Guid;

public sealed class Main : ToolBase<Option>
{
    public Main(Option opt) : base(opt) { }


    protected override Task Core()
    {
        var guid            = System.Guid.NewGuid().ToString();
        if (Opt.Upper) guid = guid.ToUpper();
        ClipboardService.SetText(guid);
        Console.WriteLine(Str.Copied, guid);
        return Task.CompletedTask;
    }
}