// ReSharper disable ClassNeverInstantiated.Global

using NSExt.Extensions;

namespace Dot.Trim;

[Description(nameof(Ln.RemoveTrailingWhiteSpaces))]
[Localization(typeof(Ln))]
internal sealed class Main : FilesTool<Option>
{
    protected override async ValueTask FileHandle(string file, CancellationToken cancelToken)
    {
        ShowMessage(1, 0, 0);
        int spacesCnt;

        await using var fsrw = OpenFileStream(file, FileMode.Open, FileAccess.ReadWrite);

        if (fsrw is null || fsrw.Length == 0 || (spacesCnt = GetSpacesCnt(fsrw)) == 0) {
            ShowMessage(0, 0, 1);
            return;
        }

        _ = fsrw.Seek(0, SeekOrigin.Begin);
        if (!fsrw.IsTextStream()) {
            ShowMessage(0, 0, 1);
            return;
        }

        if (Opt.WriteMode) {
            fsrw.SetLength(fsrw.Length - spacesCnt);
        }

        ShowMessage(0, 1, 0);
        UpdateStats(Path.GetExtension(file));
    }

    private static int GetSpacesCnt(Stream fsr)
    {
        var trimLen = 0;
        _ = fsr.Seek(-1, SeekOrigin.End);
        int data;
        while ((data = fsr.ReadByte()) != -1) {
            if (new[] { 0x20, 0x0d, 0x0a }.Contains(data)) {
                ++trimLen;
                if (fsr.Position - 2 < 0) {
                    break;
                }

                _ = fsr.Seek(-2, SeekOrigin.Current);
            }
            else {
                break;
            }
        }

        return trimLen;
    }
}