using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Api = global::Translated.Program;
using SourceWhole = global::Translated.Whole__2;
using SourceMember = global::Translated.Member__2;
using ProviderWhole = global::Translated.Whole__3;
using ProviderMember = global::Translated.Member__3;

internal static unsafe partial class CapturedBindings
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SourceWholeAlignmentProbe { public byte Prefix; public SourceWhole Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct SourceMemberAlignmentProbe { public byte Prefix; public SourceMember Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProviderWholeAlignmentProbe { public byte Prefix; public ProviderWhole Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProviderMemberAlignmentProbe { public byte Prefix; public ProviderMember Value; }

    internal static partial int Fixture() => Api.fixture();
    internal static partial int Count() => Api.lifetime_count();
    internal static partial int Overflow() => Api.lifetime_overflow();
    internal static partial int Live() => Api.lifetime_live();
    internal static partial int Value(int index) => Api.lifetime_value(index);
    internal static partial int Event(int index) => Api.lifetime_event(index);
    // Created/destroyed helpers are not advertised in this publication. These
    // reads inspect their actual independent application counter storage.
    internal static partial int Created() => *Api.selected_metadata_created;
    internal static partial int Destroyed() => *Api.selected_metadata_destroyed;
    internal static partial int ArgumentCount() => *Api.selected_metadata_argument_count;

    internal static partial PhysicalObservation Physical()
    {
        // Raw CLR carrier probes only. No translated C++ constructor or
        // destructor is invoked, and no application lifetime unit is added.
        SourceWhole source = default;
        SourceMember sourceMember = default;
        ProviderWhole provider = default;
        ProviderMember providerMember = default;
        SourceWholeAlignmentProbe sourceWholePrefix = default;
        SourceMemberAlignmentProbe sourceMemberPrefix = default;
        ProviderWholeAlignmentProbe providerWholePrefix = default;
        ProviderMemberAlignmentProbe providerMemberPrefix = default;
        SourceWhole* sourceWholes = stackalloc SourceWhole[2];
        SourceMember* sourceMembers = stackalloc SourceMember[2];
        ProviderWhole* providerWholes = stackalloc ProviderWhole[2];
        ProviderMember* providerMembers = stackalloc ProviderMember[2];

        var prefixOffset = (long)((byte*)&source.prefix - (byte*)&source);
        var memberOffset = (long)((byte*)&source.member - (byte*)&source);
        var idOffset = (long)((byte*)&sourceMember.id - (byte*)&sourceMember);
        var nestedIdOffset = (long)((byte*)&source.member.id - (byte*)&source);
        var providerPrefixOffset = (long)((byte*)&provider.prefix - (byte*)&provider);
        var providerMemberOffset = (long)((byte*)&provider.member - (byte*)&provider);
        var providerIdOffset = (long)((byte*)&providerMember.id - (byte*)&providerMember);
        var providerNestedIdOffset = (long)((byte*)&provider.member.id - (byte*)&provider);
        var sourceWholeAlignment = (long)((byte*)&sourceWholePrefix.Value - (byte*)&sourceWholePrefix);
        var sourceMemberAlignment = (long)((byte*)&sourceMemberPrefix.Value - (byte*)&sourceMemberPrefix);
        var providerWholeAlignment = (long)((byte*)&providerWholePrefix.Value - (byte*)&providerWholePrefix);
        var providerMemberAlignment = (long)((byte*)&providerMemberPrefix.Value - (byte*)&providerMemberPrefix);
        var sourceWholeStride = (long)((byte*)(&sourceWholes[1]) - (byte*)(&sourceWholes[0]));
        var sourceMemberStride = (long)((byte*)(&sourceMembers[1]) - (byte*)(&sourceMembers[0]));
        var providerWholeStride = (long)((byte*)(&providerWholes[1]) - (byte*)(&providerWholes[0]));
        var providerMemberStride = (long)((byte*)(&providerMembers[1]) - (byte*)(&providerMembers[0]));
        var aligned = ((nuint)sourceWholes) % 4 == 0 && ((nuint)sourceMembers) % 4 == 0 &&
            ((nuint)providerWholes) % 4 == 0 && ((nuint)providerMembers) % 4 == 0;

        source.prefix = 0x13579;
        source.member.id = 0x2468;
        provider.prefix = 0x34567;
        provider.member.id = 0x4567;
        var nestedAddresses = (nint)(&source.member.id) == (nint)(&source.member) &&
            (nint)(&provider.member.id) == (nint)(&provider.member);
        var fieldsIndependent = source.prefix == 0x13579 && source.member.id == 0x2468 &&
            provider.prefix == 0x34567 && provider.member.id == 0x4567;
        Check(sizeof(SourceWhole) == 8 && sizeof(ProviderWhole) == 8 && sizeof(SourceMember) == 4 && sizeof(ProviderMember) == 4 &&
            prefixOffset == 0 && memberOffset == 4 && idOffset == 0 && nestedIdOffset == 4 &&
            providerPrefixOffset == 0 && providerMemberOffset == 4 && providerIdOffset == 0 && providerNestedIdOffset == 4 &&
            sourceWholeAlignment == 4 && sourceMemberAlignment == 4 && providerWholeAlignment == 4 && providerMemberAlignment == 4 &&
            sourceWholeStride == 8 && sourceMemberStride == 4 && providerWholeStride == 8 && providerMemberStride == 4 &&
            aligned && nestedAddresses && fieldsIndependent,
            "The actual owning-TU Whole/Member carriers or nested member addresses differ.");

        var nativeCompatible = new JsonObject
        {
            ["wholeSize"] = sizeof(SourceWhole), ["wholeAlignment"] = sourceWholeAlignment,
            ["prefixOffset"] = prefixOffset, ["memberOffset"] = memberOffset,
            ["memberSize"] = sizeof(SourceMember), ["memberAlignment"] = sourceMemberAlignment,
            ["memberIdOffset"] = idOffset
        };
        var additional = new JsonObject
        {
            ["providerWholeSize"] = sizeof(ProviderWhole), ["providerMemberSize"] = sizeof(ProviderMember),
            ["providerPrefixOffset"] = providerPrefixOffset, ["providerMemberOffset"] = providerMemberOffset,
            ["providerMemberIdOffset"] = providerIdOffset, ["sourceNestedMemberIdOffset"] = nestedIdOffset,
            ["providerNestedMemberIdOffset"] = providerNestedIdOffset,
            ["sourceWholeSequentialPrefixOffset"] = sourceWholeAlignment,
            ["sourceMemberSequentialPrefixOffset"] = sourceMemberAlignment,
            ["providerWholeSequentialPrefixOffset"] = providerWholeAlignment,
            ["providerMemberSequentialPrefixOffset"] = providerMemberAlignment,
            ["sourceWholeStride"] = sourceWholeStride, ["sourceMemberStride"] = sourceMemberStride,
            ["providerWholeStride"] = providerWholeStride, ["providerMemberStride"] = providerMemberStride,
            ["allProbeStorageAligned"] = aligned, ["nestedMemberAddressesMatch"] = nestedAddresses,
            ["scalarAndNestedFieldsIndependent"] = fieldsIndependent,
            ["probeKind"] = "raw CLR stack carriers; no source constructor/destructor or C++ lifetime claim"
        };
        return new(nativeCompatible, additional);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
