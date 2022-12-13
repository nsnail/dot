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
                item.OperationalStatus    != OperationalStatus.Up)
                continue;
            foreach (var ip in item.GetIPProperties().UnicastAddresses)
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    Console.WriteLine(@$"{item.Name}: {ip.Address}");
        }

        using var http = new HttpClient();
        Console.Write(Str.PublicIP);
        var str = await http.GetStringAsync("http://httpbin.org/ip");
        Console.WriteLine(str);
    }
}