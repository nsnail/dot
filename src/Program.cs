using System.Text;
using Dot.Git;
#if NET7_0_WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Dot;

internal static class Program
{
    public static int Main(string[] args)
    {
        CustomCulture(ref args);

        var app = new CommandApp();
        app.Configure(config => {
            config.AddCommand<Main>(nameof(Git).ToLower(CultureInfo.InvariantCulture));
            #if NET7_0_WINDOWS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                config.AddCommand<Color.Main>(nameof(Color).ToLower(CultureInfo.InvariantCulture));
                config.AddCommand<Tran.Main>(nameof(Tran).ToLower(CultureInfo.InvariantCulture));
            }
            #endif
            config.AddCommand<Guid.Main>(nameof(Guid).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<IP.Main>(nameof(IP).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Json.Main>(nameof(Json).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Pwd.Main>(nameof(Pwd).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Rbom.Main>(nameof(Rbom).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Trim.Main>(nameof(Trim).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Text.Main>(nameof(Text).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Time.Main>(nameof(Time).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<ToLf.Main>(nameof(ToLf).ToLower(CultureInfo.InvariantCulture));
            config.AddCommand<Get.Main>(nameof(Get).ToLower(CultureInfo.InvariantCulture));

            config.ValidateExamples();
        });
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return app.Run(args);
    }

    private static void CustomCulture(ref string[] args)
    {
        var i = Array.IndexOf(args, "/e");
        if (i < 0) {
            return;
        }

        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(args[i + 1]);
        var argsList                                              = args.ToList();
        argsList.RemoveAt(i);
        argsList.RemoveAt(i);
        args = argsList.ToArray();
    }
}