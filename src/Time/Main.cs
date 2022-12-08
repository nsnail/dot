using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Dot.Color;

namespace Dot.Time;

public sealed class Main : ToolBase<Option>
{
    private record Server
    {
        public int           ConsoleRowIndex;
        public TimeSpan      Offset;
        public ServerStatues Status;
    }

    private enum ServerStatues : byte
    {
        Ready
      , Connecting
      , Succeed
      , Failed
    }


    private const           int    _MAX_DEGREE_OF_PARALLELISM = 10;
    private const           int    _NTP_PORT                  = 123;
    private const           string _OUTPUT_TEMP               = "{0,-30}         {1,20}    {2,20}";
    private static readonly object _lock                      = new();
    private                 int    _procedCnt;
    private readonly        int    _serverCnt;

    private readonly string[] _srvAddr = {
                                             "ntp.ntsc.ac.cn", "cn.ntp.org.cn", "edu.ntp.org.cn", "cn.pool.ntp.org"
                                           , "time.pool.aliyun.com", "time1.aliyun.com", "time2.aliyun.com"
                                           , "time3.aliyun.com", "time4.aliyun.com", "time5.aliyun.com"
                                           , "time6.aliyun.com", "time7.aliyun.com", "time1.cloud.tencent.com"
                                           , "time2.cloud.tencent.com", "time3.cloud.tencent.com"
                                           , "time4.cloud.tencent.com", "time5.cloud.tencent.com", "ntp.sjtu.edu.cn"
                                           , "ntp.neu.edu.cn", "ntp.bupt.edu.cn", "ntp.shu.edu.cn", "pool.ntp.org"
                                           , "0.pool.ntp.org", "1.pool.ntp.org", "2.pool.ntp.org", "3.pool.ntp.org"
                                           , "asia.pool.ntp.org", "time1.google.com", "time2.google.com"
                                           , "time3.google.com", "time4.google.com", "time.apple.com", "time1.apple.com"
                                           , "time2.apple.com", "time3.apple.com", "time4.apple.com", "time5.apple.com"
                                           , "time6.apple.com", "time7.apple.com", "time.windows.com", "time.nist.gov"
                                           , "time-nw.nist.gov", "time-a.nist.gov", "time-b.nist.gov", "stdtime.gov.hk"
                                         };

    private readonly Dictionary<string, Server> _srvStatus;
    private          int                        _successCnt;


    public Main(Option opt) : base(opt)
    {
        _serverCnt = _srvAddr.Length;
        var i = 0;
        _srvStatus = _srvAddr.ToDictionary(
            x => x, _ => new Server { Status = ServerStatues.Ready, ConsoleRowIndex = ++i });
    }

    private static void ChangeStatus(KeyValuePair<string, Server> server, ServerStatues status
                                   , TimeSpan                     offset = default)
    {
        server.Value.Status = status;
        server.Value.Offset = offset;
        DrawTextInConsole(0, server.Value.ConsoleRowIndex
                     ,       string.Format(_OUTPUT_TEMP, server.Key, server.Value.Status
                                      , status == ServerStatues.Succeed ? server.Value.Offset : string.Empty));
    }

    private async Task DrawLoading()
    {
        char[] loading      = { '-', '\\', '|', '/' };
        var    loadingIndex = 0;
        while (true) {
            if (Volatile.Read(ref _procedCnt) == _serverCnt) break;
            await Task.Delay(100);
            ++loadingIndex;
            for (var i = 0; i != _serverCnt; ++i)
                DrawTextInConsole(
                    34, i + 1
              ,         _srvStatus[_srvAddr[i]].Status is ServerStatues.Succeed or ServerStatues.Failed
                        ? " "
                        : loading[loadingIndex % 4].ToString());
        }

        Debug.WriteLine(Environment.CurrentManagedThreadId + ":" + DateTime.Now.ToString("O"));
    }

    private static void DrawTextInConsole(int left, int top, string text)
    {
        lock (_lock) {
            Console.SetCursorPosition(left, top);
            Console.Write(text);
        }
    }


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

    private void PrintTemplate()
    {
        Console.Clear();
        Console.CursorVisible = false;
        var row =               //
            _srvStatus.Select(x //
                                  => string.Format(_OUTPUT_TEMP, x.Key, x.Value.Status
                                                 , x.Value.Offset == TimeSpan.Zero ? string.Empty : x.Value.Offset));


        Console.WriteLine(_OUTPUT_TEMP, Str.Server, Str.Status, Str.LocalClockOffset);
        Console.WriteLine(string.Join(Environment.NewLine, row));
    }

    private ValueTask ServerHandle(KeyValuePair<string, Server> server)
    {
        ChangeStatus(server, ServerStatues.Connecting);
        var offset = GetNtpOffset(server.Key);
        Interlocked.Increment(ref _procedCnt);

        if (offset == TimeSpan.Zero) {
            ChangeStatus(server, ServerStatues.Failed);
        }
        else {
            Interlocked.Increment(ref _successCnt);
            ChangeStatus(server, ServerStatues.Succeed, offset);
        }

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

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public override async Task Run()
    {
        PrintTemplate();
        var tLoading = DrawLoading();


        await Parallel.ForEachAsync(_srvStatus
                                  , new ParallelOptions { MaxDegreeOfParallelism = _MAX_DEGREE_OF_PARALLELISM }
                                  , (server, _) => ServerHandle(server));

        await tLoading;


        var avgOffset = TimeSpan.FromTicks((long)_srvStatus //
                                                 .Where(x => x.Value.Status == ServerStatues.Succeed)
                                                 .Average(x => x.Value.Offset.Ticks));

        Console.SetCursorPosition(0, _serverCnt + 1);
        Console.WriteLine(Str.NtpReceiveDone, _successCnt, _serverCnt, avgOffset.TotalMilliseconds);

        if (!Opt.Sync) {
            if (!Opt.KeepSession) return;

            var waitObj = new ManualResetEvent(false);
            var _ = Task.Run(async () => {
                var top = Console.GetCursorPosition().Top;
                while (true) {
                    Console.SetCursorPosition(0, top);
                    Console.Write(Str.NtpServerTime, (DateTime.Now - avgOffset).ToString("O"));
                    waitObj.Set();
                    await Task.Delay(100);
                }
                // ReSharper disable once FunctionNeverReturns
            });
            waitObj.WaitOne();
            return;
        }

        SetSysteTime(DateTime.Now - avgOffset);
        Console.WriteLine(Str.LocalTimeSyncDone);
    }
}