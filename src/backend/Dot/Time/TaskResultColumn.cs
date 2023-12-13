// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using Spectre.Console.Rendering;

namespace Dot.Time;

internal sealed class TaskResultColumn : ProgressColumn
{
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
        var text = task.State.Get<TimeSpan>(nameof(TaskResultColumn));
        return new Markup(text.ToString()).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}