using System.Reflection;
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
    if (option!.KeepSession) {
        Console.WriteLine();
        Console.WriteLine(Str.PressAnyKey);
        Console.ReadKey();
    }
}


// Entry Point
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var types = LoadVerbs();

try {
    await Parser.Default.ParseArguments(args, types).WithParsedAsync(Run);
}
catch (ArgumentException ex) {
    Console.Error.WriteLine(ex.Message);
    return -1;
}

return 0;