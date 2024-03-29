// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Rbom;

[Description(nameof(Ln.移除文件的UTF8_BOM))]
[Localization(typeof(Ln))]
internal sealed class Main : FilesTool<Option>
{
    private readonly byte[] _utf8Bom = { 0xef, 0xbb, 0xbf };

    protected override async ValueTask FileHandleAsync(string file, CancellationToken cancelToken)
    {
        ShowMessage(1, 0, 0);

        string tmpFile = default;
        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }

            if (CloneFileWithoutBom(fsr, ref tmpFile)) {
                if (Opt.WriteMode) {
                    File.Copy(tmpFile, file, true);
                }

                ShowMessage(0, 1, 0);
                UpdateStats(Path.GetExtension(file));
            }
            else {
                ShowMessage(0, 0, 1);
            }
        }

        if (tmpFile != default) {
            File.Delete(tmpFile);
        }
    }

    private bool CloneFileWithoutBom(FileStream fsr, ref string tempFile)
    {
        Span<byte> buffer  = stackalloc byte[_utf8Bom.Length];
        var        readLen = fsr.Read(buffer);
        if (readLen != _utf8Bom.Length || !buffer.SequenceEqual(_utf8Bom)) {
            return false;
        }

        using var fsw = CreateTempFile(out tempFile);
        int       data;

        while ((data = fsr.ReadByte()) != -1) {
            fsw.WriteByte((byte)data);
        }

        return true;
    }
}