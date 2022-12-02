namespace Dot.Time;

[Verb("time", HelpText = nameof(Str.HelpForText), ResourceType = typeof(Str))]
public class Option : IOption
{
    [Option('s', "sync", HelpText = nameof(Str.SyncToLocalTime), Default = false, ResourceType = typeof(Str))]
    public bool Sync { get; set; }

    [Option('t', "timeout", HelpText = nameof(Str.TimeoutMillSecs), Default = 3000, ResourceType = typeof(Str))]
    public int Timeout { get; set; }
}