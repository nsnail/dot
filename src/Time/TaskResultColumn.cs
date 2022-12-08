using Spectre.Console.Rendering;

namespace Dot.Time;

public class TaskResultColumn : ProgressColumn
{
    /// <summary>
    ///     Gets or sets the alignment of the task description.
    /// </summary>
    public Justify Alignment { get; set; } = Justify.Right;

    /// <inheritdoc />
    public override IRenderable Render(RenderOptions options, ProgressTask task, TimeSpan deltaTime)
    {
        var text = task.State.Get<TimeSpan>(nameof(TaskResultColumn));
        return new Markup(text.ToString()).Overflow(Overflow.Ellipsis).Justify(Alignment);
    }
}