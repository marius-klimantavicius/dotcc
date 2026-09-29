using System.Text.Json;
using static UdpPeer;

internal static class Program
{
    private static void Main(string[] args)
    {
        Require(args.Length == 2, "usage: Applications.Udp <case> <matching-native-report.json>");
        string name = args[0];
        using var native = JsonDocument.Parse(File.ReadAllText(args[1]));
        Require(native.RootElement.GetProperty("case").GetString() == name, "matching native application case");
        Require(native.RootElement.GetProperty("status").GetString() == "passed", "native application assertions completed");
        using (var pair = new VirtualPair())
        {
            switch (name)
            {
                case "tftp-server": TftpCases.Run(pair); break;
                case "tftp-get": TftpCases.RunClientGet(pair); break;
                case "tftp-put": TftpCases.RunClientPut(pair); break;
                case "netbios": NetbiosCases.Run(pair); break;
                case "mdns": MdnsCases.Run(pair); break;
                case "sntp":
                    SntpCases.Run(pair);
                    break;
                case "owned-tftp": OwnedTftpCases.Run(pair); break;
                default: throw new ArgumentException("Unknown UDP application case: " + name);
            }
            pair.Check(); pair.Drain();
        }
        // Only emitted after all fixture and owner teardown assertions finish.
        using var report = new Utf8JsonWriter(Console.OpenStandardOutput());
        report.WriteStartObject();report.WriteString("case",name);report.WriteString("status","passed");
        report.WriteString("control","matching-native-original-source");report.WriteEndObject();report.Flush();Console.WriteLine();
    }
}
