// ReSharper disable ClassNeverInstantiated.Global

using System.Text.Json;
using NSExt.Extensions;
#if NET8_0_WINDOWS
using TextCopy;
#endif

namespace Dot.Json;

[Description(nameof(Ln.Json工具))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private object _inputObj;

    protected override Task CoreAsync()
    {
        var inputText = Opt.InputText;

        #if NET8_0_WINDOWS
        if (inputText.NullOrWhiteSpace()) {
            inputText = ClipboardService.GetText();
        }
        #endif
        if (inputText.NullOrWhiteSpace()) {
            throw new ArgumentException(Ln.输入文本为空);
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

            throw new ArgumentException(Ln.剪贴板未包含正确的Json字符串);
        }

        return CoreInternalAsync();
    }

    private static string UnescapeString(string text)
    {
        return text.Replace("\\\"", "\"");
    }

    private async Task<string> ConvertToStringAsync()
    {
        var ret = await JsonCompressAsync().ConfigureAwait(false);
        return ret.Replace("\"", "\\\"");
    }

    private async Task CoreInternalAsync()
    {
        string result = null;
        if (Opt.Compress) {
            result = await JsonCompressAsync().ConfigureAwait(false);
        }
        else if (Opt.ConvertToString) {
            result = await ConvertToStringAsync().ConfigureAwait(false);
        }
        else if (Opt.Format) {
            result = await JsonFormatAsync().ConfigureAwait(false);
        }

        if (!result.NullOrWhiteSpace()) {
            #if NET8_0_WINDOWS
            await ClipboardService.SetTextAsync(result!).ConfigureAwait(false);
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