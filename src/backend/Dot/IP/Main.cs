// ReSharper disable ClassNeverInstantiated.Global

using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Dot.IP;

[Description(nameof(Ln.IP工具))]
[Localization(typeof(Ln))]
internal sealed class Main : ToolBase<Option>
{
    private const string _HTTP_BIN_ORG_IP = "http://httpbin.org/ip";

    protected override async Task CoreAsync()
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
                    .Select(x => $"{item.Name}: {x.Address}"));
            Console.WriteLine(output);
        }

        using var http = new HttpClient();
        Console.Write($"{Ln.公网IP}: ");
        var str = await http.GetStringAsync(_HTTP_BIN_ORG_IP).ConfigureAwait(false);
        Console.WriteLine(str);
    }
}