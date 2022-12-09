using NSExt.Extensions;

namespace Dot.RmBlank;

[Description(nameof(Str.RemoveTrailingWhiteSpaces))]
[Localization(typeof(Str))]
public sealed class Main : FilesTool<Option>
{
    private static int GetSpacesCnt(Stream fsr)
    {
        var trimLen = 0;
        fsr.Seek(-1, SeekOrigin.End);
        int data;
        while ((data = fsr.ReadByte()) != -1)
            if (new[] { 0x20, 0x0d, 0x0a }.Contains(data)) {
                ++trimLen;
                if (fsr.Position - 2 < 0) break;
                fsr.Seek(-2, SeekOrigin.Current);
            }
            else {
                break;
            }

        return trimLen;
    }


    protected override async ValueTask FileHandle(string file, CancellationToken _)
    {
        ShowMessage(1, 0, 0);
        int spacesCnt;

        await using var fsrw = OpenFileStream(file, FileMode.Open, FileAccess.ReadWrite);


        if (fsrw is null || fsrw.Length == 0 || (spacesCnt = GetSpacesCnt(fsrw)) == 0) {
            ShowMessage(0, 0, 1);
            return;
        }

        fsrw.Seek(0, SeekOrigin.Begin);
        if (!fsrw.IsTextStream()) {
            ShowMessage(0, 0, 1);
            return;
        }


        if (Opt.WriteMode) fsrw.SetLength(fsrw.Length - spacesCnt);
        ShowMessage(0, 1, 0);
        UpdateStats(Path.GetExtension(file));
    }
}