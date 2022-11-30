using TextCopy;

namespace Dot.Guid;

public sealed class Main : Tool<Option>
{
    public Main(Option opt) : base(opt) { }


    public override void Run()
    {
        var guid            = System.Guid.NewGuid().ToString();
        if (Opt.Upper) guid = guid.ToUpper();
        ClipboardService.SetText(guid);
        Console.WriteLine($"已复制到剪贴板：{guid}");
    }
}