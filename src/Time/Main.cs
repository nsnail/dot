using System.Net.Sockets;

namespace Dot.Time;

[Description(nameof(Str.TimeTool))]
[Localization(typeof(Str))]
public sealed class Main : ToolBase<Option>
{
    private const int _MAX_DEGREE_OF_PARALLELISM = 10;
    private const int _NTP_PORT                  = 123;

    private readonly string[] _ntpServers = {
                                                "ntp.ntsc.ac.cn", "cn.ntp.org.cn", "edu.ntp.org.cn", "cn.pool.ntp.org"
                                              , "time.pool.aliyun.com", "time1.aliyun.com", "time2.aliyun.com"
                                              , "time3.aliyun.com", "time4.aliyun.com", "time5.aliyun.com"
                                              , "time6.aliyun.com", "time7.aliyun.com", "time1.cloud.tencent.com"
                                              , "time2.cloud.tencent.com", "time3.cloud.tencent.com"
                                              , "time4.cloud.tencent.com", "time5.cloud.tencent.com", "ntp.sjtu.edu.cn"
                                              , "ntp.neu.edu.cn", "ntp.bupt.edu.cn", "ntp.shu.edu.cn", "pool.ntp.org"
                                              , "0.pool.ntp.org", "1.pool.ntp.org", "2.pool.ntp.org", "3.pool.ntp.org"
                                              , "asia.pool.ntp.org", "time1.google.com", "time2.google.com"
                                              , "time3.google.com", "time4.google.com", "time.apple.com"
                                              , "time1.apple.com", "time2.apple.com", "time3.apple.com"
                                              , "time4.apple.com", "time5.apple.com", "time6.apple.com"
                                              , "time7.apple.com", "time.windows.com", "time.nist.gov"
                                              , "time-nw.nist.gov", "time-a.nist.gov", "time-b.nist.gov"
                                              , "stdtime.gov.hk"
                                            };

    private double _offsetAvg;


    private int _successCnt;


    private TimeSpan GetNtpOffset(string server)
    {
        Span<byte> ntpData = stackalloc byte[48];
        ntpData[0] = 0x1B;
        using var socket
            = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {
                  ReceiveTimeout = Opt.Timeout
              };


        try {
            socket.Connect(server, _NTP_PORT);
            socket.Send(ntpData);
            var timeBefore = DateTime.Now;
            socket.Receive(ntpData);
            var transferTime = DateTime.Now - timeBefore;

            var intPart = ((ulong)ntpData[40]   << 24) //
                          | ((ulong)ntpData[41] << 16) //
                          | ((ulong)ntpData[42] << 8)  //
                          | ntpData[43];
            var fractPart = ((ulong)ntpData[44]   << 24) //
                            | ((ulong)ntpData[45] << 16) //
                            | ((ulong)ntpData[46] << 8)  //
                            | ntpData[47];
            var from1900Ms = intPart * 1000 + fractPart * 1000 / 0x100000000L;
            var onlineTime = new DateTime(1900, 1, 1).AddMilliseconds((long)from1900Ms) + transferTime / 2;
            return DateTime.UtcNow - onlineTime;
        }
        catch (Exception) {
            return TimeSpan.Zero;
        }
        finally {
            socket.Close();
        }
    }

    private ValueTask ServerHandle(KeyValuePair<string, ProgressTask> payload, CancellationToken _)
    {
        payload.Value.StartTask();
        payload.Value.State.Status(TaskStatusColumn.Statues.Connecting);
        var offset = GetNtpOffset(payload.Key);
        if (offset == TimeSpan.Zero) {
            payload.Value.State.Status(TaskStatusColumn.Statues.Failed);
        }
        else {
            payload.Value.State.Status(TaskStatusColumn.Statues.Succeed);
            payload.Value.State.Result(offset);
            Interlocked.Increment(ref _successCnt);
        }

        payload.Value.StopTask();
        return ValueTask.CompletedTask;
    }


    private static void SetSysteTime(DateTime time)
    {
        var timeToSet = new Win32.Systemtime {
                                                 wDay          = (ushort)time.Day
                                               , wDayOfWeek    = (ushort)time.DayOfWeek
                                               , wHour         = (ushort)time.Hour
                                               , wMilliseconds = (ushort)time.Millisecond
                                               , wMinute       = (ushort)time.Minute
                                               , wMonth        = (ushort)time.Month
                                               , wSecond       = (ushort)time.Second
                                               , wYear         = (ushort)time.Year
                                             };
        Win32.SetLocalTime(timeToSet);
    }


    protected override async Task Core()
    {
        await AnsiConsole.Progress()
                         .Columns(new TaskDescriptionColumn() //
                                , new ProgressBarColumn()     //
                                , new ElapsedTimeColumn()     //
                                , new SpinnerColumn()         //
                                , new TaskStatusColumn()      //
                                , new TaskResultColumn())
                         .StartAsync(async ctx => {
                             var tasks = _ntpServers.ToDictionary(server => server
                                                                , server => ctx.AddTask(server, false)
                                                                               .IsIndeterminate());

                             await Parallel.ForEachAsync(
                                 tasks, new ParallelOptions { MaxDegreeOfParallelism = _MAX_DEGREE_OF_PARALLELISM }
                               , ServerHandle);

                             _offsetAvg = tasks.Where(x => x.Value.State.Status() == TaskStatusColumn.Statues.Succeed)
                                               .Average(x => x.Value.State.Result().TotalMilliseconds);
                         });

        AnsiConsole.MarkupLine(Str.NtpReceiveDone, $"[green]{_successCnt}[/]", _ntpServers.Length
                             , $"[yellow]{_offsetAvg:f2}[/]");


        if (Opt.Sync) {
            SetSysteTime(DateTime.Now.AddMilliseconds(-_offsetAvg));
            AnsiConsole.MarkupLine($"[green]{Str.LocalTimeSyncDone}[/]");
        }
    }

    protected override async Task Run()
    {
        await Core();
        if (Opt.KeepSession) {
            var table = new Table().HideHeaders()
                                   .AddColumn(new TableColumn(string.Empty))
                                   .AddColumn(new TableColumn(string.Empty))
                                   .Caption(Str.PressAnyKey)
                                   .AddRow(Str.NtpClock,   DateTime.Now.AddMilliseconds(-_offsetAvg).ToString("O"))
                                   .AddRow(Str.LocalClock, DateTime.Now.ToString("O"));

            var cts = new CancellationTokenSource();
            var task = AnsiConsole.Live(table)
                                  .StartAsync(async ctx => {
                                      while (!cts.IsCancellationRequested) {
                                          ctx.UpdateTarget(
                                              table.UpdateCell(
                                                       0, 1, DateTime.Now.AddMilliseconds(-_offsetAvg).ToString("O"))
                                                   .UpdateCell(1, 1, DateTime.Now.ToString("O")));
                                          await Task.Delay(100);
                                      }
                                  });

            await AnsiConsole.Console.Input.ReadKeyAsync(true, cts.Token);
            cts.Cancel();
            await task;
        }
    }
}