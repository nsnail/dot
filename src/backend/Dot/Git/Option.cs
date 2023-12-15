// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Dot.Git;

internal sealed class Option : OptionBase
{
    [CommandOption("-a|--args")]
    [Description(nameof(Ln.传递给Git的参数))]
    [Localization(typeof(Ln))]
    [DefaultValue("status")]
    public string Args { get; set; }

    [CommandOption("-e|--git-output-encoding")]
    [Description(nameof(Ln.Git输出编码))]
    [Localization(typeof(Ln))]
    [DefaultValue("utf-8")]
    public string Git输出编码 { get; set; }

    [CommandOption("-d|--max-recursion-depth")]
    [Description(nameof(Ln.目录检索深度))]
    [Localization(typeof(Ln))]
    [DefaultValue(int.MaxValue)]
    public int MaxRecursionDepth { get; set; }

    [CommandArgument(0, "[PATH]")]
    [Description(nameof(Ln.要处理的目录路径))]
    [Localization(typeof(Ln))]
    [DefaultValue(".")]
    public string Path { get; set; }

    #if NET8_0_WINDOWS
    [CommandOption("-i|--install-right-click-menu")]
    [Description(nameof(Ln.添加Windows右键菜单))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool InstallRightClickMenu { get; set; }

    [CommandOption("-u|--uninstall-right-click-menu")]
    [Description(nameof(Ln.卸载Windows右键菜单))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool UninstallRightClickMenu { get; set; }

    [CommandOption("-o|--open-git-url")]
    [Description(nameof(Ln.打开Git仓库网址))]
    [Localization(typeof(Ln))]
    [DefaultValue(false)]
    public bool OpenGitUrl { get; set; }
    #endif
}