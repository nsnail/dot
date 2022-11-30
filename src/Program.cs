using System.Reflection;
using Dot;

Type[] LoadVerbs()
{
    return typeof(Program).Assembly.GetTypes()
                          //
                          .Where(x => x.GetCustomAttribute<VerbAttribute>() is not null)
                          .ToArray();
}


void Run(object args)
{
    var tool = ToolsFactory.Create(args as IOption);
    tool.Run();
}


//Entry Point

var types = LoadVerbs();

try {
    Parser.Default.ParseArguments(args, types).WithParsed(Run);
}
catch (ArgumentException ex) {
    Console.Error.WriteLine(ex.Message);
    return -1;
}

return 0;