using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using LeftRecord = Translated.Left__2;
using RightRecord = Translated.Right__2;
using DerivedRecord = Translated.Derived__2;
using LeftView = Translated.Record__3;
using RightView = Translated.Record__4;

internal static unsafe class Checks
{
    private const string ProviderHash = "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
    private const string ProviderPath = "/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33";
    private const string ClassExport = "_ZTVN10__cxxabiv117__class_type_infoE";
    private const string MultipleInheritanceExport = "_ZTVN10__cxxabiv121__vmi_class_type_infoE";

    [StructLayout(LayoutKind.Sequential)]
    private struct LeftAlignmentProbe { public byte Prefix; public LeftRecord Value; }

    [StructLayout(LayoutKind.Sequential)]
    private struct RightAlignmentProbe { public byte Prefix; public RightRecord Value; }

    [StructLayout(LayoutKind.Sequential)]
    private struct DerivedAlignmentProbe { public byte Prefix; public DerivedRecord Value; }

    public static void Main(string[] args)
    {
        if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
        var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations.");
        var runs = new JsonArray();
        for (var run = 0; run < 2; ++run) runs.Add((JsonNode)RunFixture());
        Require(JsonNode.DeepEquals(runs[0], runs[1]), "Repeated fixture observations differ.");
        var observations = new JsonObject { ["runs"] = runs };
        Require(JsonNode.DeepEquals(observations, native), "Generated observations differ from the separate native oracle.");
        observations["physical"] = CheckPhysicalExecution(native);
        observations["providers"] = CheckProviders();
        Require(Application.lifetime_live() == 0 && Application.lifetime_overflow() == 0,
            "Physical checks leave live objects or event overflow.");
        Console.WriteLine(observations.ToJsonString());
    }

    private static JsonObject RunFixture()
    {
        var result = Application.fixture();
        var count = Application.lifetime_count();
        var overflow = Application.lifetime_overflow();
        var live = Application.lifetime_live();
        Require(count is >= 0 and <= 64 && overflow == 0 && live == 0,
            "Fixture event bounds or live/overflow invariant failed.");
        var values = new JsonArray();
        for (var index = 0; index < 9; ++index)
            values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
        Require(Application.lifetime_value(8) == 0, "Fixture entered with prior live objects.");
        var events = new JsonArray();
        for (var index = 0; index < count; ++index)
            events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
        return new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live,
            ["values"] = values, ["events"] = events };
    }

    private static JsonObject CheckPhysicalExecution(JsonNode native)
    {
        Require(sizeof(LeftRecord) == 16 && sizeof(RightRecord) == 16 && sizeof(DerivedRecord) == 32 &&
            sizeof(LeftView) == 12 && sizeof(RightView) == 12,
            "Complete records or borrowed occupied-base views have incorrect sizes.");
        LeftAlignmentProbe leftAlignment = new() { Prefix = 1, Value = default };
        RightAlignmentProbe rightAlignment = new() { Prefix = 1, Value = default };
        DerivedAlignmentProbe derivedAlignment = new() { Prefix = 1, Value = default };
        Require((byte*)&leftAlignment.Value - (byte*)&leftAlignment == 8 &&
            (byte*)&rightAlignment.Value - (byte*)&rightAlignment == 8 &&
            (byte*)&derivedAlignment.Value - (byte*)&derivedAlignment == 8,
            "Complete record alignment is not 8 in sequential containing records.");

        LeftRecord* left = stackalloc LeftRecord[2];
        RightRecord* right = stackalloc RightRecord[2];
        DerivedRecord* derived = stackalloc DerivedRecord[2];
        Require(((nuint)left & 7) == 0 && ((nuint)right & 7) == 0 && ((nuint)derived & 7) == 0 &&
            (byte*)(left + 1) - (byte*)left == 16 && (byte*)(right + 1) - (byte*)right == 16 &&
            (byte*)(derived + 1) - (byte*)derived == 32,
            "Complete object owners have incorrect observed alignment or stride.");
        Require((byte*)&left[0].__vptr - (byte*)&left[0] == 0 &&
            (byte*)&left[0].left_id - (byte*)&left[0] == 8 &&
            (byte*)&right[0].__vptr - (byte*)&right[0] == 0 &&
            (byte*)&right[0].right_id - (byte*)&right[0] == 8 &&
            (byte*)&derived[0].__layout_alignment - (byte*)&derived[0] == 0 &&
            (byte*)&derived[0].__b_4Left - (byte*)&derived[0] == 0 &&
            (byte*)&derived[0].__b_5Right - (byte*)&derived[0] == 16 &&
            (byte*)&derived[0].__b_4Left.left_id - (byte*)&derived[0] == 8 &&
            (byte*)&derived[0].__b_5Right.right_id - (byte*)&derived[0] == 24 &&
            (byte*)&derived[0].bonus - (byte*)&derived[0] == 28,
            "Physical field offsets do not match the captured layout.");

        var leftTable = (long*)Application.storage__3;
        var rightTable = (long*)Application.storage__6;
        var derivedTable = (long*)Application.storage__9;
        Require(leftTable != null && rightTable != null && derivedTable != null &&
            leftTable != rightTable && leftTable != derivedTable && rightTable != derivedTable &&
            ((nuint)leftTable & 7) == 0 && ((nuint)rightTable & 7) == 0 && ((nuint)derivedTable & 7) == 0,
            "Generated application tables are not distinct aligned storage.");
        Require(leftTable[0] == 0 && rightTable[0] == 0 && derivedTable[0] == 0 && derivedTable[4] == -16 &&
            leftTable[1] == (long)(nint)Application.storage && rightTable[1] == (long)(nint)Application.storage__4 &&
            derivedTable[1] == (long)(nint)Application.storage__7 && derivedTable[5] == (long)(nint)Application.storage__7,
            "Application tables did not preserve signed prefixes and generated typeinfo identities.");
        nint[] canonicalSlots = [(nint)leftTable[2], (nint)rightTable[2], (nint)derivedTable[2],
            (nint)derivedTable[3], (nint)derivedTable[6]];
        Require(canonicalSlots.All(address => address != 0) && canonicalSlots.Distinct().Count() == 5,
            "All five retained canonical entries must be nonzero and distinct, including direct override and thunk.");

        var before = Application.lifetime_count();
        Require(before is >= 0 and <= 39 && Application.lifetime_live() == 0,
            "Physical execution must begin with bounded observations and no live objects.");
        var constructedLeft = 0;
        var constructedRight = 0;
        var constructedDerived = 0;
        int[] results = new int[5];
        var callStart = 0;
        nint completeReceiver = 0;
        nint secondaryReceiver = 0;
        try
        {
            for (; constructedLeft < 2; ++constructedLeft) Application.Left(left + constructedLeft, 2);
            for (; constructedRight < 2; ++constructedRight) Application.Right(right + constructedRight, 3);
            for (; constructedDerived < 2; ++constructedDerived) Application.Derived(derived + constructedDerived, 5, 7, 11);
            Require(Application.lifetime_live() == 10, "Actual constructors did not establish the complete lifetime units.");
            for (var index = 0; index < 2; ++index)
            {
                Require(left[index].left_id == 2 && left[index].__vptr == leftTable + 2 &&
                    right[index].right_id == 3 && right[index].__vptr == rightTable + 2,
                    "Complete base objects have incorrect fields or canonical address points.");
                Require(derived[index].__b_4Left.left_id == 5 && derived[index].__b_5Right.right_id == 7 &&
                    derived[index].bonus == 11 && derived[index].__b_4Left.__vptr == derivedTable + 2 &&
                    derived[index].__b_5Right.__vptr == derivedTable + 6 &&
                    derived[index].__layout_alignment == (nint)(derivedTable + 2),
                    "Derived base views, reused tail or actual address points are incorrect.");
                Require((nint)left[index].__vptr[0] == canonicalSlots[0] &&
                    (nint)right[index].__vptr[0] == canonicalSlots[1] &&
                    (nint)derived[index].__b_4Left.__vptr[0] == canonicalSlots[2] &&
                    (nint)derived[index].__b_4Left.__vptr[1] == canonicalSlots[3] &&
                    (nint)derived[index].__b_5Right.__vptr[0] == canonicalSlots[4],
                    "Actual loaded entries are not stable across complete receivers.");
            }
            var rightSubobject = (RightRecord*)&derived[0].__b_5Right;
            completeReceiver = (nint)derived;
            secondaryReceiver = (nint)rightSubobject;
            Require((byte*)rightSubobject - (byte*)derived == 16 &&
                derived[0].__b_4Left.__vptr[-2] == 0 && rightSubobject->__vptr[-2] == -16,
                "Actual secondary receiver or signed address-point prefixes are incorrect.");

            callStart = Application.lifetime_count();
            var leftCall = (delegate* unmanaged[Cdecl]<LeftRecord*, int, int>)(nint)left[0].__vptr[0];
            var rightCall = (delegate* unmanaged[Cdecl]<RightRecord*, int, int>)(nint)right[0].__vptr[0];
            var derivedLeftCall = (delegate* unmanaged[Cdecl]<DerivedRecord*, int, int>)(nint)derived[0].__b_4Left.__vptr[0];
            var directRightCall = (delegate* unmanaged[Cdecl]<DerivedRecord*, int, int>)(nint)derived[0].__b_4Left.__vptr[1];
            var thunkRightCall = (delegate* unmanaged[Cdecl]<RightRecord*, int, int>)(nint)rightSubobject->__vptr[0];
            results[0] = leftCall(left, 3);
            results[1] = rightCall(right, 4);
            results[2] = derivedLeftCall(derived, 13);
            results[3] = directRightCall(derived, 17);
            // Pass the actual secondary-subobject raw address. The selected
            // generated thunk body, not this test, performs its -16 adjustment.
            results[4] = thunkRightCall(rightSubobject, 17);
            int[] nativeValueIndices = [0, 1, 2, 3, 3];
            int[] nativeEventIndices = [6, 8, 10, 12, 12];
            Require(Application.lifetime_count() == callStart + 5 && Application.lifetime_live() == 10,
                "Five loaded UCO calls did not execute exactly their method events or preserve receiver lifetimes.");
            for (var index = 0; index < 5; ++index)
                Require(results[index] == NativeInteger(native, "values", nativeValueIndices[index]) &&
                    Application.lifetime_event(callStart + index) == NativeInteger(native, "events", nativeEventIndices[index]),
                    "A canonical loaded entry differs from the selected native method result/event.");
            Require(results[3] == results[4] && canonicalSlots[3] != canonicalSlots[4] &&
                derived[0].__b_4Left.left_id == 5 && derived[0].__b_5Right.right_id == 7 && derived[0].bonus == 11,
                "Distinct direct/thunk paths did not preserve the same complete object and override result.");
        }
        finally
        {
            while (constructedDerived > 0) Application._Derived(derived + --constructedDerived);
            while (constructedRight > 0) Application._Right(right + --constructedRight);
            while (constructedLeft > 0) Application._Left(left + --constructedLeft);
        }
        Require(Application.lifetime_live() == 0 && Application.lifetime_overflow() == 0,
            "Actual generated destructors did not balance complete constructor lifetimes.");
        Require(leftTable[2] == (long)canonicalSlots[0] && rightTable[2] == (long)canonicalSlots[1] &&
            derivedTable[2] == (long)canonicalSlots[2] && derivedTable[3] == (long)canonicalSlots[3] &&
            derivedTable[6] == (long)canonicalSlots[4] && derivedTable[0] == 0 && derivedTable[4] == -16,
            "Canonical entries or signed table prefixes changed during actual lifetimes.");

        var calls = new JsonArray();
        string[] roles = ["complete-left", "complete-right", "derived-left", "derived-right-direct", "derived-right-thunk"];
        for (var index = 0; index < 5; ++index)
            calls.Add((JsonNode)new JsonObject { ["role"] = roles[index], ["address"] = AddressText(canonicalSlots[index]),
                ["result"] = results[index], ["event"] = Application.lifetime_event(callStart + index) });
        var effects = new JsonArray();
        for (var index = before; index < Application.lifetime_count(); ++index)
            effects.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
        return new JsonObject
        {
            ["leftSize"] = sizeof(LeftRecord), ["rightSize"] = sizeof(RightRecord), ["derivedSize"] = sizeof(DerivedRecord),
            ["leftViewSize"] = sizeof(LeftView), ["rightViewSize"] = sizeof(RightView), ["completeAlignment"] = 8,
            ["leftBaseOffset"] = 0, ["rightBaseOffset"] = 16, ["derivedBonusOffset"] = 28,
            ["primaryAddressPointByteOffset"] = 16, ["secondaryAddressPointByteOffset"] = 48,
            ["primaryOffsetToTop"] = 0, ["secondaryOffsetToTop"] = -16,
            ["completeReceiver"] = AddressText(completeReceiver), ["secondaryReceiver"] = AddressText(secondaryReceiver),
            ["completeOwnersAligned"] = true, ["canonicalSlotsStableAndDistinct"] = true,
            ["bothRightEntriesExecuted"] = true, ["balancedGeneratedLifetimes"] = true,
            ["calls"] = calls, ["events"] = effects
        };
    }

    private static JsonArray CheckProviders()
    {
        var resolutions = BorrowedNativeRttiData.GetDiagnostics();
        Require(resolutions.Count == 2, "Expected exactly class and VMI borrowed data exports for this input.");
        Require(sizeof(Translated.__class_type_info) == 16 && sizeof(Translated.Record) == 56 &&
            ((nuint)Application.storage & 7) == 0 && ((nuint)Application.storage__4 & 7) == 0 &&
            ((nuint)Application.storage__7 & 7) == 0,
            "Generated class/VMI metadata storage has incorrect captured size or observed alignment.");
        var metadata = Application.storage__7;
        var baseInfo = (Translated.Record__2*)&metadata->base_info;
        Require((byte*)&metadata->flags - (byte*)metadata == 16 &&
            (byte*)&metadata->base_count - (byte*)metadata == 20 &&
            (byte*)&metadata->base_info - (byte*)metadata == 24 && metadata->flags == 0 && metadata->base_count == 2 &&
            baseInfo[0].tinfo == Application.storage && baseInfo[0].offset_flags == 2 &&
            baseInfo[1].tinfo == Application.storage__4 && baseInfo[1].offset_flags == 4098,
            "Anonymous generated metadata did not preserve its actual captured fields and raw constants.");

        var rows = new JsonArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in resolutions)
        {
            Require(seen.Add(row.ExportName), "Repeated provider export diagnostic.");
            var expectedRva = row.ExportName switch
            {
                ClassExport => 0x26dd18UL,
                MultipleInheritanceExport => 0x26ec98UL,
                _ => throw new InvalidOperationException("Unexpected native data export for this input.")
            };
            Require(row.CanonicalLibraryPath == ProviderPath && row.LibrarySha256 == ProviderHash &&
                row.PinnedProviderElfSymbolVersion == "CXXABI_1.3" && row.ExportRelativeVirtualAddress == expectedRva &&
                row.SymbolSizeBytes == 88 && row.LoadedOwnerVerified && row.ReadOnlyMappingVerified &&
                row.MappingPermissions == "r--p" && row.SymbolBaseAddress > 0 && row.ModuleLoadBaseAddress > 0 &&
                (ulong)(nuint)row.SymbolBaseAddress == checked((ulong)(nuint)row.ModuleLoadBaseAddress + expectedRva) &&
                row.MappingStartAddress <= row.SymbolBaseAddress &&
                checked((ulong)(nuint)row.SymbolBaseAddress + 88) <= (ulong)(nuint)row.MappingEndAddress,
                "Borrowed data diagnostics do not match the audited descriptor and exact owner mapping.");
            using (var file = File.OpenRead(row.CanonicalLibraryPath))
                Require(Convert.ToHexStringLower(SHA256.HashData(file)) == ProviderHash, "Physical provider file hash changed.");
            var handle = NativeLibrary.Load(row.CanonicalLibraryPath);
            Require(NativeLibrary.GetExport(handle, row.ExportName) == row.SymbolBaseAddress,
                "Independent exact-provider GetExport differs from the reported symbol base.");
            VerifyMapping(row);
            var storedVptrs = new JsonArray();
            if (row.ExportName == ClassExport)
            {
                CheckStoredVptr((nint)Application.storage->@base.__vptr, row.SymbolBaseAddress, storedVptrs);
                CheckStoredVptr((nint)Application.storage__4->@base.__vptr, row.SymbolBaseAddress, storedVptrs);
            }
            else CheckStoredVptr((nint)metadata->@base.@base.__vptr, row.SymbolBaseAddress, storedVptrs);
            rows.Add((JsonNode)new JsonObject
            {
                ["canonicalLibraryPath"] = row.CanonicalLibraryPath, ["librarySha256"] = row.LibrarySha256,
                ["exportName"] = row.ExportName, ["pinnedProviderElfSymbolVersion"] = row.PinnedProviderElfSymbolVersion,
                ["symbolBaseAddress"] = AddressText(row.SymbolBaseAddress),
                ["moduleLoadBaseAddress"] = AddressText(row.ModuleLoadBaseAddress),
                ["exportRelativeVirtualAddress"] = row.ExportRelativeVirtualAddress,
                ["providerElfSymbolSizeBytes"] = row.SymbolSizeBytes,
                ["mappingStartAddress"] = AddressText(row.MappingStartAddress),
                ["mappingEndAddress"] = AddressText(row.MappingEndAddress),
                ["mappingPermissions"] = row.MappingPermissions, ["loadedOwnerVerified"] = true,
                ["readOnlyMappingVerified"] = true, ["independentExportMatch"] = true,
                ["independentMappingMatch"] = true, ["capturedAddressByteOffset"] = 16,
                ["generatedTypeinfoVptrs"] = storedVptrs
            });
        }
        Require(seen.Contains(ClassExport) && seen.Contains(MultipleInheritanceExport), "Missing admitted data export.");
        return rows;
    }

    private static void CheckStoredVptr(nint stored, nint symbolBase, JsonArray values)
    {
        Require(stored == symbolBase + 16, "Generated typeinfo did not apply the captured 16-byte offset exactly once.");
        values.Add((JsonNode?)JsonValue.Create(AddressText(stored)));
    }

    private static void VerifyMapping(BorrowedNativeRttiDataResolution resolution)
    {
        var symbol = (ulong)(nuint)resolution.SymbolBaseAddress;
        var moduleBase = (ulong)(nuint)resolution.ModuleLoadBaseAddress;
        var foundSymbol = false;
        var foundBase = false;
        foreach (var line in File.ReadLines("/proc/self/maps"))
        {
            var fields = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 6 || fields[5].TrimStart(' ') != resolution.CanonicalLibraryPath) continue;
            var bounds = fields[0].Split('-');
            Require(bounds.Length == 2, "Malformed provider mapping bounds.");
            var start = ulong.Parse(bounds[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            var end = ulong.Parse(bounds[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            var offset = ulong.Parse(fields[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            if (start == moduleBase && offset == 0) foundBase = true;
            if (start <= symbol && end >= checked(symbol + 88) && fields[1] == "r--p" &&
                start == (ulong)(nuint)resolution.MappingStartAddress && end == (ulong)(nuint)resolution.MappingEndAddress)
                foundSymbol = true;
        }
        Require(foundBase && foundSymbol, "Independent proc inspection did not confirm the exact provider owner and read-only symbol extent.");
    }

    private static int NativeInteger(JsonNode native, string array, int index) =>
        native["runs"]?[0]?[array]?[index]?.GetValue<int>() ?? throw new InvalidOperationException("Missing native method observation.");

    private static string AddressText(nint address) => "0x" + ((ulong)(nuint)address).ToString("x", CultureInfo.InvariantCulture);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
