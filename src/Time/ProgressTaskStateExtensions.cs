namespace Dot.Time;

internal static class ProgressTaskStateExtensions
{
    public static TimeSpan Result(this ProgressTaskState me)
    {
        return me.Get<TimeSpan>(nameof(TaskResultColumn));
    }

    public static void Result(this ProgressTaskState me, TimeSpan value)
    {
        me.Update<TimeSpan>(nameof(TaskResultColumn), _ => value);
    }

    public static TaskStatusColumn.Statues Status(this ProgressTaskState me)
    {
        return me.Get<TaskStatusColumn.Statues>(nameof(TaskStatusColumn));
    }

    public static void Status(this ProgressTaskState me, TaskStatusColumn.Statues value)
    {
        me.Update<TaskStatusColumn.Statues>(nameof(TaskStatusColumn), _ => value);
    }
}