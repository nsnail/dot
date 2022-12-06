using Dot.RmBom;

namespace Dot;

public static class ToolsFactory
{
    public static ITool Create(IOption option)
    {
        return option switch {
                   Option o         => new Main(o)
                 , ToLf.Option o    => new ToLf.Main(o)
                 , RmBlank.Option o => new RmBlank.Main(o)
                 , Pwd.Option o     => new Pwd.Main(o)
                 , Text.Option o    => new Text.Main(o)
                 , Guid.Option o    => new Guid.Main(o)
                 , Time.Option o    => new Time.Main(o)
                 , Color.Option o   => new Color.Main(o)
                 , IP.Option o      => new IP.Main(o)
                 , Git.Option o     => new Git.Main(o)
                 , _                => throw new ArgumentOutOfRangeException(nameof(option))
               };
    }
}