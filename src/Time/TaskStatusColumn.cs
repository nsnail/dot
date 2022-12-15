// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using NSExt.Extensions;
using Spectre.Console.Rendering;

namespace Dot.Time;

internal sealed class TaskStatusColumn : ProgressColumn
{
    public enum Statues : byte
    {
        // ReSharper disable once UnusedMember.Global
        [Description($"[gray]{nameof(Ready)}[/]")]
        Ready

      , [Description($"[yellow]{nameof(Connecting)}[/]")]
        Connecting

      , [Description($"[green]{nameof(Succeed)}[/]")]
        Succeed

      , [Description($"[red]{nameof(Failed)}[/]")]
        Failed
    }

    /// <summary>
    ///     Gets or sets the alignment of the task description.
    /// </summary>
    public Justify Alignment { get; set; } = Justify.Right;

    /// <inheritdoc />
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var text = task.State.Get<Statues>(nameof(TaskStatusColumn));
        return new Markup(text.Desc()).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}