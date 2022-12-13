using System.Diagnostics;
using System.Security.Cryptography;

namespace Dot.Tests;

public class TestGet
{
    private static string GetFileSha1(string file)
    {
        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return BitConverter.ToString(SHA1.HashData(fs));
    }

    [Test]
    public void DownloadFile()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tmp");
        try {
            using var p = Process.Start(new ProcessStartInfo //
                                        {
                                            FileName  = "../../../dot/bin/net7.0-windows/dot.exe"
                                          , Arguments = $"get http://dl.360safe.com/360zip_setup.exe -o \"{file}\""
                                        });
            p!.WaitForExit();

            Assert.That(
                p.ExitCode == 0 && "6C2ADC1F69281ABBD2ED7D6782A208FAA621C868" ==
                GetFileSha1(file).Replace("-", string.Empty), Is.True);
        }
        catch (Exception) {
            File.Delete(file);
            throw;
        }
    }

    [SetUp]
    public void Setup() { }
}