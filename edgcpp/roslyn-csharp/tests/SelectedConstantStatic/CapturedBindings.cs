using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Api = global::Translated.Program;
using SharedArray = global::Translated.Array_sbyte_50;
using DifferentArray = global::Translated.Array_sbyte_50__2;

internal static unsafe partial class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SharedAlignmentProbe
    {
        public byte Prefix;
        public SharedArray Array;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DifferentAlignmentProbe
    {
        public byte Prefix;
        public DifferentArray Array;
    }

    internal static partial int Fixture() => Api.fixture();
    internal static partial int Count() => Api.lifetime_count();
    internal static partial int Overflow() => Api.lifetime_overflow();
    internal static partial int Live() => Api.lifetime_live();
    internal static partial int Value(int index) => Api.lifetime_value(index);
    internal static partial int Event(int index) => Api.lifetime_event(index);
    internal static partial nint SharedA() => (nint)Api.selected_constant_a();
    internal static partial nint SharedB() => (nint)Api.selected_constant_b();
    internal static partial nint DifferentA() => (nint)Api.selected_different_a();
    internal static partial nint DifferentB() => (nint)Api.selected_different_b();

    internal static partial JsonObject Physical(ConstantAddresses addresses)
    {
        SharedArray shared = default;
        DifferentArray different = default;
        SharedAlignmentProbe sharedPrefix = default;
        DifferentAlignmentProbe differentPrefix = default;
        SharedArray* sharedElements = stackalloc SharedArray[2];
        DifferentArray* differentElements = stackalloc DifferentArray[2];
        var sharedElementOffset = (long)((byte*)&shared.__firstElement - (byte*)&shared);
        var differentElementOffset = (long)((byte*)&different.__firstElement - (byte*)&different);
        var sharedPrefixOffset = (long)((byte*)&sharedPrefix.Array - (byte*)&sharedPrefix);
        var differentPrefixOffset = (long)((byte*)&differentPrefix.Array - (byte*)&differentPrefix);
        var sharedStride = (long)((byte*)(&sharedElements[1]) - (byte*)(&sharedElements[0]));
        var differentStride = (long)((byte*)(&differentElements[1]) - (byte*)(&differentElements[0]));
        var sharedStorage = Api.shared_bytes;
        var differentStorage = Api.different_bytes;
        var sharedStorageAddress = (nint)sharedStorage;
        var differentStorageAddress = (nint)differentStorage;
        var sharedActualElementOffset = (long)((byte*)&sharedStorage->__firstElement - (byte*)sharedStorage);
        var differentActualElementOffset = (long)((byte*)&differentStorage->__firstElement - (byte*)differentStorage);
        var matchesCanonical = addresses.SharedA == sharedStorageAddress && addresses.SharedB == sharedStorageAddress &&
            addresses.DifferentA == differentStorageAddress && addresses.DifferentB == differentStorageAddress;
        var distinct = sharedStorageAddress != differentStorageAddress;
        var terminators = ((sbyte*)sharedStorage)[49] == 0 && ((sbyte*)differentStorage)[49] == 0;
        Check(sizeof(SharedArray) == 50 && sizeof(DifferentArray) == 50 &&
            sharedElementOffset == 0 && differentElementOffset == 0 && sharedPrefixOffset == 1 && differentPrefixOffset == 1 &&
            sharedStride == 50 && differentStride == 50 && sharedActualElementOffset == 0 && differentActualElementOffset == 0 &&
            matchesCanonical && sharedStorageAddress != 0 && differentStorageAddress != 0 && distinct && terminators,
            "Actual canonical constant storage, array carriers or returned wrapper addresses differ.");
        return new JsonObject
        {
            ["sharedArraySize"] = sizeof(SharedArray), ["differentArraySize"] = sizeof(DifferentArray),
            ["sharedFirstElementOffset"] = sharedElementOffset, ["differentFirstElementOffset"] = differentElementOffset,
            ["sharedSequentialPrefixOffset"] = sharedPrefixOffset, ["differentSequentialPrefixOffset"] = differentPrefixOffset,
            ["sharedStride"] = sharedStride, ["differentStride"] = differentStride,
            ["actualSharedFirstElementOffset"] = sharedActualElementOffset,
            ["actualDifferentFirstElementOffset"] = differentActualElementOffset,
            ["allFourWrappersMatchCanonicalStorage"] = matchesCanonical,
            ["differentNamedCanonicalStorageDistinct"] = distinct,
            ["bothTerminatorsPresent"] = terminators,
            ["arrayElementStorage"] = "signed8 carrier; bytes inspected without writes",
            ["initializationObservation"] = "actual emitted constant storage and fresh bytes; no synthetic execution counter"
        };
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
