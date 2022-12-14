using System.Text;
using Dot;
using Dot.Git;

var app = new CommandApp();

app.Configure(config => {
    config.SetApplicationName(AssemblyInfo.ASSEMBLY_PRODUCT);
    config.SetApplicationVersion(AssemblyInfo.ASSEMBLY_VERSION);

    config.AddCommand<Main>(nameof(Dot.Git).ToLower());
    #if NET7_0_WINDOWS
    config.AddCommand<Dot.Color.Main>(nameof(Dot.Color).ToLower());
    #endif
    config.AddCommand<Dot.Guid.Main>(nameof(Dot.Guid).ToLower());
    config.AddCommand<Dot.IP.Main>(nameof(Dot.IP).ToLower());
    config.AddCommand<Dot.Json.Main>(nameof(Dot.Json).ToLower());
    config.AddCommand<Dot.Pwd.Main>(nameof(Dot.Pwd).ToLower());
    config.AddCommand<Dot.Rbom.Main>(nameof(Dot.Rbom).ToLower());
    config.AddCommand<Dot.Trim.Main>(nameof(Dot.Trim).ToLower());
    config.AddCommand<Dot.Text.Main>(nameof(Dot.Text).ToLower());
    config.AddCommand<Dot.Time.Main>(nameof(Dot.Time).ToLower());
    config.AddCommand<Dot.ToLf.Main>(nameof(Dot.ToLf).ToLower());
    config.AddCommand<Dot.Get.Main>(nameof(Dot.Get).ToLower());

    config.ValidateExamples();
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
return app.Run(args);