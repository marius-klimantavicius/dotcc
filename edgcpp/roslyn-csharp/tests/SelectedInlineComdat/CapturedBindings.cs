using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Api = global::Translated.Program;
using SourceTracked = global::Translated.InlineTracked;
using ProviderTracked = global::Translated.InlineTracked__2;

internal static unsafe class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SourceAlignmentProbe
    {
        public byte Prefix;
        public SourceTracked Item;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProviderAlignmentProbe
    {
        public byte Prefix;
        public ProviderTracked Item;
    }

    internal static void Initialize() => Api.InitializeSelectedProgram();
    internal static void Shutdown() => Api.ShutdownSelectedProgram();
    internal static int Fixture() => Api.fixture();
    internal static int Count() => Api.lifetime_count();
    internal static int Overflow() => Api.lifetime_overflow();
    internal static int Live() => Api.lifetime_live();
    internal static int Calls() => Api.startup_calls();
    internal static int Value(int index) => Api.lifetime_value(index);
    internal static int Event(int index) => Api.lifetime_event(index);
    internal static ManagedSelectedProgramLifetimeDiagnostics Diagnostics() => Api.SelectedProgramDiagnostics();
    // These source helpers are not advertised in this selected publication;
    // inspect the actual independent application storage, not invented calls.
    internal static int CreatedStorage() => *Api.selected_inline_created;
    internal static int DestroyedStorage() => *Api.selected_inline_destroyed;

    internal static ApplicationSnapshot SnapshotStorage()
    {
        var count = *Api.event_count;
        Check(count is >= 0 and <= 64, "The final event storage exceeded its bound.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = ((int*)Api.values)[index];
        for (var index = 0; index < events.Length; ++index) events[index] = ((int*)Api.events)[index];
        return new(*Api.fixture_calls, *Api.selected_inline_created, *Api.selected_inline_destroyed,
            *Api.selected_inline_live, *Api.event_overflow, values, events);
    }

    internal static ObjectSnapshot SnapshotObjects() => new((nint)Api.selected_inline_first, (nint)Api.selected_inline_second,
        SourceBytes().ToArray(), ProviderBytes().ToArray());

    internal static bool ObjectsUnchanged(ObjectSnapshot snapshot) =>
        snapshot.FirstAddress == (nint)Api.selected_inline_first && snapshot.SecondAddress == (nint)Api.selected_inline_second &&
        SourceBytes().SequenceEqual(snapshot.FirstBytes) && ProviderBytes().SequenceEqual(snapshot.SecondBytes);

    internal static PhysicalObservation Physical(ObjectSnapshot snapshot)
    {
        SourceTracked source = default;
        ProviderTracked provider = default;
        SourceAlignmentProbe sourcePrefix = default;
        ProviderAlignmentProbe providerPrefix = default;
        SourceTracked* sourceElements = stackalloc SourceTracked[2];
        ProviderTracked* providerElements = stackalloc ProviderTracked[2];
        var idOffset = (long)((byte*)&source.id - (byte*)&source);
        var providerIdOffset = (long)((byte*)&provider.id - (byte*)&provider);
        var sourcePrefixOffset = (long)((byte*)&sourcePrefix.Item - (byte*)&sourcePrefix);
        var providerPrefixOffset = (long)((byte*)&providerPrefix.Item - (byte*)&providerPrefix);
        var sourceStride = (long)((byte*)(&sourceElements[1]) - (byte*)(&sourceElements[0]));
        var providerStride = (long)((byte*)(&providerElements[1]) - (byte*)(&providerElements[0]));
        var first = Api.selected_inline_first;
        var second = Api.selected_inline_second;
        var distinct = (nint)first != (nint)second;
        var aligned = ((nuint)first) % 4 == 0 && ((nuint)second) % 4 == 0;
        var probeStorageAligned = ((nuint)sourceElements) % 4 == 0 && ((nuint)providerElements) % 4 == 0;
        var unchanged = ObjectsUnchanged(snapshot);
        Check(sizeof(SourceTracked) == 4 && sizeof(ProviderTracked) == 4 && idOffset == 0 && providerIdOffset == 0 &&
            sourcePrefixOffset == 4 && providerPrefixOffset == 4 && sourceStride == 4 && providerStride == 4 &&
            aligned && probeStorageAligned && distinct && unchanged && first->id == 1 && second->id == 2,
            "The two actual TU carriers or initialized global object identities differ.");
        return new(new JsonObject
        {
            ["trackedSize"] = sizeof(SourceTracked), ["trackedAlignment"] = sourcePrefixOffset, ["idOffset"] = idOffset,
            ["globalObjectsDistinct"] = distinct, ["globalStorageAligned"] = aligned, ["globalBytesUnchanged"] = unchanged,
            ["firstId"] = first->id, ["secondId"] = second->id
        }, new JsonObject
        {
            ["sourceTrackedSize"] = sizeof(SourceTracked), ["providerTrackedSize"] = sizeof(ProviderTracked),
            ["sourceIdOffset"] = idOffset, ["providerIdOffset"] = providerIdOffset,
            ["sourceSequentialPrefixOffset"] = sourcePrefixOffset, ["providerSequentialPrefixOffset"] = providerPrefixOffset,
            ["sourceStride"] = sourceStride, ["providerStride"] = providerStride, ["probeStorageAligned"] = probeStorageAligned,
            ["distinctTuClrCarriersMeasured"] = true, ["globalObjectBytesUnchangedBeforeShutdown"] = unchanged,
            ["inspectedWhileReady"] = true
        });
    }

    private static ReadOnlySpan<byte> SourceBytes() => new(Api.selected_inline_first, sizeof(SourceTracked));
    private static ReadOnlySpan<byte> ProviderBytes() => new(Api.selected_inline_second, sizeof(ProviderTracked));

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
