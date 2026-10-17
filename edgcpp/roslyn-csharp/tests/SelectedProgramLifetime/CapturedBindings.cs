using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Api = global::Translated.Program;
using StartupItem = global::Translated.StartupItem;

internal static unsafe partial class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct StartupItemAlignmentProbe
    {
        public byte Prefix;
        public StartupItem Item;
    }

    internal static partial void Initialize() => Api.InitializeSelectedProgram();
    internal static partial void Shutdown() => Api.ShutdownSelectedProgram();
    internal static partial int Fixture() => Api.fixture();
    internal static partial int Count() => Api.lifetime_count();
    internal static partial int Overflow() => Api.lifetime_overflow();
    internal static partial int Live() => Api.lifetime_live();
    internal static partial int Constructed() => Api.startup_constructed();
    internal static partial int Destroyed() => Api.startup_destroyed();
    internal static partial int Calls() => Api.startup_calls();
    internal static partial int Value(int index) => Api.lifetime_value(index);
    internal static partial int Event(int index) => Api.lifetime_event(index);
    internal static partial ManagedSelectedProgramLifetimeDiagnostics Diagnostics() => Api.SelectedProgramDiagnostics();
    internal static partial nint FirstObjectAddress() => (nint)Api.first;
    internal static partial nint SecondObjectAddress() => (nint)Api.second;

    internal static partial GeneratedStorageSnapshot SnapshotStorage()
    {
        // Independent scalar/array storage only. In particular, no advertised
        // source method or destroyed StartupItem member is read on this path.
        var count = *Api.event_count;
        Check(count is >= 0 and <= 64, "The final generated event count exceeded the captured capacity.");
        var values = new int[9];
        var events = new int[count];
        for (var index = 0; index < values.Length; ++index) values[index] = ((int*)Api.values)[index];
        for (var index = 0; index < events.Length; ++index) events[index] = ((int*)Api.events)[index];
        return new(*Api.live_count, *Api.constructed_count, *Api.destroyed_count,
            *Api.fixture_calls, *Api.event_overflow, values, events);
    }

    internal static partial JsonObject Physical()
    {
        StartupItem item = default;
        StartupItemAlignmentProbe prefix = default;
        StartupItem* elements = stackalloc StartupItem[2];
        var idOffset = (long)((byte*)&item.id - (byte*)&item);
        var prefixOffset = (long)((byte*)&prefix.Item - (byte*)&prefix);
        var stride = (long)((byte*)&elements[1] - (byte*)&elements[0]);
        var first = Api.first;
        var second = Api.second;
        var aligned = ((nuint)first) % 4 == 0 && ((nuint)second) % 4 == 0 && ((nuint)elements) % 4 == 0;
        Check(sizeof(StartupItem) == 4 && idOffset == 0 && prefixOffset == 4 && stride == 4 &&
            first != second && aligned && first->id == 1 && second->id == 2,
            "The actual generated StartupItem carrier, global identities or initialized fields differ.");
        return new JsonObject
        {
            ["startupItemSize"] = sizeof(StartupItem), ["idOffset"] = idOffset,
            ["sequentialPrefixOffset"] = prefixOffset, ["elementStride"] = stride,
            ["actualStorageAligned"] = aligned, ["globalObjectsDistinct"] = first != second,
            ["firstInitializedId"] = first->id, ["secondInitializedId"] = second->id,
            ["inspectedWhileReady"] = true
        };
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
