using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

return ComposedScalarChecks.Run(args);

internal static unsafe class ComposedScalarChecks
{
    private const int ValueCount = 24, EventCapacity = 64;
    private const string InputHash = "24fed8a43ee41261eaebeff5b002b0a01358d95bc2c4c010a284f6770c79ad5b";
    private const string FactsHash = "fc1365753f9eedf699ffad3e322789ed5674891859d09b3f849c54f17d45f3a2";
    private const string NativeHash = "6c0cf00896ec82ea20a6c25d9cc0144e22cd5322b556dda0d7e8162146ebe65d";

    internal static int Run(string[] arguments)
    {
        if (arguments.Length != 2) throw new ArgumentException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
        Require(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64 &&
            BitConverter.IsLittleEndian && sizeof(nint) == 8, "The captured layout-byte protocol requires Linux x64 little endian.");
        var native = JsonNode.Parse(File.ReadAllText(arguments[0]))?.AsObject() ?? throw new InvalidDataException("Missing native observations.");
        var provenance = JsonNode.Parse(File.ReadAllText(arguments[1]))?.AsObject() ?? throw new InvalidDataException("Missing capture provenance.");
        Require(Hash(arguments[0]) == NativeHash && Text(provenance, "nativeObservations", "sha256") == NativeHash &&
            Text(provenance, "input", "sha256") == InputHash && Text(provenance, "facts", "sha256") == FactsHash &&
            Required(provenance, "sourceUnchanged").GetValue<bool>(), "The supplied immutable native/capture/facts identities differ.");
        var sources = Required(provenance, "sourcePins").AsObject();
        Require(sources.Count == 3 && HasSource(sources, "/Fixtures/T5/composed-scalar-call-chain.hpp", "aa438175ee3b951c24b9a536a789b5fb11d1103d1ea4f3d8bc1c63f63f5d51cc") &&
            HasSource(sources, "/Fixtures/T5/composed-scalar-call-chain.cpp", "4b74ddbe82109b8fae950e4e2745c107713a78cfad5a325e1cda41f6f48c09b7") &&
            HasSource(sources, "/ComposedScalarCallChain/native.cpp", "a77a67d07b06dbacc519c0f8197d906ad0d40f4141e2e1849a52c3dfad9cb01c"),
            "The source/header/native-oracle pins differ.");
        ValidateNativePhysical(Required(native, "nativePhysical").AsObject());
        var expectedScenarios = Required(native, "scenarios").AsArray();
        Require(expectedScenarios.Count == 2, "The finite oracle requires two scenarios.");

        using var firstOwner = new RawLayoutBytes(0x6d);
        using var secondOwner = new RawLayoutBytes(0xd6);
        using var firstInput = new RawLayoutBytes(0xa5);
        using var secondInput = new RawLayoutBytes(0x5a);
        RawLayoutBytes[] buffers = [firstOwner, secondOwner, firstInput, secondInput];
        Require(buffers.Select(buffer => (nuint)buffer.Data).Distinct().Count() == 4 &&
            buffers.All(buffer => ((nuint)buffer.Data) % 8 == 0), "Four independent naturally aligned raw buffers are required.");
        InitializeInput(firstInput, false);
        InitializeInput(secondInput, true);
        InitializeOwner(firstOwner, false, secondInput.Data);
        InitializeOwner(secondOwner, true, firstInput.Data);

        var scenarios = new JsonArray();
        var reads = new JsonArray();
        var invocationCount = 0;
        for (var scenarioIndex = 0; scenarioIndex < 2; ++scenarioIndex)
        {
            var expected = expectedScenarios[scenarioIndex]?.AsObject() ?? throw new InvalidDataException("Missing native scenario.");
            var firstKind = scenarioIndex == 0 ? 5 : 128;
            var secondKind = scenarioIndex == 0 ? 201 : 255;
            Require(JsonNode.DeepEquals(expected["inputKinds"], Ints(firstKind, secondKind)), "The actual native input tuple differs.");
            firstInput.WriteByte(24, (byte)firstKind);
            secondInput.WriteByte(24, (byte)secondKind);
            var beforeRead = Snapshots(buffers);
            var firstRead = CapturedBindings.ReadKind(firstInput.Data);
            var secondRead = CapturedBindings.ReadKind(secondInput.Data);
            Require(firstRead == firstKind && secondRead == secondKind && AllUnchanged(buffers, beforeRead),
                "Actual generated unsigned-byte reads or input/guard immutability failed.");
            reads.Add((JsonNode)new JsonObject { ["inputKinds"] = Ints(firstKind, secondKind), ["actualReadKinds"] = Ints(firstRead, secondRead) });
            var expectedPhases = Required(expected, "phases").AsArray();
            Require(expectedPhases.Count == 4, "The oracle requires four pointer/branch configurations per scenario.");
            var phases = new JsonArray();
            for (var phaseIndex = 0; phaseIndex < 4; ++phaseIndex)
            {
                var expectedPhase = expectedPhases[phaseIndex]?.AsObject() ?? throw new InvalidDataException("Missing native phase.");
                int[] targets = phaseIndex switch { 0 => [0, 2], 1 => [1, 0], 2 => [2, 1], _ => [1, 2] };
                bool[] direct = phaseIndex < 2 ? [true, false] : [false, true];
                Require(JsonNode.DeepEquals(expectedPhase["pointerTargets"], Ints(targets)) &&
                    JsonNode.DeepEquals(expectedPhase["useDirect"], Bools(direct)), "The frozen pointer/branch tuple differs.");
                firstOwner.WritePointer(24, Select(targets[0], firstInput.Data, secondInput.Data));
                secondOwner.WritePointer(24, Select(targets[1], firstInput.Data, secondInput.Data));
                firstOwner.WriteByte(40, direct[0] ? (byte)1 : (byte)0);
                secondOwner.WriteByte(40, direct[1] ? (byte)1 : (byte)0);
                var before = Snapshots(buffers);
                var first = Observe(buffers, before);
                var second = Observe(buffers, before);
                invocationCount += 2;
                Require(JsonNode.DeepEquals(first, expectedPhase["first"]) && JsonNode.DeepEquals(second, expectedPhase["second"]) &&
                    JsonNode.DeepEquals(first, second), "Full generated/native two-run scalar observations differ.");
                var phase = new JsonObject { ["pointerTargets"] = Ints(targets), ["useDirect"] = Bools(direct), ["first"] = first, ["second"] = second };
                Require(JsonNode.DeepEquals(phase, expectedPhase), "The complete native-compatible phase protocol differs.");
                phases.Add((JsonNode)phase);
            }
            var scenario = new JsonObject { ["inputKinds"] = Ints(firstKind, secondKind), ["phases"] = phases };
            Require(JsonNode.DeepEquals(scenario, expected), "The complete scenario protocol differs.");
            scenarios.Add((JsonNode)scenario);
        }
        Require(invocationCount == 16, "The two scenarios/four phases/two invocations were not all exercised.");
        var beforePhysical = Snapshots(buffers);
        var physical = CapturedBindings.PhysicalHooks(firstOwner.Data, secondOwner.Data, firstInput.Data, secondInput.Data);
        Require(AllUnchanged(buffers, beforePhysical), "Physical hooks modified a whole borrowed buffer or its outer guards.");
        Console.WriteLine(new JsonObject { ["scenarios"] = scenarios }.ToJsonString());
        Console.WriteLine(new JsonObject { ["composedScalarPhysical"] = new JsonObject
        {
            ["ownerAndInputRepresentation"] = "generated-aligned-layout-bytes", ["nativeRepresentation"] = "live-native-cpp-owners-and-pointees",
            ["cppConstructionOrVirtualExecutionClaim"] = false, ["ownershipOrLifetimeClaim"] = false,
            ["bufferBytes"] = 56, ["bufferAlignment"] = 8, ["outerGuardBytesPerSide"] = 8,
            ["fourIndependentBufferAddresses"] = true, ["wholeBuffersAndGuardsUnchanged"] = true,
            ["opaqueVptrOrPaddingEqualityAcrossRepresentationsClaim"] = false, ["fixtureInvocations"] = invocationCount,
            ["actualFreshUnsignedReads"] = reads, ["actualGeneratedBindings"] = physical
        } }.ToJsonString());
        Console.WriteLine(new JsonObject { ["suppliedCaptureProvenance"] = new JsonObject
        {
            ["inputSha256"] = InputHash, ["publicFactsSha256"] = FactsHash, ["nativeObservationsSha256"] = NativeHash,
            ["captureProvenanceSha256"] = Hash(arguments[1]),
            ["compiledProjectAttribution"] = "The outer receipt binds the actual completed GeneratedProject and checker binary; supplied hashes alone do not identify compiled code."
        } }.ToJsonString());
        return 0;
    }

    private static JsonObject Observe(RawLayoutBytes[] buffers, byte[][] before)
    {
        var result = CapturedBindings.Fixture(buffers[0].Data, buffers[1].Data, buffers[2].Data, buffers[3].Data);
        var count = CapturedBindings.Count();
        var overflow = CapturedBindings.Overflow();
        var live = CapturedBindings.Live();
        Require(count is >= 0 and <= EventCapacity && overflow == 0 && live == 0,
            "The bounded scalar event/zero-live protocol failed; zero live is not lifetime evidence.");
        var unchanged = buffers.Select((buffer, index) => buffer.Unchanged(before[index])).ToArray();
        var borrowed = new[] { CapturedBindings.View(buffers[0].Data) == (byte*)buffers[0].Data + 24,
            CapturedBindings.View(buffers[1].Data) == (byte*)buffers[1].Data + 24 };
        Require(unchanged.All(value => value) && borrowed.All(value => value), "Borrowed field identity or whole-buffer/guard immutability failed.");
        var values = new JsonArray();
        for (var index = 0; index < ValueCount; ++index) values.Add((JsonNode)JsonValue.Create(CapturedBindings.Value(index))!);
        var events = new JsonArray();
        for (var index = 0; index < count; ++index) events.Add((JsonNode)JsonValue.Create(CapturedBindings.Event(index))!);
        return new JsonObject { ["result"] = result, ["values"] = values, ["events"] = events, ["overflow"] = overflow,
            ["live"] = live, ["sameObjectByteChecks"] = Bools(unchanged), ["borrowedFieldIdentity"] = Bools(borrowed) };
    }

    private static void ValidateNativePhysical(JsonObject value)
    {
        Require(Required(value, "representation").GetValue<string>() == "live-native-cpp-owners-and-pointees" &&
            Required(value, "allSameObjectAndRepeatChecksPassed").GetValue<bool>(), "The independent native representation/immutability evidence differs.");
        foreach (var (name, expected) in new (string, int)[] { ("ownerSize",56),("ownerAlignment",8),("ownerPrefixSize",24),
            ("ownerPrefixAlignment",8),("prefixOffset",0),("prefixFirstOffset",8),("prefixSecondOffset",16),("payloadOffset",24),("suffixOffset",48),
            ("proxySize",24),("proxyAlignment",8),("internalOffset",0),("useDirectOffset",16),("storageSize",16),("storageAlignment",8),
            ("derivedStorageSize",16),("derivedStorageAlignment",8),("pointerOffset",0),("refcountOffset",8),("cookieOffset",0),
            ("accessSize",1),("accessAlignment",1),("storageBaseOffset",0),("accessBaseOffset",0),("inputSize",56),("inputAlignment",8),
            ("concreteInputSize",56),("concreteInputAlignment",8),("inputPrefixBaseOffset",8),("inputFirstOffset",8),("inputSecondOffset",16),
            ("kindOffset",24),("codeSize",1),("codeAlignment",1) })
            Require(Int(value, name) == expected, "Native physical coordinate differs: " + name);
        Require(JsonNode.DeepEquals(value["tailOffsets"], Ints(32,40,48)) &&
            JsonNode.DeepEquals(value["addressesAligned"], Bools(true,true,true,true)), "Native array/alignment observations differ.");
    }
    private static void InitializeInput(RawLayoutBytes target, bool second)
    {
        target.WriteInt64(8, second ? -0x4123456789ABCDE0L : -0x6123456789ABCDE0L);
        target.WriteInt64(16, second ? 0x3123456789ABCDE0L : 0x5123456789ABCDE0L);
        target.WriteInt64(32, second ? 0x1112131415161718L : 0x0102030405060708L);
        target.WriteInt64(40, second ? -0x1213141516171819L : -0x0203040506070809L);
        target.WriteInt64(48, second ? 0x131415161718191AL : 0x030405060708090AL);
    }
    private static void InitializeOwner(RawLayoutBytes target, bool second, void* cookie)
    {
        target.WriteInt64(8, second ? -0x3123456789ABCDE0L : -0x2123456789ABCDE0L);
        target.WriteInt64(16, second ? 0x2123456789ABCDE0L : 0x1123456789ABCDE0L);
        target.WriteInt64(48, second ? -0x3142536475061728L : 0x2132435465760718L);
        target.WritePointer(32, cookie);
    }
    private static void* Select(int selected, void* first, void* second) => selected switch
    { 0 => null, 1 => first, 2 => second, _ => throw new InvalidDataException("Unknown frozen pointer selector.") };
    private static byte[][] Snapshots(RawLayoutBytes[] values) => values.Select(value => value.Snapshot()).ToArray();
    private static bool AllUnchanged(RawLayoutBytes[] values, byte[][] before) => values.Select((value,index) => value.Unchanged(before[index])).All(value => value);
    private static bool HasSource(JsonObject sources, string suffix, string hash) => sources.Any(pair => pair.Key.EndsWith(suffix, StringComparison.Ordinal) && pair.Value?["sha256"]?.GetValue<string>() == hash);
    private static string? Text(JsonObject owner, string name, string field) => owner[name]?[field]?.GetValue<string>();
    private static JsonNode Required(JsonObject value, string name) => value[name] ?? throw new InvalidDataException("Missing " + name);
    private static int Int(JsonObject value, string name) => Required(value, name).GetValue<int>();
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static JsonArray Ints(params int[] values)
    { var result = new JsonArray(); foreach (var value in values) result.Add((JsonNode)JsonValue.Create(value)!); return result; }
    private static JsonArray Bools(params bool[] values)
    { var result = new JsonArray(); foreach (var value in values) result.Add((JsonNode)JsonValue.Create(value)!); return result; }
}

internal sealed unsafe class RawLayoutBytes : IDisposable
{
    private const int Guard = 8, Bytes = 56, Allocation = Bytes + 2 * Guard;
    private byte* allocation;
    internal void* Data => allocation + Guard;
    internal RawLayoutBytes(byte opaquePattern)
    {
        allocation = (byte*)NativeMemory.AlignedAlloc((nuint)Allocation, 8);
        if (allocation == null) throw new OutOfMemoryException();
        new Span<byte>(allocation, Allocation).Fill(opaquePattern);
    }
    internal void WriteByte(int offset, byte value) { Range(offset,1); allocation[Guard + offset] = value; }
    internal void WriteInt64(int offset, long value) { Range(offset,8); BinaryPrimitives.WriteInt64LittleEndian(new Span<byte>(allocation+Guard+offset,8),value); }
    internal void WritePointer(int offset, void* value) { Range(offset,8); BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(allocation+Guard+offset,8),(ulong)(nuint)value); }
    private static void Range(int offset, int count) { if (offset < 0 || offset > Bytes-count) throw new ArgumentOutOfRangeException(nameof(offset)); }
    internal byte[] Snapshot() => new ReadOnlySpan<byte>(allocation,Allocation).ToArray();
    internal bool Unchanged(byte[] before) => new ReadOnlySpan<byte>(allocation,Allocation).SequenceEqual(before);
    public void Dispose() { var owned = allocation; allocation = null; if (owned != null) NativeMemory.AlignedFree(owned); }
}

// The separate binding partial must use the actual completed publication.
// Until then this draft is intentionally incomplete, with no guessed names,
// native application/reader/reflection fallback or emulated method bodies.
internal static unsafe partial class CapturedBindings
{
    internal static partial int Fixture(void* firstOwner, void* secondOwner, void* firstInput, void* secondInput);
    internal static partial int Count();
    internal static partial int Overflow();
    internal static partial int Live();
    internal static partial int Event(int index);
    internal static partial int Value(int index);
    internal static partial int ReadKind(void* input);
    internal static partial void* View(void* owner);
    internal static partial JsonObject PhysicalHooks(void* firstOwner, void* secondOwner, void* firstInput, void* secondInput);
}
