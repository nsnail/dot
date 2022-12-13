namespace Dot.Git;

internal static class ProgressTaskStateExtensions
{
    public static void Status(this ProgressTaskState me, TaskStatusColumn.Statues value)
    {
        me.Update<TaskStatusColumn.Statues>(nameof(TaskStatusColumn), _ => value);
    }
}