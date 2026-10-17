using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

return SelectedCoreChecks.Run(args);

internal static unsafe class SelectedCoreChecks
{
    private const string InputSha256 = "cf385364234784aa0d748781d06f74161ad51d28a6a34215449c3ae212810b65";
    private const string LinkageName = "_ZNK6duckdb12VectorBuffer13GetVectorTypeEv";

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 0) throw new ArgumentException("SelectedCore checks take no arguments; the outer receipt binds the selected input and generated project.");
        Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64 &&
            BitConverter.IsLittleEndian && sizeof(nint) == 8, "This finite checker requires Linux x64 little endian.");

        using var first = new GuardedLayoutBytes(0xa5);
        using var second = new GuardedLayoutBytes(0x5a);
        Require(first.Data != second.Data && Aligned8(first.Data) && Aligned8(second.Data),
            "The independent borrowed layout buffers must be naturally aligned.");
        var scenarios = new JsonArray();
        ReadOnlySpan<byte> firstKinds = [0, 127, 128, 201, 255];
        ReadOnlySpan<byte> secondKinds = [255, 201, 128, 127, 0];
        for (var scenario = 0; scenario < firstKinds.Length; ++scenario)
        {
            var firstValue = firstKinds[scenario];
            var secondValue = secondKinds[scenario];
            first.SetKind(firstValue);
            second.SetKind(secondValue);
            var firstBefore = first.Snapshot();
            var secondBefore = second.Snapshot();
            var firstReads = new JsonArray();
            var secondReads = new JsonArray();
            for (var repeat = 0; repeat < 2; ++repeat)
            {
                var firstRead = CapturedBindings.ReadKind(first.Data);
                Require(firstRead == firstValue, "The selected getter did not freshly return the first unsigned byte.");
                Require(first.Unchanged(firstBefore) && second.Unchanged(secondBefore), "The first getter call modified an input or outer guard.");
                var secondRead = CapturedBindings.ReadKind(second.Data);
                Require(secondRead == secondValue, "The selected getter did not freshly return the second unsigned byte.");
                Require(first.Unchanged(firstBefore) && second.Unchanged(secondBefore), "The second getter call modified an input or outer guard.");
                firstReads.Add((JsonNode)JsonValue.Create(firstRead)!);
                secondReads.Add((JsonNode)JsonValue.Create(secondRead)!);
            }
            scenarios.Add((JsonNode)new JsonObject
            {
                ["inputKinds"] = Integers(firstValue, secondValue),
                ["firstReads"] = firstReads,
                ["secondReads"] = secondReads,
                ["wholeInputsAndOuterGuardsUnchanged"] = true
            });
        }

        var firstFinal = first.Snapshot();
        var secondFinal = second.Snapshot();
        var physical = CapturedBindings.InspectPhysical(first.Data, second.Data);
        Require(Int(physical, "viewSize") == 56 && Int(physical, "kindOffset") == 24 &&
            Int(physical, "sequentialPrefixOffset") == 8 && Int(physical, "elementStride") == 56,
            "Actual generated borrowed-view size, field offset, alignment or stride differs.");
        Require(physical["actualInputAddressesAligned"]?.GetValue<bool>() == true,
            "The physical binding did not verify actual input alignment.");
        Require(first.Unchanged(firstFinal) && second.Unchanged(secondFinal), "Physical inspection modified an input or outer guard.");

        Console.WriteLine(new JsonObject
        {
            ["selectedCore"] = new JsonObject
            {
                ["inputSha256"] = InputSha256,
                ["selectedLinkageName"] = LinkageName,
                ["inputRepresentation"] = "generated-aligned-layout-bytes",
                ["inputBytes"] = GuardedLayoutBytes.InputSize,
                ["inputAlignment"] = 8,
                ["outerGuardBytesPerSide"] = GuardedLayoutBytes.GuardSize,
                ["independentInputAddresses"] = true,
                ["scenarios"] = scenarios,
                ["wholeInputsAndOuterGuardsUnchanged"] = true,
                ["nativeDuckdbOracleComparison"] = false,
                ["classConstructionOrVirtualExecutionClaim"] = false,
                ["ownershipOrLifetimeClaim"] = false,
                ["wholeCoreTranslationClaim"] = false,
                ["compiledProjectAttribution"] = "The outer receipt binds the input, request, completed generated project and checker binary."
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject { ["selectedCorePhysical"] = physical }.ToJsonString());
        return 0;
    }

    private static JsonArray Integers(params int[] values)
    {
        var result = new JsonArray();
        foreach (var value in values) result.Add((JsonNode)JsonValue.Create(value)!);
        return result;
    }
    private static int Int(JsonObject value, string name) => value[name]?.GetValue<int>() ?? throw new InvalidDataException("Missing physical fact " + name);
    private static bool Aligned8(void* address) => ((nuint)address) % 8 == 0;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

internal sealed unsafe class GuardedLayoutBytes : IDisposable
{
    internal const int GuardSize = 8, InputSize = 56;
    private const int AllocationSize = InputSize + GuardSize * 2;
    private byte* allocation;
    internal void* Data => allocation + GuardSize;

    internal GuardedLayoutBytes(byte opaquePattern)
    {
        allocation = (byte*)NativeMemory.AlignedAlloc((nuint)AllocationSize, 8);
        if (allocation == null) throw new OutOfMemoryException();
        new Span<byte>(allocation, AllocationSize).Fill(opaquePattern);
    }
    internal void SetKind(byte value) => allocation[GuardSize + 24] = value;
    internal byte[] Snapshot() => new ReadOnlySpan<byte>(allocation, AllocationSize).ToArray();
    internal bool Unchanged(byte[] before) => new ReadOnlySpan<byte>(allocation, AllocationSize).SequenceEqual(before);
    public void Dispose()
    {
        var owned = allocation;
        allocation = null;
        if (owned != null) NativeMemory.AlignedFree(owned);
    }
}

// The required implementation is added only after publication supplies actual
// generated names. It invokes the selected body and measures its typed view;
// there is no reflection, reader, native application or emulated getter fallback.
internal static unsafe partial class CapturedBindings
{
    internal static partial int ReadKind(void* input);
    internal static partial JsonObject InspectPhysical(void* first, void* second);
}
