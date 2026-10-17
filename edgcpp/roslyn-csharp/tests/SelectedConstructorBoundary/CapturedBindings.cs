using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Api = global::Translated.Program;
using SourceCell = global::Translated.BoundaryCell;
using ProviderCell = global::Translated.BoundaryCell__3;

internal static unsafe partial class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SourceAlignmentProbe
    {
        public byte Prefix;
        public SourceCell Cell;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProviderAlignmentProbe
    {
        public byte Prefix;
        public ProviderCell Cell;
    }

    internal static partial void Initialize() => Api.InitializeSelectedProgram();
    internal static partial void Shutdown() => Api.ShutdownSelectedProgram();
    internal static partial int Fixture() => Api.fixture();
    internal static partial int Count() => Api.lifetime_count();
    internal static partial int Overflow() => Api.lifetime_overflow();
    internal static partial int Live() => Api.lifetime_live();
    internal static partial int Calls() => Api.startup_calls();
    internal static partial int Value(int index) => Api.lifetime_value(index);
    internal static partial int Event(int index) => Api.lifetime_event(index);
    internal static partial ManagedSelectedProgramLifetimeDiagnostics Diagnostics() => Api.SelectedProgramDiagnostics();
    internal static partial int ProviderSerialCounter() => *Api.next_serial;

    internal static partial GeneratedStorageSnapshot SnapshotStorage()
    {
        // This path is valid before initialization and after shutdown. It calls
        // no guarded application root and reads no BoundaryCell member.
        var count = *Api.event_count;
        Check(count is >= 0 and <= 64, "Actual event storage exceeded its finite capacity.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = ((int*)Api.values)[index];
        for (var index = 0; index < events.Length; ++index) events[index] = ((int*)Api.events)[index];
        // The captured lifetime_live body returns literal zero. Here zero is
        // the test protocol constant, not an ownership or lifetime observation.
        return new(*Api.fixture_calls, 0, *Api.event_overflow, values, events);
    }

    internal static partial GeneratedObjectSnapshot SnapshotObjects() => new(
        (nint)Api.first, (nint)Api.second,
        Bytes(Api.first).ToArray(), Bytes(Api.second).ToArray(), *Api.next_serial);

    internal static partial bool ObjectsUnchanged(GeneratedObjectSnapshot snapshot) =>
        (nint)Api.first == snapshot.FirstAddress && (nint)Api.second == snapshot.SecondAddress &&
        *Api.next_serial == snapshot.ProviderSerialCounter &&
        Bytes(Api.first).SequenceEqual(snapshot.FirstBytes) && Bytes(Api.second).SequenceEqual(snapshot.SecondBytes);

    internal static partial PhysicalObservation Physical(GeneratedObjectSnapshot snapshot)
    {
        SourceCell source = default;
        ProviderCell provider = default;
        SourceAlignmentProbe sourcePrefix = default;
        ProviderAlignmentProbe providerPrefix = default;
        SourceCell* sourceElements = stackalloc SourceCell[2];
        ProviderCell* providerElements = stackalloc ProviderCell[2];
        var canaryOffset = (long)((byte*)&source.canary - (byte*)&source);
        var serialOffset = (long)((byte*)&source.serial - (byte*)&source);
        var kindOffset = (long)((byte*)&source.kind - (byte*)&source);
        var providerCanaryOffset = (long)((byte*)&provider.canary - (byte*)&provider);
        var providerSerialOffset = (long)((byte*)&provider.serial - (byte*)&provider);
        var providerKindOffset = (long)((byte*)&provider.kind - (byte*)&provider);
        var sourcePrefixOffset = (long)((byte*)&sourcePrefix.Cell - (byte*)&sourcePrefix);
        var providerPrefixOffset = (long)((byte*)&providerPrefix.Cell - (byte*)&providerPrefix);
        var sourceStride = (long)((byte*)(&sourceElements[1]) - (byte*)(&sourceElements[0]));
        var providerStride = (long)((byte*)(&providerElements[1]) - (byte*)(&providerElements[0]));
        var first = Api.first;
        var second = Api.second;
        var distinct = first != second;
        var aligned = ((nuint)first) % 8 == 0 && ((nuint)second) % 8 == 0;
        var probeStorageAligned = ((nuint)sourceElements) % 8 == 0 && ((nuint)providerElements) % 8 == 0;
        var unchanged = ObjectsUnchanged(snapshot);
        var canariesIntact = first->canary == -0x6123456789ABCDE0L && second->canary == -0x6123456789ABCDE0L;
        Check(sizeof(SourceCell) == 16 && sizeof(ProviderCell) == 16 && sourcePrefixOffset == 8 && providerPrefixOffset == 8 &&
            canaryOffset == 0 && serialOffset == 8 && kindOffset == 12 && providerCanaryOffset == 0 &&
            providerSerialOffset == 8 && providerKindOffset == 12 && sourceStride == 16 && providerStride == 16 &&
            distinct && aligned && probeStorageAligned && unchanged && canariesIntact &&
            first->kind == 201 && second->kind == 201 && first->serial > 0 && first->serial != second->serial &&
            *Api.next_serial == 2,
            "Actual distinct-TU carriers, constructed fields, provider counter or unchanged global bytes differ.");

        var nativeCompatible = new JsonObject
        {
            ["cellSize"] = sizeof(SourceCell), ["cellAlignment"] = sourcePrefixOffset,
            ["canaryOffset"] = canaryOffset, ["serialOffset"] = serialOffset, ["kindOffset"] = kindOffset,
            ["globalObjectsDistinct"] = distinct, ["globalStorageAligned"] = aligned,
            ["globalBytesUnchanged"] = unchanged, ["firstSerial"] = first->serial, ["secondSerial"] = second->serial
        };
        var additional = new JsonObject
        {
            ["sourceCellSize"] = sizeof(SourceCell), ["providerCellSize"] = sizeof(ProviderCell),
            ["sourceSequentialPrefixOffset"] = sourcePrefixOffset, ["providerSequentialPrefixOffset"] = providerPrefixOffset,
            ["sourceStride"] = sourceStride, ["providerStride"] = providerStride,
            ["providerCanaryOffset"] = providerCanaryOffset, ["providerSerialOffset"] = providerSerialOffset,
            ["providerKindOffset"] = providerKindOffset, ["probeStorageAligned"] = probeStorageAligned,
            ["providerSerialCounter"] = *Api.next_serial, ["fullWidthCanariesIntact"] = canariesIntact,
            ["firstUnsignedKind"] = first->kind, ["secondUnsignedKind"] = second->kind,
            ["distinctTuClrCarriersMeasured"] = true,
            ["constructedByActualSelectedStartup"] = true, ["manualConstructorOrDestructorCalls"] = false
        };
        return new(nativeCompatible, additional);
    }

    private static ReadOnlySpan<byte> Bytes(SourceCell* pointer) => new(pointer, sizeof(SourceCell));

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
