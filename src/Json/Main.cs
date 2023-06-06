// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json;
using NSExt.Extensions;
#if NET7_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Json;

[Description(nameof(Ln.Json))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private object _inputObj;

    protected override Task CoreAsync()
    {
        var inputText = Opt.InputText;

        #if NET7_0_WINDOWS
        if (inputText.NullOrWhiteSpace()) {
            inputText = ClipboardService.GetText();
        }
        #endif
        if (inputText.NullOrWhiteSpace()) {
            throw new ArgumentException(Ln.InputTextIsEmpty);
        }

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

            throw new ArgumentException(Ln.InvalidJsonString);
        }

        return CoreInternalAsync();
    }

    private static string UnescapeString(string text)
    {
        return text.Replace("\\\"", "\"");
    }

    private async Task<string> ConvertToStringAsync()
    {
        var ret = await JsonCompressAsync();
        return ret.Replace("\"", "\\\"");
    }

    private async Task CoreInternalAsync()
    {
        string result = null;
        if (Opt.Compress) {
            result = await JsonCompressAsync();
        }
        else if (Opt.ConvertToString) {
            result = await ConvertToStringAsync();
        }
        else if (Opt.Format) {
            result = await JsonFormatAsync();
        }

        if (!result.NullOrWhiteSpace()) {
            #if NET7_0_WINDOWS
            await ClipboardService.SetTextAsync(result!);
            #endif
        }
    }

    private Task<string> JsonCompressAsync()
    {
        var ret = _inputObj.Json();
        return Task.FromResult(ret);
    }

    private Task<string> JsonFormatAsync()
    {
        var ret = _inputObj.Json(new JsonSerializerOptions { WriteIndented = true });
        return Task.FromResult(ret);
    }
}