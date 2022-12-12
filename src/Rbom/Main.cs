namespace Dot.Rbom;

[Description(nameof(Str.TrimUtf8Bom))]
[Localization(typeof(Str))]
public sealed class Main : FilesTool<Option>
{
    private readonly byte[] _utf8Bom = { 0xef, 0xbb, 0xbf };


    private bool CloneFileWithoutBom(Stream fsr, ref string tempFile)
    {
        Span<byte> buffer  = stackalloc byte[_utf8Bom.Length];
        var        readLen = fsr.Read(buffer);
        if (readLen != _utf8Bom.Length || !buffer.SequenceEqual(_utf8Bom)) return false;

        using var fsw = CreateTempFile(out tempFile);
        int       data;
        while ((data = fsr.ReadByte()) != -1) fsw.WriteByte((byte)data);
        return true;
    }


    protected override async ValueTask FileHandle(string file, CancellationToken _)
    {
        ShowMessage(1, 0, 0);

        string tmpFile = default;
        await using (var fsr = OpenFileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            if (fsr is null) {
                ShowMessage(0, 0, 1);
                return;
            }

            if (CloneFileWithoutBom(fsr, ref tmpFile)) {
                if (Opt.WriteMode) File.Copy(tmpFile, file, true);
                ShowMessage(0, 1, 0);
                UpdateStats(Path.GetExtension(file));
            }
            else {
                ShowMessage(0, 0, 1);
            }
        }

        if (tmpFile != default) File.Delete(tmpFile);
    }
}
