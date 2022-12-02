using System.Reflection;
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
    var tool = ToolsFactory.Create(args as IOption);
    await tool.Run();
}


//Entry Point

var types = LoadVerbs();

try {
    await Parser.Default.ParseArguments(args, types).WithParsedAsync(Run);
}
catch (ArgumentException ex) {
    Console.Error.WriteLine(ex.Message);
    return -1;
}

return 0;