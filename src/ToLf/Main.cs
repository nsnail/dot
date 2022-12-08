namespace Dot.ToLf;

public sealed class Main : FilesTool<Option>
{
    public Main(Option opt) : base(opt) { }

    protected override async ValueTask FileHandle(string file, CancellationToken _)
    {
        ShowMessage(1, 0, 0);

        var    hasWrote = false;
        var    isBin    = false;
        string tmpFile;
        int    data;

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


        if (hasWrote && !isBin) {
            if (Opt.WriteMode) CopyFile(tmpFile, file);
            ShowMessage(0, 1, 0);
            UpdateStats(Path.GetExtension(file));
        }
        else {
            ShowMessage(0, 0, 1);
        }

        File.Delete(tmpFile);
    }
}