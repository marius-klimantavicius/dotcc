using Managed.Networking;
internal static unsafe partial class Program
{
    private static int checks;
    internal static void Require(bool condition,string name){if(!condition)throw new InvalidOperationException(name);Interlocked.Increment(ref checks);}
    internal static byte Pattern(int index)=>(byte)(index*31+(index>>8));
    static void Main(string[] args)
    {
        using var pair=new VirtualPair();
#if !LWIP_NOSYS
        NetconnCase(pair,false);NetconnCase(pair,true);
#endif
        AltcpCase(pair,false);AltcpCase(pair,true);pair.Check();
        Console.WriteLine($"PASS direct-apis: {checks} assertions; explicit netconn/plain-altcp cases completed");
    }
}
