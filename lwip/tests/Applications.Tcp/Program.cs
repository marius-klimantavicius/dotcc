using System.Text.Json;
using static Fixture;

internal static class Program
{
    static void Main(string[] args)
    {
        Require(args.Length==2,"usage: Applications.Tcp <mqtt|smtp|lwiperf|http-client|proxy-connect> <matching-native-report.json>");
        using var native=JsonDocument.Parse(File.ReadAllText(args[1]));
        Require(native.RootElement.GetProperty("case").GetString()==(args[0]=="owned-mqtt"?"mqtt":args[0])&&native.RootElement.GetProperty("status").GetString()=="passed","matching successful native original-source control required");
        using(var fixture=new Fixture())
        {
            switch(args[0])
            {
                case "owned-mqtt":OwnedMqttCases.Run(fixture);break;
                case "mqtt":MqttCases.Run(fixture);break;
                case "smtp":SmtpCases.Run(fixture);break;
                case "lwiperf":LwiperfCases.Run(fixture);break;
                case "http-client":HttpClientCases.Run(fixture);break;
                case "proxy-connect":ProxyConnectCases.Run(fixture);break;
                default:throw new ArgumentException("Unknown TCP application draft");
            }
            fixture.Pair.Drain();fixture.Pair.Check();
        }
        using var writer=new Utf8JsonWriter(Console.OpenStandardOutput());
        writer.WriteStartObject();writer.WriteString("case",args[0]);writer.WriteString("status","passed");
        writer.WriteString("scope","finite original TCP application behavior; product service ownership remains required");
        writer.WriteEndObject();writer.Flush();Console.WriteLine();
    }
}
