using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Dot.Time;

public sealed class Main : Tool<Option>
{
    private enum ServerStatues : byte
    {
        Ready
      , Connecting
      , Succeed
      , Failed
    }

    [StructLayout(LayoutKind.Explicit)]
    private ref struct Systemtime
    {
        [FieldOffset(6)]  public ushort wDay;
        [FieldOffset(4)]  public ushort wDayOfWeek;
        [FieldOffset(8)]  public ushort wHour;
        [FieldOffset(14)] public ushort wMilliseconds;
        [FieldOffset(10)] public ushort wMinute;
        [FieldOffset(2)]  public ushort wMonth;
        [FieldOffset(12)] public ushort wSecond;
        [FieldOffset(0)]  public ushort wYear;
    }

    private const    int    _MAX_DEGREE_OF_PARALLELISM = 10;
    private const    int    _NTP_PORT                  = 123;
    private readonly char[] _loading                   = { '-', '\\', '|', '/' };
    private          int    _procedCnt;
    private readonly int    _serverCnt;

    private readonly Dictionary<string, Server> _serverDictionary;


    private readonly string[] _serverDomains = {
                                                   "ntp.ntsc.ac.cn", "cn.ntp.org.cn", "edu.ntp.org.cn"
                                                 , "cn.pool.ntp.org", "time.pool.aliyun.com", "time1.aliyun.com"
                                                 , "time2.aliyun.com", "time3.aliyun.com", "time4.aliyun.com"
                                                 , "time5.aliyun.com", "time6.aliyun.com", "time7.aliyun.com"
                                                 , "time1.cloud.tencent.com", "time2.cloud.tencent.com"
                                                 , "time3.cloud.tencent.com", "time4.cloud.tencent.com"
                                                 , "time5.cloud.tencent.com", "ntp.sjtu.edu.cn", "ntp.neu.edu.cn"
                                                 , "ntp.bupt.edu.cn", "ntp.shu.edu.cn", "pool.ntp.org", "0.pool.ntp.org"
                                                 , "1.pool.ntp.org", "2.pool.ntp.org", "3.pool.ntp.org"
                                                 , "asia.pool.ntp.org", "time1.google.com", "time2.google.com"
                                                 , "time3.google.com", "time4.google.com", "time.apple.com"
                                                 , "time1.apple.com", "time2.apple.com", "time3.apple.com"
                                                 , "time4.apple.com", "time5.apple.com", "time6.apple.com"
                                                 , "time7.apple.com", "time.windows.com", "time.nist.gov"
                                                 , "time-nw.nist.gov", "time-a.nist.gov", "time-b.nist.gov"
                                                 , "stdtime.gov.hk"
                                               };

    private int _successCnt;


    public Main(Option opt) : base(opt)
    {
        _serverCnt        = _serverDomains.Length;
        _serverDictionary = _serverDomains.ToDictionary(x => x, _ => new Server { Status = ServerStatues.Ready });
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

    private async void Printing()
    {
        const string outputTemp = "{0,-30}\t{1}\t{2,20}\t{3,20}";
        var          rolling    = 0;

        Console.Clear();
        while (true) {
            await Task.Delay(100);
            Console.SetCursorPosition(0, 0);
            var row =                      //
                _serverDictionary.Select(x //
                                             => string.Format(outputTemp, x.Key
                                                            , x.Value.Status == ServerStatues.Connecting
                                                                  ? _loading[++rolling % 4]
                                                                  : ' ', x.Value.Status
                                                            , x.Value.Offset == TimeSpan.Zero
                                                                  ? string.Empty
                                                                  : x.Value.Offset));


            Console.WriteLine(outputTemp, Str.Server, ' ', Str.Status, Str.LocalClockOffset);
            Console.WriteLine(string.Join(Environment.NewLine, row));
            if (_procedCnt == _serverCnt) break;
        }
    }


    [DllImport("Kernel32.dll")]
    private static extern void SetLocalTime(Systemtime st);

    private static void SetSysteTime(DateTime time)
    {
        var timeToSet = new Systemtime {
                                           wDay          = (ushort)time.Day
                                         , wDayOfWeek    = (ushort)time.DayOfWeek
                                         , wHour         = (ushort)time.Hour
                                         , wMilliseconds = (ushort)time.Millisecond
                                         , wMinute       = (ushort)time.Minute
                                         , wMonth        = (ushort)time.Month
                                         , wSecond       = (ushort)time.Second
                                         , wYear         = (ushort)time.Year
                                       };
        SetLocalTime(timeToSet);
    }


    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public override async Task Run()
    {
        var tPrinting = Task.Run(Printing);

        await Parallel.ForEachAsync(_serverDictionary
                                  , new ParallelOptions { MaxDegreeOfParallelism = _MAX_DEGREE_OF_PARALLELISM }
                                  , (server, _) => {
                                        server.Value.Status = ServerStatues.Connecting;
                                        var offset = GetNtpOffset(server.Key);

                                        Interlocked.Increment(ref _procedCnt);

                                        if (offset == TimeSpan.Zero) {
                                            server.Value.Status = ServerStatues.Failed;
                                        }
                                        else {
                                            server.Value.Status = ServerStatues.Succeed;
                                            Interlocked.Increment(ref _successCnt);
                                            server.Value.Offset = offset;
                                        }

                                        return ValueTask.CompletedTask;
                                    });

        tPrinting.Wait();
        var avgOffset = TimeSpan.FromTicks((long)_serverDictionary //
                                                 .Where(x => x.Value.Status == ServerStatues.Succeed)
                                                 .Average(x => x.Value.Offset.Ticks));


        Console.WriteLine(Str.NtpReceiveDone, _successCnt, _serverCnt, avgOffset.TotalMilliseconds);
        if (!Opt.Sync) return;
        Console.WriteLine();
        SetSysteTime(DateTime.Now - avgOffset);
        Console.WriteLine(Str.LocalTimeSyncDone);
    }

    private record Server
    {
        public TimeSpan      Offset;
        public ServerStatues Status;
    }
}