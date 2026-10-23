using System.Runtime.InteropServices;
using System.Text.Json;
using Raw = Managed.Security.S2n.S2nTls;

// This probe executes only host argument parsing, without a TLS engine.
internal static unsafe class GetoptProbe
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Option { public byte* Name; public int HasArgument; public int* Flag; public int Value; }

    private static string Text(byte* pointer) => pointer == null ? "~" : Marshal.PtrToStringUTF8((nint)pointer)!;

    private static void Main(string[] arguments)
    {
        using var input = JsonDocument.Parse(File.ReadAllText(arguments[0]));
        int index = 0;
        foreach (JsonElement scenario in input.RootElement.EnumerateArray())
        {
            var allocations = new List<nint>();
            byte* String(string value) { nint p = Marshal.StringToCoTaskMemUTF8(value); allocations.Add(p); return (byte*)p; }
            int flag = 0;
            var items = scenario.GetProperty("options").EnumerateArray().ToArray();
            Option* options = (Option*)NativeMemory.AllocZeroed((nuint)(items.Length + 1), (nuint)sizeof(Option));
            var args = scenario.GetProperty("argv").EnumerateArray().Select(v => v.GetString()!).ToArray();
            byte** argv = (byte**)NativeMemory.AllocZeroed((nuint)(args.Length + 1), (nuint)sizeof(byte*));
            try
            {
                for (int i = 0; i < items.Length; i++)
                    options[i] = new() { Name = String(items[i].GetProperty("name").GetString()!),
                        HasArgument = items[i].GetProperty("argument").GetInt32(),
                        Flag = items[i].GetProperty("flag").GetBoolean() ? &flag : null,
                        Value = items[i].GetProperty("value").GetInt32() };
                for (int i = 0; i < args.Length; i++) argv[i] = String(args[i]);
                byte* shorts = String(scenario.GetProperty("short").GetString()!);
                Raw.ResetUpstreamGetopt(); *Raw.UpstreamOpterr() = 0;
                for (int guard = 0; ; guard++)
                {
                    if (guard > 200) throw new InvalidOperationException("GNU getopt did not terminate");
                    int longIndex = -9;
                    int value = Raw.UpstreamGetoptLong(args.Length, argv, shorts, options, &longIndex);
                    Console.WriteLine($"{index}:{value}:{*Raw.UpstreamOptind()}:{Text(*Raw.UpstreamOptarg())}:{longIndex}:{*Raw.UpstreamOptopt()}:{flag}");
                    if (value == -1) break;
                }
                Console.WriteLine(index + ":argv:" + string.Join(",", Enumerable.Range(0, args.Length).Select(i => Text(argv[i]))));
            }
            finally
            {
                foreach (nint allocation in allocations) Marshal.FreeCoTaskMem(allocation);
                NativeMemory.Free(options); NativeMemory.Free(argv);
            }
            index++;
        }
    }
}
