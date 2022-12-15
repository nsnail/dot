// ReSharper disable ClassNeverInstantiated.Global

using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Dot.IP;

[Description(nameof(Str.Ip))]
[Localization(typeof(Str))]
internal sealed class Main : ToolBase<Option>
{
    protected override async Task Core()
    {
        foreach (var item in NetworkInterface.GetAllNetworkInterfaces()) {
            if (item.NetworkInterfaceType != NetworkInterfaceType.Ethernet ||
                item.OperationalStatus    != OperationalStatus.Up) {
                continue;
            }

            var output = string.Join( //
                Environment.NewLine
              , item.GetIPProperties()
                    .UnicastAddresses.Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => @$"{item.Name}: {x.Address}"));
            Console.WriteLine(output);
        }

        using var http = new HttpClient();
        Console.Write(Str.PublicIP);
        var str = await http.GetStringAsync("http://httpbin.org/ip");
        Console.WriteLine(str);
    }
}