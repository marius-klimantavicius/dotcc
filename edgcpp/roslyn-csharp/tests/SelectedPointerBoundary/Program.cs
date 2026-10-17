using System.Runtime.InteropServices;
using Translated;
using Application = Translated.Program;

if (args.Length != 1) throw new ArgumentException("Expected pinned native oracle stdout path.");
var line = File.ReadAllText(args[0]).Trim();
if (!line.StartsWith("result=", StringComparison.Ordinal) || !int.TryParse(line[7..], out var expected))
    throw new InvalidOperationException("Native oracle result is missing.");
for (var iteration = 0; iteration < 2; ++iteration)
    if (Application.fixture() != expected) throw new InvalidOperationException("Cross-unit pointer observations differ from native.");
unsafe
{
    if (sizeof(BoundaryCell) != 8 || sizeof(BoundaryCell__2) != 8)
        throw new InvalidOperationException("Complete carrier size changed.");
    Prefix first = default;
    Prefix2 second = default;
    BoundaryCell* array = stackalloc BoundaryCell[2];
    BoundaryCell__2* array2 = stackalloc BoundaryCell__2[2];
    if ((byte*)&first.Value - (byte*)&first != 4 || (byte*)&second.Value - (byte*)&second != 4 ||
        (byte*)&array[0].value - (byte*)array != 0 || (byte*)&array[0].canary - (byte*)array != 4 ||
        (byte*)&array2[0].value - (byte*)array2 != 0 || (byte*)&array2[0].canary - (byte*)array2 != 4 ||
        (byte*)&array[1] - (byte*)array != 8 || (byte*)&array2[1] - (byte*)array2 != 8 ||
        (nuint)array % 4 != 0 || (nuint)array2 % 4 != 0)
        throw new InvalidOperationException("Captured carrier alignment, field offset or stride changed.");
}
Console.WriteLine($"result={expected}; runs=2; sizes=8/8; alignment=4; offsets=0/4; strides=8/8");

[StructLayout(LayoutKind.Sequential)]
struct Prefix { public byte PrefixByte; public BoundaryCell Value; }
[StructLayout(LayoutKind.Sequential)]
struct Prefix2 { public byte PrefixByte; public BoundaryCell__2 Value; }
