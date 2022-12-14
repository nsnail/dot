using System.Diagnostics;

namespace Dot;

// ReSharper disable once UnusedType.Global
internal class CsxEditor
{
    // ReSharper disable once UnusedMember.Local
    #pragma warning disable CA1822
    private void Run()
        #pragma warning restore CA1822
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


        Parallel.ForEach(files, file => {
            var startInfo = new ProcessStartInfo {
                                                     FileName = "pngquant"
                                                   , Arguments
                                                         = $"\"{file}\" --force --output \"{file}\" --skip-if-larger"
                                                 };
            using var p = Process.Start(startInfo);
            p.WaitForExit();
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

        Parallel.ForEach(files, file => {
            var startInfo = new ProcessStartInfo {
                                                     FileName  = "jpegtran"
                                                   , Arguments = $"-copy none -optimize -perfect \"{file}\" \"{file}\""
                                                 };
            using var p = Process.Start(startInfo);
            p.WaitForExit();
            Console.WriteLine(p.ExitCode);
        });
    }
}