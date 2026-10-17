using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Api = global::Translated.Program;
using ConsumerCell = global::Translated.SelectedProviderCell__2;
using ProviderCell = global::Translated.SelectedProviderCell__3;

internal static unsafe partial class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ConsumerAlignmentProbe { public byte Prefix; public ConsumerCell Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProviderAlignmentProbe { public byte Prefix; public ProviderCell Value; }

    internal static partial int Fixture() => Api.fixture();
    internal static partial int ValueCount() => Api.lifetime_value_count();
    internal static partial int Value(int index) => Api.lifetime_value(index);
    internal static partial int Count() => Api.lifetime_count();
    internal static partial int Event(int index) => Api.lifetime_event(index);
    internal static partial int Overflow() => Api.lifetime_overflow();
    internal static partial int Live()
    {
        var value = Api.lifetime_live();
        Check(value == *Api.selected_suppressed_live, "Live protocol differs from actual generated scalar storage.");
        return value;
    }
    internal static partial int Constructed()
    {
        var value = Api.lifetime_constructed();
        Check(value == *Api.selected_suppressed_constructed, "Constructed protocol differs from actual generated scalar storage.");
        return value;
    }
    internal static partial int Destroyed()
    {
        var value = Api.lifetime_destroyed();
        Check(value == *Api.selected_suppressed_destroyed, "Destroyed protocol differs from actual generated scalar storage.");
        return value;
    }

    internal static partial JsonObject Physical()
    {
        // These are raw CLR carrier measurements. Only Fixture() executes the
        // actual selected C++ construction/destruction calls and live counters.
        ConsumerCell consumer = default;
        ProviderCell provider = default;
        ConsumerAlignmentProbe consumerPrefix = default;
        ProviderAlignmentProbe providerPrefix = default;
        ConsumerCell* consumers = stackalloc ConsumerCell[2];
        ProviderCell* providers = stackalloc ProviderCell[2];
        var consumerOffset = (long)((byte*)&consumer.value - (byte*)&consumer);
        var providerOffset = (long)((byte*)&provider.value - (byte*)&provider);
        var consumerAlignment = (long)((byte*)&consumerPrefix.Value - (byte*)&consumerPrefix);
        var providerAlignment = (long)((byte*)&providerPrefix.Value - (byte*)&providerPrefix);
        var consumerStride = (long)((byte*)(&consumers[1]) - (byte*)(&consumers[0]));
        var providerStride = (long)((byte*)(&providers[1]) - (byte*)(&providers[0]));
        var aligned = ((nuint)consumers) % 4 == 0 && ((nuint)providers) % 4 == 0 &&
            ((nuint)(&consumer)) % 4 == 0 && ((nuint)(&provider)) % 4 == 0;
        var fieldAddresses = (nint)(&consumer.value) == (nint)(&consumer) &&
            (nint)(&provider.value) == (nint)(&provider) &&
            (nint)(&consumers[0].value) == (nint)(&consumers[0]) &&
            (nint)(&providers[1].value) == (nint)(&providers[1]);

        consumer.value = 0x13579;
        provider.value = -0x2468;
        consumers[0].value = 17;
        consumers[1].value = 29;
        providers[0].value = 41;
        providers[1].value = 53;
        var independent = consumer.value == 0x13579 && provider.value == -0x2468 &&
            consumers[0].value == 17 && consumers[1].value == 29 &&
            providers[0].value == 41 && providers[1].value == 53;
        Check(sizeof(ConsumerCell) == 4 && sizeof(ProviderCell) == 4 &&
            consumerOffset == 0 && providerOffset == 0 && consumerAlignment == 4 && providerAlignment == 4 &&
            consumerStride == 4 && providerStride == 4 && aligned && fieldAddresses && independent,
            "The actual consumer/provider complete carriers, field addresses or independent stores differ.");
        return new JsonObject
        {
            ["consumerSize"] = sizeof(ConsumerCell), ["providerSize"] = sizeof(ProviderCell),
            ["consumerValueOffset"] = consumerOffset, ["providerValueOffset"] = providerOffset,
            ["consumerSequentialPrefixOffset"] = consumerAlignment, ["providerSequentialPrefixOffset"] = providerAlignment,
            ["consumerStride"] = consumerStride, ["providerStride"] = providerStride,
            ["allProbeStorageAligned"] = aligned, ["fieldAddressesMatch"] = fieldAddresses,
            ["independentScalarStores"] = independent,
            ["probeKind"] = "raw CLR complete carriers; no application constructor/destructor or C++ lifetime claim"
        };
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
