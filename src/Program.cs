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


    config.ValidateExamples();
});

return app.Run(args);


/*using System.Reflection;
using System.Text;
using Dot;

Type[] LoadVerbs()
{
    return typeof(Program).Assembly.GetTypes()
                          //
                          .Where(x => x.GetCustomAttribute<VerbAttribute>() is not null)
                          .ToArray();
}


async Task Run(object args)
{
    if (args is not OptionBase option) return;
    var tool = ToolsFactory.Create(option);
    await tool.Run();
}


// Entry Point
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var types = LoadVerbs();

try {
    await Parser.Default.ParseArguments(args, types).WithParsedAsync(Run);
}
catch (ArgumentException ex) {
    AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
    return -1;
}

return 0;*/