namespace Dot.Git;

public static class ProgressTaskStateExtensions
{
    public static TaskStatusColumn.Statues Status(this ProgressTaskState me)
    {
        return me.Get<TaskStatusColumn.Statues>(nameof(TaskStatusColumn));
    }

    public static void Status(this ProgressTaskState me, TaskStatusColumn.Statues value)
    {
        me.Update<TaskStatusColumn.Statues>(nameof(TaskStatusColumn), _ => value);
    }
}