using Dot.TrimUtf8Bom;

namespace Dot;

public static class ToolsFactory
{
    public static ITool Create(IOption option)
    {
        return option switch {
                   Option o                          => new Main(o)
                 , Convert2Lf.Option o               => new Convert2Lf.Main(o)
                 , RemoveTrailingWhiteSpace.Option o => new RemoveTrailingWhiteSpace.Main(o)
                 , Pwd.Option o                      => new Pwd.Main(o)
                 , Text.Option o                     => new Text.Main(o)
                 , Guid.Option o                     => new Guid.Main(o)
                 , _                                 => throw new ArgumentOutOfRangeException(nameof(option))
               };
    }
}