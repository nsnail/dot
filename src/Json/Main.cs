using System.Text.Json;
using NSExt.Extensions;
using TextCopy;

namespace Dot.Json;

public class Main : ToolBase<Option>
{
    private readonly object _inputObj;

    public Main(Option opt) : base(opt)
    {
        var inputText = ClipboardService.GetText();
        if (inputText.NullOrWhiteSpace()) throw new ArgumentException(Str.InputTextIsEmpty);

        try {
            _inputObj = inputText.Object<object>();
        }
        catch (JsonException) {
            try {
                inputText = UnescapeString(inputText);
                _inputObj = inputText.Object<object>();
                return;
            }
            catch (JsonException) { }

            throw new ArgumentException(Str.InvalidJsonString);
        }
    }


    private async Task<string> ConvertToString()
    {
        var ret = await JsonCompress();
        ret = ret.Replace("\"", "\\\"");
        return ret;
    }

    private Task<string> JsonCompress()
    {
        var ret = _inputObj.Json();
        return Task.FromResult(ret);
    }

    private Task<string> JsonFormat()
    {
        var ret = _inputObj.Json(true);
        return Task.FromResult(ret);
    }

    private static string UnescapeString(string text)
    {
        return text.Replace("\\\"", "\"");
    }

    public override async Task Run()
    {
        string result = null;
        if (Opt.Compress)
            result = await JsonCompress();
        else if (Opt.ConvertToString)
            result                  = await ConvertToString();
        else if (Opt.Format) result = await JsonFormat();

        if (result.NullOrWhiteSpace()) return;
        await ClipboardService.SetTextAsync(result!);
    }
}