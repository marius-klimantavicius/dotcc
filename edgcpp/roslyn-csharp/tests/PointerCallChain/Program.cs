using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

return PointerCallChainChecks.Run(args);

internal static unsafe class PointerCallChainChecks
{
    private const int ValueCount = 32, EventCapacity = 64;

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2) throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
        Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64 &&
            BitConverter.IsLittleEndian && sizeof(nint) == 8, "This finite layout-byte checker requires Linux x64 little endian.");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0]))?.AsObject() ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1]))?.AsObject() ?? throw new InvalidDataException("Missing capture provenance.");
        var inputHash = Required(provenance, "input")["sha256"]?.GetValue<string>();
        Require(inputHash == "735089e79666aeca94f4daf65f32305fc2a8964a1a730c74ca24da4cbc238620", "The supplied capture input differs from this fixture.");
        var sources = Required(provenance, "sources").AsObject();
        Require(sources.Count == 2 && sources.Any(pair => pair.Key.EndsWith("/pointer-call-chain.cpp", StringComparison.Ordinal) &&
            pair.Value?["sha256"]?.GetValue<string>() == "c2cb2277fa8b2273ec51ad0150874e3e3a7863361bffbccbbb2869b2555b6014") &&
            sources.Any(pair => pair.Key.EndsWith("/pointer-call-chain.hpp", StringComparison.Ordinal) &&
            pair.Value?["sha256"]?.GetValue<string>() == "184cba6160bbd33ae7c8767011262a197a50a5d96c44ac9e0a402b4d49d7ec57"),
            "The supplied capture source/header identities differ.");

        var nativePhysical = Required(native, "nativeInputPhysical").AsObject();
        Require(Required(nativePhysical, "representation").GetValue<string>() == "two-live-native-cpp-objects", "Native input representation label differs.");
        Require(Int(nativePhysical, "cellInputSize") == 56 && Int(nativePhysical, "cellInputAlignment") == 8 &&
            Int(nativePhysical, "prefixBaseOffset") == 8 && Int(nativePhysical, "firstOffset") == 8 &&
            Int(nativePhysical, "secondOffset") == 16 && Int(nativePhysical, "kindOffset") == 24,
            "Native selected size/alignment/field coordinates differ from the captured layout.");
        Require(JsonNode.DeepEquals(nativePhysical["tailOffsets"], Ints(32, 40, 48)), "Native tail coordinates differ.");
        Require(Required(nativePhysical, "allSameObjectByteChecksPassed").GetValue<bool>(), "The native inputs were modified.");
        var expectedScenarios = Required(native, "scenarios").AsArray();
        Require(expectedScenarios.Count == 2, "Expected exactly two frozen input scenarios.");

        using var first = new RawInput(0xa5, Required(nativePhysical, "deterministicFirstFields").AsObject());
        using var second = new RawInput(0x5a, Required(nativePhysical, "deterministicSecondFields").AsObject());
        Require(first.Data != second.Data && ((nuint)first.Data) % 8 == 0 && ((nuint)second.Data) % 8 == 0,
            "Independent aligned layout-byte input allocation failed.");
        var scenarios = new JsonArray();
        var readAudits = new JsonArray();
        for (var index = 0; index < 2; ++index)
        {
            var expected = expectedScenarios[index]?.AsObject() ?? throw new InvalidDataException("Missing native scenario.");
            var firstKind = index == 0 ? 5 : 37;
            var secondKind = index == 0 ? 201 : 19;
            Require(JsonNode.DeepEquals(expected["inputKinds"], Ints(firstKind, secondKind)), "Native scenario input kinds differ.");
            first.SetKind((byte)firstKind);
            second.SetKind((byte)secondKind);
            var firstBefore = first.Snapshot();
            var secondBefore = second.Snapshot();
            var readFirst = CapturedBindings.ReadKind(first.Data);
            var readSecond = CapturedBindings.ReadKind(second.Data);
            Require(readFirst == firstKind && readSecond == secondKind, "Actual generated reader did not preserve the unsigned field value.");
            Require(first.Unchanged(firstBefore) && second.Unchanged(secondBefore), "The generated reader modified layout-byte inputs or guards.");
            var run1 = Observe(first, second, firstBefore, secondBefore);
            var run2 = Observe(first, second, firstBefore, secondBefore);
            Require(JsonNode.DeepEquals(run1, expected["first"]) && JsonNode.DeepEquals(run2, expected["second"]),
                "Generated full observations differ from the independent native oracle.");
            Require(JsonNode.DeepEquals(run1, run2), "Repeated generated observations differ.");
            var scenario = new JsonObject
            {
                ["inputKinds"] = Ints(firstKind, secondKind), ["first"] = run1, ["second"] = run2,
                ["inputByteChecks"] = new JsonObject { ["firstRun"] = Bools(true, true), ["secondRun"] = Bools(true, true) }
            };
            Require(JsonNode.DeepEquals(scenario, expected), "Native-compatible scenario protocol differs.");
            scenarios.Add((JsonNode)scenario);
            readAudits.Add((JsonNode)new JsonObject { ["inputKinds"] = Ints(firstKind, secondKind), ["actualGeneratedReadKinds"] = Ints(readFirst, readSecond) });
        }

        var finalFirst = first.Snapshot();
        var finalSecond = second.Snapshot();
        // The partial binds actual published generated entries, never emulated
        // application bodies. Raw input bytes retain their separate test scope.
        var bindingPhysical = CapturedBindings.PhysicalHooks(first.Data, second.Data);
        Require(first.Unchanged(finalFirst) && second.Unchanged(finalSecond), "Physical hooks modified raw inputs or outer guards.");
        Console.WriteLine(new JsonObject { ["scenarios"] = scenarios }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["pointerCallChainPhysical"] = new JsonObject
            {
                ["inputRepresentation"] = "generated-aligned-layout-bytes",
                ["nativeInputRepresentation"] = "two-live-native-cpp-objects",
                ["cppConstructionOrVirtualExecutionClaim"] = false, ["ownershipOrLifetimeClaim"] = false,
                ["inputBytes"] = 56, ["inputAlignment"] = 8, ["outerGuardBytesPerSide"] = 8,
                ["prefixOffsets"] = Ints(8, 16), ["kindOffset"] = 24, ["tailOffsets"] = Ints(32, 40, 48),
                ["independentInputAddresses"] = true, ["alignedInputAddresses"] = Bools(true, true),
                ["wholeInputsAndOuterGuardsUnchanged"] = true, ["opaqueVptrAndPaddingComparedAcrossRepresentations"] = false,
                ["directReaderScenarios"] = readAudits, ["actualGeneratedBindings"] = bindingPhysical
            }
        }.ToJsonString());
        Console.WriteLine(new JsonObject
        {
            ["suppliedCaptureProvenance"] = new JsonObject
            {
                ["inputSha256"] = inputHash, ["captureProvenanceSha256"] = Hash(arguments[1]),
                ["nativeObservationsSha256"] = Hash(arguments[0]),
                ["compiledGeneratedProjectIdentityScope"] = "Outer receipt only; supplied hashes do not identify the compiled project."
            }
        }.ToJsonString());
        return 0;
    }

    private static JsonObject Observe(RawInput first, RawInput second, byte[] firstBefore, byte[] secondBefore)
    {
        var result = CapturedBindings.Fixture(first.Data, second.Data);
        var count = CapturedBindings.Count();
        var overflow = CapturedBindings.Overflow();
        var protocolLive = CapturedBindings.Live();
        Require(count is >= 0 and <= EventCapacity && overflow == 0 && protocolLive == 0,
            "Bounded event/overflow/zero-live harness protocol failed; no ownership is inferred from zero live.");
        Require(first.Unchanged(firstBefore) && second.Unchanged(secondBefore), "Generated fixture modified a whole input or outer guard.");
        var values = new JsonArray();
        for (var index = 0; index < ValueCount; ++index) values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
        var events = new JsonArray();
        for (var index = 0; index < count; ++index) events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
        Require(count == 25, "The observed frozen native fixture has exactly 25 events.");
        return new JsonObject { ["result"] = result, ["values"] = values, ["events"] = events, ["overflow"] = overflow, ["live"] = protocolLive };
    }

    private static JsonArray Ints(params int[] values)
    {
        var result = new JsonArray();
        foreach (var value in values) result.Add((JsonNode)JsonValue.Create(value)!);
        return result;
    }
    private static JsonArray Bools(params bool[] values)
    {
        var result = new JsonArray();
        foreach (var value in values) result.Add((JsonNode)JsonValue.Create(value)!);
        return result;
    }
    private static JsonNode Required(JsonObject owner, string name) => owner[name] ?? throw new InvalidDataException("Missing " + name);
    private static int Int(JsonObject owner, string name) => Required(owner, name).GetValue<int>();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static string Hash(string path) { using var file = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(file)); }
}

internal sealed unsafe class RawInput : IDisposable
{
    private const int Guard = 8, InputSize = 56, AllocationSize = InputSize + 2 * Guard;
    private byte* allocation;
    internal void* Data => allocation + Guard;

    internal RawInput(byte opaquePattern, JsonObject nativeFields)
    {
        allocation = (byte*)NativeMemory.AlignedAlloc((nuint)AllocationSize, 8);
        if (allocation == null) throw new OutOfMemoryException();
        try
        {
            new Span<byte>(allocation, AllocationSize).Fill(opaquePattern);
            var prefix = nativeFields["prefixHex"]?.AsArray() ?? throw new InvalidDataException("Missing deterministic native prefix.");
            var tail = nativeFields["tailHex"]?.AsArray() ?? throw new InvalidDataException("Missing deterministic native tail.");
            if (prefix.Count != 2 || tail.Count != 3) throw new InvalidDataException("Native deterministic field counts differ.");
            for (var index = 0; index < 2; ++index) WriteHex(8 + index * 8, prefix[index]);
            for (var index = 0; index < 3; ++index) WriteHex(32 + index * 8, tail[index]);
        }
        catch { Dispose(); throw; }
    }
    private void WriteHex(int offset, JsonNode? value)
    {
        var text = value?.GetValue<string>() ?? throw new InvalidDataException("Missing native 64-bit field bits.");
        if (text.Length != 16 || !ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var bits))
            throw new InvalidDataException("Invalid native 64-bit field bits.");
        BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(allocation + Guard + offset, 8), bits);
    }
    internal void SetKind(byte value) => allocation[Guard + 24] = value;
    internal byte[] Snapshot() => new ReadOnlySpan<byte>(allocation, AllocationSize).ToArray();
    internal bool Unchanged(byte[] snapshot) => new ReadOnlySpan<byte>(allocation, AllocationSize).SequenceEqual(snapshot);
    public void Dispose()
    {
        var owned = allocation;
        allocation = null;
        if (owned != null) NativeMemory.AlignedFree(owned);
    }
}

// Implemented by the test-only partial using exact published methods/types;
// no reflection, native application library, reader or application fallback.
internal static unsafe partial class CapturedBindings
{
    internal static partial int Fixture(void* first, void* second);
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Event(int index);
    internal static partial int Value(int index);
    internal static partial int ReadKind(void* input);
    internal static partial JsonObject PhysicalHooks(void* first, void* second);
}
