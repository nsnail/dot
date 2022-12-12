using System.Text;
using Dot;
using Dot.Git;

var app = new CommandApp();

app.Configure(config => {
    config.SetApplicationName(AssemblyInfo.ASSEMBLY_PRODUCT);
    config.SetApplicationVersion(AssemblyInfo.ASSEMBLY_VERSION);

    config.AddCommand<Main>("git");
    #if NET7_0_WINDOWS
    config.AddCommand<Dot.Color.Main>("color");
    #endif
    config.AddCommand<Dot.Guid.Main>("guid");
    config.AddCommand<Dot.IP.Main>("ip");
    config.AddCommand<Dot.Json.Main>("json");
    config.AddCommand<Dot.Pwd.Main>("pwd");
    config.AddCommand<Dot.RmBlank.Main>("rblank");
    config.AddCommand<Dot.RmBom.Main>("rbom");
    config.AddCommand<Dot.Text.Main>("text");
    config.AddCommand<Dot.Time.Main>("time");
    config.AddCommand<Dot.ToLf.Main>("tolf");
    config.AddCommand<Dot.Get.Main>("get");


    config.ValidateExamples();
});

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
return app.Run(args);