using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using BaseRecord = Translated.Base__2;
using DerivedRecord = Translated.Derived__2;
using BorrowedBaseView = Translated.Record;

internal static unsafe class Checks
{
    private const string ProviderHash = "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
    private const string ProviderPath = "/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33";
    private const string ClassExport = "_ZTVN10__cxxabiv117__class_type_infoE";
    private const string SingleInheritanceExport = "_ZTVN10__cxxabiv120__si_class_type_infoE";

    [StructLayout(LayoutKind.Sequential)]
    private struct BaseAlignmentProbe { public byte Prefix; public BaseRecord Value; }

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

        var physical = CheckPhysicalExecution(native);
        var providers = CheckProviders();
        Require(Application.lifetime_live() == 0 && Application.lifetime_overflow() == 0,
            "Physical checks leave live objects or event overflow.");
        observations["physical"] = physical;
        observations["providers"] = providers;
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
        Require(sizeof(BaseRecord) == 16 && sizeof(DerivedRecord) == 16 && sizeof(BorrowedBaseView) == 12,
            "Complete records or borrowed occupied-base view have incorrect sizes.");
        BaseAlignmentProbe baseAlignment = new() { Prefix = 1, Value = default };
        DerivedAlignmentProbe derivedAlignment = new() { Prefix = 1, Value = default };
        Require((byte*)&baseAlignment.Value - (byte*)&baseAlignment == 8 &&
            (byte*)&derivedAlignment.Value - (byte*)&derivedAlignment == 8,
            "Complete record alignment is not 8 in a sequential containing record.");

        BaseRecord* bases = stackalloc BaseRecord[2];
        DerivedRecord* derived = stackalloc DerivedRecord[2];
        Require(((nuint)bases & 7) == 0 && ((nuint)derived & 7) == 0 &&
            (byte*)(bases + 1) - (byte*)bases == 16 && (byte*)(derived + 1) - (byte*)derived == 16,
            "Complete object owners have incorrect observed alignment or stride.");
        Require((byte*)&bases[0].__vptr - (byte*)&bases[0] == 0 &&
            (byte*)&bases[0].id - (byte*)&bases[0] == 8 &&
            (byte*)&derived[0].__layout_alignment - (byte*)&derived[0] == 0 &&
            (byte*)&derived[0].__b_4Base - (byte*)&derived[0] == 0 &&
            (byte*)&derived[0].__b_4Base.__vptr - (byte*)&derived[0] == 0 &&
            (byte*)&derived[0].__b_4Base.id - (byte*)&derived[0] == 8 &&
            (byte*)&derived[0].bonus - (byte*)&derived[0] == 12,
            "Physical field offsets do not match the captured layout.");

        var baseTable = (long*)Application.storage__3;
        var derivedTable = (long*)Application.storage__6;
        Require(baseTable != null && derivedTable != null && baseTable != derivedTable &&
            ((nuint)baseTable & 7) == 0 && ((nuint)derivedTable & 7) == 0,
            "Generated application tables are not distinct aligned storage.");
        Require(baseTable[0] == 0 && derivedTable[0] == 0 &&
            baseTable[1] == (long)(nint)Application.storage &&
            derivedTable[1] == (long)(nint)Application.storage__4,
            "Generated vtable headers do not retain their generated typeinfo identities.");
        var baseSlot = (nint)baseTable[2];
        var derivedSlot = (nint)derivedTable[2];
        Require(baseSlot != 0 && derivedSlot != 0 && baseSlot != derivedSlot,
            "Generated method slots are missing or collapsed.");

        var before = Application.lifetime_count();
        Require(before is >= 0 and <= 48 && Application.lifetime_live() == 0,
            "Physical execution must begin with bounded observations and no live objects.");
        var constructedBases = 0;
        var constructedDerived = 0;
        var baseResult = 0;
        var derivedResult = 0;
        var callStart = 0;
        try
        {
            for (; constructedBases < 2; ++constructedBases) Application.Base(bases + constructedBases, 2);
            for (; constructedDerived < 2; ++constructedDerived) Application.Derived(derived + constructedDerived, 5, 11);
            Require(Application.lifetime_live() == 6, "Actual constructors did not establish the expected complete lifetime units.");
            for (var index = 0; index < 2; ++index)
            {
                Require(bases[index].id == 2 && bases[index].__vptr == baseTable + 2,
                    "Complete base object fields or canonical address point are incorrect.");
                Require(derived[index].__b_4Base.id == 5 && derived[index].bonus == 11 &&
                    derived[index].__b_4Base.__vptr == derivedTable + 2 &&
                    derived[index].__layout_alignment == (nint)(derivedTable + 2),
                    "Derived base view, tail bonus or canonical address point is incorrect.");
            }
            Require((nint)bases[0].__vptr[0] == baseSlot && (nint)bases[1].__vptr[0] == baseSlot &&
                (nint)derived[0].__b_4Base.__vptr[0] == derivedSlot &&
                (nint)derived[1].__b_4Base.__vptr[0] == derivedSlot,
                "The actual loaded method address is not stable across complete receivers.");

            callStart = Application.lifetime_count();
            var baseCall = (delegate* unmanaged[Cdecl]<BaseRecord*, int, int>)(nint)bases[0].__vptr[0];
            var derivedCall = (delegate* unmanaged[Cdecl]<DerivedRecord*, int, int>)(nint)derived[0].__b_4Base.__vptr[0];
            baseResult = baseCall(bases, 3);
            derivedResult = derivedCall(derived, 7);
            Require(baseResult == NativeInteger(native, "values", 4) && derivedResult == NativeInteger(native, "values", 5),
                "Direct execution of the two canonical UCO slots differs from native method observations.");
            Require(Application.lifetime_count() == callStart + 2 &&
                Application.lifetime_event(callStart) == NativeInteger(native, "events", 4) &&
                Application.lifetime_event(callStart + 1) == NativeInteger(native, "events", 6) &&
                Application.lifetime_live() == 6,
                "Direct UCO calls did not execute exactly the selected method bodies and preserve their receivers.");
        }
        finally
        {
            while (constructedDerived > 0) Application._Derived(derived + --constructedDerived);
            while (constructedBases > 0) Application._Base(bases + --constructedBases);
        }
        Require(Application.lifetime_live() == 0 && Application.lifetime_overflow() == 0,
            "Actual generated destructors did not balance the physical constructor lifetimes.");
        Require(baseTable[2] == (long)baseSlot && derivedTable[2] == (long)derivedSlot,
            "Canonical application slot addresses changed during actual lifetimes.");

        var effects = new JsonArray();
        for (var index = before; index < Application.lifetime_count(); ++index)
            effects.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
        return new JsonObject
        {
            ["baseSize"] = sizeof(BaseRecord), ["derivedSize"] = sizeof(DerivedRecord),
            ["borrowedBaseViewSize"] = sizeof(BorrowedBaseView), ["completeAlignment"] = 8,
            ["baseViewOffset"] = 0, ["baseIdOffset"] = 8, ["derivedBonusOffset"] = 12,
            ["completeOwnersAligned"] = true, ["canonicalSlotsStable"] = true,
            ["baseSlot"] = AddressText(baseSlot), ["derivedSlot"] = AddressText(derivedSlot),
            ["baseSlotResult"] = baseResult, ["derivedSlotResult"] = derivedResult,
            ["baseBodyEvent"] = Application.lifetime_event(callStart),
            ["derivedBodyEvent"] = Application.lifetime_event(callStart + 1),
            ["balancedGeneratedLifetimes"] = true, ["events"] = effects
        };
    }

    private static JsonArray CheckProviders()
    {
        var resolutions = BorrowedNativeRttiData.GetDiagnostics();
        Require(resolutions.Count == 2, "Expected exactly two successfully resolved borrowed RTTI data exports.");
        Require(sizeof(Translated.__class_type_info) == 16 && sizeof(Translated.__si_class_type_info) == 24 &&
            ((nuint)Application.storage & 7) == 0 && ((nuint)Application.storage__4 & 7) == 0 &&
            Application.storage__4->base_type == Application.storage,
            "Generated typeinfo storage or derived-to-base typeinfo identity is incorrect.");
        var rows = new JsonArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in resolutions)
        {
            Require(seen.Add(row.ExportName), "Repeated provider export diagnostic.");
            var expectedRva = row.ExportName switch
            {
                ClassExport => 0x26dd18UL,
                SingleInheritanceExport => 0x26ebd8UL,
                _ => throw new InvalidOperationException("Unexpected native data export.")
            };
            Require(row.CanonicalLibraryPath == ProviderPath && row.LibrarySha256 == ProviderHash &&
                row.PinnedProviderElfSymbolVersion == "CXXABI_1.3" && row.ExportRelativeVirtualAddress == expectedRva &&
                row.SymbolSizeBytes == 88 && row.LoadedOwnerVerified && row.ReadOnlyMappingVerified &&
                row.MappingPermissions == "r--p" && row.SymbolBaseAddress > 0 && row.ModuleLoadBaseAddress > 0 &&
                (ulong)(nuint)row.SymbolBaseAddress == checked((ulong)(nuint)row.ModuleLoadBaseAddress + expectedRva) &&
                row.MappingStartAddress <= row.SymbolBaseAddress &&
                checked((ulong)(nuint)row.SymbolBaseAddress + 88) <= (ulong)(nuint)row.MappingEndAddress,
                "Borrowed provider diagnostics do not match the exact audited descriptor and owner mapping.");
            using (var file = File.OpenRead(row.CanonicalLibraryPath))
                Require(Convert.ToHexStringLower(SHA256.HashData(file)) == ProviderHash, "Physical provider file hash changed.");

            // This independent loader reference remains held until process
            // exit, just like the runtime's original owner reference.
            var handle = NativeLibrary.Load(row.CanonicalLibraryPath);
            Require(NativeLibrary.GetExport(handle, row.ExportName) == row.SymbolBaseAddress,
                "An independent exact-provider GetExport does not identify the reported symbol base.");
            VerifyMapping(row);
            var storedVptr = row.ExportName == ClassExport
                ? (nint)Application.storage->@base.__vptr
                : (nint)Application.storage__4->@base.@base.__vptr;
            Require(storedVptr == row.SymbolBaseAddress + 16,
                "Generated typeinfo did not store the actual borrowed export base plus the captured 16-byte offset exactly once.");
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
                ["independentMappingMatch"] = true, ["generatedTypeinfoVptr"] = AddressText(storedVptr)
            });
        }
        Require(seen.Contains(ClassExport) && seen.Contains(SingleInheritanceExport), "Missing admitted native data export.");
        return rows;
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
        Require(foundBase && foundSymbol, "Independent proc mapping inspection did not confirm the exact provider owner and read-only symbol range.");
    }

    private static int NativeInteger(JsonNode native, string array, int index) =>
        native["runs"]?[0]?[array]?[index]?.GetValue<int>() ?? throw new InvalidOperationException("Missing native method observation.");

    private static string AddressText(nint address) => "0x" + ((ulong)(nuint)address).ToString("x", CultureInfo.InvariantCulture);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
