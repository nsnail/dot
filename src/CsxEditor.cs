using System.Diagnostics;

namespace Dot;

// ReSharper disable once UnusedType.Global
// ReSharper disable once UnusedMember.Global
internal static class CsxEditor
{
    // ReSharper disable once UnusedMember.Local
    #pragma warning disable S1144, RCS1213, IDE0051
    private static void Run()
        #pragma warning restore IDE0051, RCS1213, S1144
    {
        /*
            for %%i in (*.png) do pngquant %%i --force --output %%i --skip-if-larger
            for %%i in (*.jpg) do jpegtran -copy none -optimize -perfect %%i %%i
         *
         */

        var files = Directory.EnumerateFiles(".", "*.png"
                                           , new EnumerationOptions {
                                                                        RecurseSubdirectories = true
                                                                      , AttributesToSkip = FileAttributes.ReparsePoint
                                                                      , IgnoreInaccessible = true
                                                                    })
                             .ToArray();

        _ = Parallel.ForEach(files, file => {
            var startInfo = new ProcessStartInfo {
                                                     FileName = "pngquant"
                                                   , Arguments
                                                         = $"\"{file}\" --force --output \"{file}\" --skip-if-larger"
                                                 };
            using var p = Process.Start(startInfo);
            p!.WaitForExit();
            Console.WriteLine(p.ExitCode);
        });

        files = new[] { "*.jpg", "*.jpeg" }
                .SelectMany(x => Directory.EnumerateFiles(
                                ".", x
                              , new EnumerationOptions {
                                                           RecurseSubdirectories = true
                                                         , AttributesToSkip      = FileAttributes.ReparsePoint
                                                         , IgnoreInaccessible    = true
                                                       }))
                .ToArray();

        _ = Parallel.ForEach(files, file => {
            var startInfo = new ProcessStartInfo {
                                                     FileName  = "jpegtran"
                                                   , Arguments = $"-copy none -optimize -perfect \"{file}\" \"{file}\""
                                                 };
            using var p = Process.Start(startInfo);
            p!.WaitForExit();
            Console.WriteLine(p.ExitCode);
        });
    }
}