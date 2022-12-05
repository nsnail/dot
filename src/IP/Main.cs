using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Dot.IP;

public sealed class Main : Tool<Option>

{
    public Main(Option opt) : base(opt) { }

    public override async Task Run()
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