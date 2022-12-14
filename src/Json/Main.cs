// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json;
using NSExt.Extensions;
#if NET7_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Json;

[Description(nameof(Str.Json))]
[Localization(typeof(Str))]
internal class Main : ToolBase<Option>
{
    private object _inputObj;

    private async Task<string> ConvertToString()
    {
        var ret = await JsonCompress();
        ret = ret.Replace("\"", "\\\"");
        return ret;
    }

    private async Task CoreInternal()
    {
        string result = null;
        if (Opt.Compress)
            result = await JsonCompress();
        else if (Opt.ConvertToString)
            result                  = await ConvertToString();
        else if (Opt.Format) result = await JsonFormat();

        if (!result.NullOrWhiteSpace()) {
            #if NET7_0_WINDOWS
            await ClipboardService.SetTextAsync(result!);
            #endif
        }
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

    protected override Task Core()
    {
        var inputText = Opt.InputText;

        #if NET7_0_WINDOWS
        if (inputText.NullOrWhiteSpace()) inputText = ClipboardService.GetText();
        #endif
        if (inputText.NullOrWhiteSpace()) throw new ArgumentException(Str.InputTextIsEmpty);

        try {
            _inputObj = inputText.Object<object>();
        }
        catch (JsonException) {
            try {
                inputText = UnescapeString(inputText);
                _inputObj = inputText.Object<object>();
                return Task.CompletedTask;
            }
            catch (JsonException) {
                // ignored
            }

            throw new ArgumentException(Str.InvalidJsonString);
        }

        return CoreInternal();
    }
}