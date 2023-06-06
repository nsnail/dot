// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using NSExt.Extensions;
using Spectre.Console.Rendering;

namespace Dot.Git;

internal sealed class TaskStatusColumn : ProgressColumn
{
    public enum Statues : byte
    {
        /// <summary>
        ///     Ready
        /// </summary>
        [Description($"[gray]{nameof(Ready)}[/]")]
        Ready = 0

       ,

        /// <summary>
        ///     Executing
        /// </summary>
        [Description($"[yellow]{nameof(Executing)}[/]")]
        Executing = 1

       ,

        /// <summary>
        ///     Succeed
        /// </summary>
        [Description($"[green]{nameof(Succeed)}[/]")]
        Succeed = 2

       ,

        /// <summary>
        ///     Failed
        /// </summary>
        [Description($"[red]{nameof(Failed)}[/]")]
        Failed = 3
    }

    /// <summary>
    ///     Gets or sets the alignment of the task description.
    /// </summary>
    /// <value>
    ///     The alignment of the task description.
    /// </value>
    public Justify Alignment { get; set; } = Justify.Right;

    /// <inheritdoc />
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var text = task.State.Get<Statues>(nameof(TaskStatusColumn));
        return new Markup(text.ResDesc<Ln>()).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}