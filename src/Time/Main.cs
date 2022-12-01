using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Dot.Time;

public sealed class Main : Tool<Option>
{
    [StructLayout(LayoutKind.Explicit)]
    public ref struct _SYSTEMTIME
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

    private readonly Dictionary<string, string> _serverList = new() {
                                                                        { "ntp.ntsc.ac.cn", "国家授时中心 NTP 服务器" }
                                                                      , { "cn.ntp.org.cn", "中国 NTP 快速授时服务" }
                                                                      , { "edu.ntp.org.cn", "中国 NTP 快速授时服务" }
                                                                      , { "cn.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "time.pool.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time1.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time2.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time3.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time4.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time5.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time6.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time7.aliyun.com", "阿里云公共 NTP 服务器" }
                                                                      , { "time1.cloud.tencent.com", "腾讯云公共 NTP 服务器" }
                                                                      , { "time2.cloud.tencent.com", "腾讯云公共 NTP 服务器" }
                                                                      , { "time3.cloud.tencent.com", "腾讯云公共 NTP 服务器" }
                                                                      , { "time4.cloud.tencent.com", "腾讯云公共 NTP 服务器" }
                                                                      , { "time5.cloud.tencent.com", "腾讯云公共 NTP 服务器" }
                                                                      , { "ntp.sjtu.edu.cn", "教育网（高校自建）" }
                                                                      , { "ntp.neu.edu.cn", "教育网（高校自建）" }
                                                                      , { "ntp.bupt.edu.cn", "教育网（高校自建）" }
                                                                      , { "ntp.shu.edu.cn", "教育网（高校自建）" }
                                                                      , { "pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "0.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "1.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "2.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "3.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "asia.pool.ntp.org", "国际 NTP 快速授时服务" }
                                                                      , { "time1.google.com", "谷歌公共 NTP 服务器" }
                                                                      , { "time2.google.com", "谷歌公共 NTP 服务器" }
                                                                      , { "time3.google.com", "谷歌公共 NTP 服务器" }
                                                                      , { "time4.google.com", "谷歌公共 NTP 服务器" }
                                                                      , { "time.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time1.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time2.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time3.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time4.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time5.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time6.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time7.apple.com", "苹果公司公共 NTP 服务器" }
                                                                      , { "time.windows.com", "微软 Windows NTP 服务器" }
                                                                      , { "time.nist.gov", "美国标准技术研究院 NTP 服务器" }
                                                                      , { "time-nw.nist.gov", "美国标准技术研究院 NTP 服务器" }
                                                                      , { "time-a.nist.gov", "美国标准技术研究院 NTP 服务器" }
                                                                      , { "time-b.nist.gov", "美国标准技术研究院 NTP 服务器" }
                                                                      , { "stdtime.gov.hk", "香港天文台公共 NTP 服务器" }
                                                                    };

    public Main(Option opt) : base(opt) { }


    private TimeSpan GetNtpOffset()
    {
        Span<byte> ntpData = stackalloc byte[48];
        TimeSpan   ts;
        ntpData[0] = 0x1B;
        foreach (var server in _serverList) {
            Console.Write(Strings.Main_GetUtc_, server.Key, server.Value);

            using var socket
                = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = 3000 };

            try {
                socket.Connect(_serverList.First().Key, 123);
                Console.WriteLine(Strings.OK);
                Console.Write(Strings.Main_GetUtc_sdf);
                socket.Send(ntpData);
                Console.WriteLine(Strings.OK);
                Console.Write(Strings.Main_GetUtc_接收请求___);
                var timeStart = DateTime.Now;
                socket.Receive(ntpData);
                ts = DateTime.Now - timeStart;
                Console.WriteLine(Strings.Main_GetUtc__0_us, ts.TotalMilliseconds);
            }
            catch (Exception) {
                Console.WriteLine(Strings.Timeout);
                continue;
            }
            finally {
                socket.Close();
            }


            var intPart = ((ulong)ntpData[40]   << 24) //
                          | ((ulong)ntpData[41] << 16) //
                          | ((ulong)ntpData[42] << 8)  //
                          | ntpData[43];

            var fractPart = ((ulong)ntpData[44]   << 24) //
                            | ((ulong)ntpData[45] << 16) //
                            | ((ulong)ntpData[46] << 8)  //
                            | ntpData[47];

            var from1900Ms = intPart * 1000 + fractPart * 1000 / 0x100000000L;
            var onlineTime = new DateTime(1900, 1, 1).AddMilliseconds((long)from1900Ms) + ts;
            return DateTime.UtcNow - onlineTime;
        }

        throw new TimeoutException();
    }


    [DllImport("Kernel32.dll")]
    public static extern unsafe void GetLocalTime(_SYSTEMTIME* st);

    public override void Run()
    {
        while (true) {
            TimeSpan offset;
            try {
                offset = GetNtpOffset();
            }
            catch (TimeoutException) {
                Console.Error.WriteLine(Strings.NoService);
                return;
            }


            using var tokenSource = new CancellationTokenSource();
            var       token       = tokenSource.Token;
            Task.Run(async () => {
                for (;;) {
                    if (token.IsCancellationRequested) return;
                    Console.WriteLine(Strings.Main_Run_NTP服务器时间___0_, (DateTime.Now + offset).ToString("O"));
                    Console.WriteLine(Strings.Main_Run_本地时间___0_,     DateTime.Now.ToString("O"));
                    Console.WriteLine(offset > TimeSpan.Zero
                                          ? string.Format(Strings.Main_Run_时差___0__ms, offset.TotalMilliseconds)
                                          : string.Format(Strings.Main_Run_时差___1__ms, -offset.TotalMilliseconds));
                    Console.WriteLine(Strings.Main_Run_SyncClock);
                    await Task.Delay(1000, token);
                    Console.Clear();
                }
            }, token);

            if (Console.ReadKey().Key == ConsoleKey.Y) {
                var ntpTime = DateTime.Now - offset;
                var f = new _SYSTEMTIME {
                                            wDay          = (ushort)ntpTime.Day
                                          , wDayOfWeek    = (ushort)ntpTime.DayOfWeek
                                          , wHour         = (ushort)ntpTime.Hour
                                          , wMilliseconds = (ushort)ntpTime.Millisecond
                                          , wMinute       = (ushort)ntpTime.Minute
                                          , wMonth        = (ushort)ntpTime.Month
                                          , wSecond       = (ushort)ntpTime.Second
                                          , wYear         = (ushort)ntpTime.Year
                                        };

                SetLocalTime(f);
                Console.WriteLine(Strings.Main_Run_SyncDone);
                tokenSource.Cancel();
                continue;
            }

            break;
        }
    }

    [DllImportAttribute("Kernel32.dll")]
    public static extern void SetLocalTime(_SYSTEMTIME st);
}