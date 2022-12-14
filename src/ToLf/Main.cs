// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.ToLf;

[Description(nameof(Str.ConvertEndOfLineToLF))]
[Localization(typeof(Str))]
internal sealed class Main : FilesTool<Option>
{
    protected override async ValueTask FileHandle(string file, CancellationToken cancelToken)
    {
        ShowMessage(1, 0, 0);

        var    hasWrote = false;
        var    isBin    = false;
        string tmpFile;
        // ReSharper disable once TooWideLocalVariableScope
        int data;

        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }

            await using var fsw = CreateTempFile(out tmpFile);

            while ((data = fsr.ReadByte()) != -1) {
                switch (data) {
                    case 0x0d when fsr.ReadByte() == 0x0a: // crlf windows
                        fsw.WriteByte(0x0a);
                        hasWrote = true;
                        continue;
                    case 0x0d: //cr macos
                        fsw.WriteByte(0x0a);
                        fsr.Seek(-1, SeekOrigin.Current);
                        hasWrote = true;
                        continue;
                    case 0x00 or 0xff: //非文本文件
                        isBin = true;
                        break;
                    default:
                        fsw.WriteByte((byte)data);
                        continue;
                }

                break;
            }
        }

        #pragma warning disable S2583
        if (hasWrote && !isBin) {
            #pragma warning restore S2583
            if (Opt.WriteMode) File.Copy(tmpFile, file, true);
            ShowMessage(0, 1, 0);
            UpdateStats(Path.GetExtension(file));
        }
        else {
            ShowMessage(0, 0, 1);
        }

        File.Delete(tmpFile);
    }
}