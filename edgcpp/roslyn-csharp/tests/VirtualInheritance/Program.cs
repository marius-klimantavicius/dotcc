using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using RootRecord = Translated.Root__2;
using BranchRecord = Translated.Branch__2;
using FinalRecord = Translated.Final__2;
using RootView = Translated.Record__2;
using BranchView = Translated.Record__3;

internal static unsafe class Checks
{
    private const string ProviderHash = "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
    private const string ProviderPath = "/usr/lib/x86_64-linux-gnu/libstdc++.so.6.0.33";
    private const string ClassExport = "_ZTVN10__cxxabiv117__class_type_infoE";
    private const string VmiExport = "_ZTVN10__cxxabiv121__vmi_class_type_infoE";
    private const string SiExport = "_ZTVN10__cxxabiv120__si_class_type_infoE";
    private static long* RootTable => (long*)Application.storage__3;
    private static long* BranchTable => (long*)Application.storage__6;
    private static long** BranchVtt => (long**)Application.storage__7;
    private static long* FinalTable => (long*)Application.storage__10;
    private static long** FinalVtt => (long**)Application.storage__11;
    private static long* BranchConstructionTable => (long*)Application.storage__12;
    private static long* RootConstructionTable => (long*)Application.storage__13;

    [StructLayout(LayoutKind.Sequential)]
    private struct RootAlignmentProbe { public byte Prefix; public RootRecord Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct BranchAlignmentProbe { public byte Prefix; public BranchRecord Value; }
    [StructLayout(LayoutKind.Sequential)]
    private struct FinalAlignmentProbe { public byte Prefix; public FinalRecord Value; }

    public static void Main(string[] args)
    {
        if (args.Length != 1) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON");
        var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations.");
        var runs = new JsonArray();
        for (var run = 0; run < 2; ++run) runs.Add((JsonNode)RunFixture());
        Require(JsonNode.DeepEquals(runs[0], runs[1]), "Repeated fixture observations differ.");
        var observations = new JsonObject { ["runs"] = runs };
        Require(JsonNode.DeepEquals(observations, native), "Generated observations differ from the separate native oracle.");

        var layout = CheckLayout();
        var slots = CheckTables();
        Require(Application.lifetime_count() <= 30, "Insufficient finite event capacity for physical lifecycles.");
        var phases = new JsonArray();
        phases.Add((JsonNode)CheckCompleteBranch(native, slots));
        phases.Add((JsonNode)CheckConstructionPrefix(native, slots));
        phases.Add((JsonNode)CheckCompleteFinal(native, slots));
        Require(CheckTables().SequenceEqual(slots), "Canonical table entries changed during physical lifecycles.");
        Require(Application.lifetime_live() == 0 && Application.lifetime_overflow() == 0 && Application.lifetime_count() <= 64,
            "Physical lifecycles leave live units or invalid event storage.");
        observations["physical"] = new JsonObject
        {
            ["layout"] = layout, ["allFiveCanonicalEntriesDistinct"] = true,
            ["allFiveCanonicalEntriesExecuted"] = true, ["branchThunkExecutedWithBothDeltas"] = true,
            ["balancedGeneratedLifetimes"] = true, ["delegationD9Executed"] = false,
            ["tables"] = TableObservations(), ["phases"] = phases
        };
        observations["providers"] = CheckProviders();
        Console.WriteLine(observations.ToJsonString());
    }

    private static JsonObject RunFixture()
    {
        var result = Application.fixture();
        var count = Application.lifetime_count();
        var overflow = Application.lifetime_overflow();
        var live = Application.lifetime_live();
        Require(count is >= 0 and <= 64 && overflow == 0 && live == 0, "Fixture event/live bounds failed.");
        var values = new JsonArray();
        for (var index = 0; index < 9; ++index) values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
        Require(Application.lifetime_value(8) == 0, "Fixture entered with prior live units.");
        return new JsonObject { ["result"] = result, ["overflow"] = overflow, ["live"] = live,
            ["values"] = values, ["events"] = Events(0, count) };
    }

    private static JsonObject CheckLayout()
    {
        Require(sizeof(RootRecord) == 16 && sizeof(BranchRecord) == 32 && sizeof(FinalRecord) == 40 &&
            sizeof(RootView) == 12 && sizeof(BranchView) == 12, "Published complete/occupied record sizes differ.");
        RootAlignmentProbe rootProbe = new() { Prefix = 1, Value = default };
        BranchAlignmentProbe branchProbe = new() { Prefix = 1, Value = default };
        FinalAlignmentProbe finalProbe = new() { Prefix = 1, Value = default };
        Require((byte*)&rootProbe.Value - (byte*)&rootProbe == 8 &&
            (byte*)&branchProbe.Value - (byte*)&branchProbe == 8 &&
            (byte*)&finalProbe.Value - (byte*)&finalProbe == 8, "Complete record alignment is not 8.");
        RootRecord root = default;
        BranchRecord branch = default;
        FinalRecord final = default;
        Require((byte*)&root.__vptr - (byte*)&root == 0 && (byte*)&root.id - (byte*)&root == 8 &&
            (byte*)&branch.__vptr - (byte*)&branch == 0 && (byte*)&branch.branch_id - (byte*)&branch == 8 &&
            (byte*)&branch.__v_4Root - (byte*)&branch == 16 &&
            (byte*)&final.__b_6Branch - (byte*)&final == 0 && (byte*)&final.bonus - (byte*)&final == 16 &&
            (byte*)&final.__v_4Root - (byte*)&final == 24 && sizeof(long) == 8,
            "Published base, bonus or virtual Root offsets differ from capture.");
        // Final's 24-byte occupied prefix has no separate emitted SO carrier.
        // Its actual field boundary and bonus extent prove the bounded view.
        Require((byte*)&final.bonus + sizeof(long) == (byte*)&final.__v_4Root,
            "Final's occupied nonvirtual prefix does not end at byte 24.");
        return new JsonObject
        {
            ["rootSize"] = 16, ["branchSize"] = 32, ["finalSize"] = 40, ["completeAlignment"] = 8,
            ["rootViewSize"] = 12, ["branchViewSize"] = 12, ["finalOccupiedPrefixBytes"] = 24,
            ["finalOccupiedPrefixHasStandaloneCarrier"] = false,
            ["branchRootOffset"] = 16, ["finalBranchOffset"] = 0,
            ["finalBonusOffset"] = 16, ["finalRootOffset"] = 24
        };
    }

    private static nint[] CheckTables()
    {
        var root = RootTable;
        var branch = BranchTable;
        var final = FinalTable;
        var construction = BranchConstructionTable;
        var rootConstruction = RootConstructionTable;
        foreach (var pointer in new nint[] { (nint)root, (nint)branch, (nint)final, (nint)construction,
            (nint)rootConstruction, (nint)BranchVtt, (nint)FinalVtt })
            Require(pointer != 0 && ((nuint)pointer & 7) == 0, "Application table/VTT storage is not aligned.");
        Require(root[0] == 0 && root[1] == (long)(nint)Application.storage &&
            branch[0] == 16 && branch[1] == 0 && branch[2] == (long)(nint)Application.storage__4 &&
            branch[4] == -16 && branch[5] == -16 && branch[6] == branch[2] &&
            final[0] == 24 && final[1] == 0 && final[2] == (long)(nint)Application.storage__8 &&
            final[4] == -24 && final[5] == -24 && final[6] == final[2], "Ordinary signed table contents differ.");
        Require(construction[0] == 24 && construction[1] == 0 && construction[2] == branch[2] && construction[3] == branch[3] &&
            rootConstruction[0] == -24 && rootConstruction[1] == -24 && rootConstruction[2] == branch[2] && rootConstruction[3] == branch[7],
            "Construction tables did not retain their actual signed prefixes and canonical entries.");
        Require(BranchVtt[0] == branch + 3 && BranchVtt[1] == branch + 7 &&
            FinalVtt[0] == final + 3 && FinalVtt[1] == construction + 3 &&
            FinalVtt[2] == rootConstruction + 3 && FinalVtt[3] == final + 7,
            "Actual VTT pointers/address-point byte offsets differ from captured constants.");
        nint[] slots = [(nint)root[2], (nint)branch[3], (nint)branch[7], (nint)final[3], (nint)final[7]];
        Require(slots.All(address => address != 0) && slots.Distinct().Count() == 5, "Canonical method/thunk entries are missing or collapsed.");
        return slots;
    }

    private static JsonObject TableObservations()
    {
        static JsonArray Words(long* table, int count)
        {
            var values = new JsonArray();
            for (var index = 0; index < count; ++index) values.Add((JsonNode?)JsonValue.Create(table[index]));
            return values;
        }
        static JsonArray Addresses(long** table, int count)
        {
            var values = new JsonArray();
            for (var index = 0; index < count; ++index) values.Add((JsonNode?)JsonValue.Create(AddressText((nint)table[index])));
            return values;
        }
        return new JsonObject
        {
            ["root"] = Words(RootTable, 3), ["branch"] = Words(BranchTable, 8), ["final"] = Words(FinalTable, 8),
            ["branchInFinal"] = Words(BranchConstructionTable, 4), ["rootInBranchInFinal"] = Words(RootConstructionTable, 4),
            ["branchVtt"] = Addresses(BranchVtt, 2), ["finalVtt"] = Addresses(FinalVtt, 4)
        };
    }

    private static JsonObject CheckCompleteBranch(JsonNode native, nint[] slots)
    {
        BranchRecord value = default;
        var owner = &value;
        var root = (RootRecord*)&value.__v_4Root;
        Require(((nuint)owner & 7) == 0 && (byte*)root - (byte*)owner == 16, "Complete Branch owner/Root view is incorrect.");
        var start = Application.lifetime_count();
        var constructed = false;
        JsonObject? calls = null;
        try
        {
            Application.function__7(owner, 5, 7); // Actual complete Branch C1.
            constructed = true;
            Require(Application.lifetime_live() == 2 && root->id == 5 && owner->branch_id == 7 &&
                owner->__vptr == BranchVtt[0] && root->__vptr == BranchVtt[1] &&
                owner->__vptr[-3] == 16 && root->__vptr[-3] == -16 && root->__vptr[-2] == -16 &&
                Application.lifetime_value(0) == NativeInteger(native, "values", 0), "Complete Branch construction/VTT phase differs.");
            var callStart = Application.lifetime_count();
            var direct = (delegate* unmanaged[Cdecl]<BranchRecord*, int, int>)(nint)owner->__vptr[0];
            var thunk = (delegate* unmanaged[Cdecl]<RootRecord*, int, int>)(nint)root->__vptr[0];
            Require((nint)owner->__vptr[0] == slots[1] && (nint)root->__vptr[0] == slots[2], "Complete Branch loaded entries differ.");
            var directResult = direct(owner, 3);
            var thunkResult = thunk(root, 3);
            Require(directResult == NativeInteger(native, "values", 0) && thunkResult == directResult &&
                Application.lifetime_count() == callStart + 2 &&
                Application.lifetime_event(callStart) == NativeInteger(native, "events", 3) &&
                Application.lifetime_event(callStart + 1) == NativeInteger(native, "events", 3), "Complete Branch UCO results/events differ.");
            calls = new JsonObject { ["directAddress"] = AddressText(slots[1]), ["thunkAddress"] = AddressText(slots[2]),
                ["directResult"] = directResult, ["thunkResult"] = thunkResult, ["rootReceiver"] = AddressText((nint)root),
                ["vcallDelta"] = root->__vptr[-3], ["events"] = Events(callStart, 2) };
        }
        finally
        {
            if (constructed) Application.function__9(owner); // Actual complete Branch D1.
        }
        Require(Application.lifetime_live() == 0 && Application.lifetime_value(3) == NativeInteger(native, "values", 3),
            "Complete Branch D1 did not balance Root/Branch and the native destructor observation.");
        return new JsonObject { ["phase"] = "complete-branch", ["constructor"] = "Branch C1/function__7",
            ["destructor"] = "Branch D1/function__9", ["rootOffset"] = 16,
            ["vttArgument"] = AddressText((nint)BranchVtt), ["calls"] = calls,
            ["finalLive"] = 0, ["events"] = Events(start, Application.lifetime_count() - start) };
    }

    private static JsonObject CheckConstructionPrefix(JsonNode native, nint[] slots)
    {
        FinalRecord storage = default;
        var owner = &storage;
        var root = (RootRecord*)&storage.__v_4Root;
        var branch = (BranchRecord*)owner;
        var vtt = FinalVtt + 1;
        Require(((nuint)owner & 7) == 0 && (byte*)root - (byte*)owner == 24, "Prefix-only owner/Root view is incorrect.");
        var start = Application.lifetime_count();
        var rootConstructed = false;
        var branchConstructed = false;
        var afterBranchLive = -1;
        var rootResult = 0;
        var thunkResult = 0;
        var rootCallStart = 0;
        var branchCallStart = 0;
        long primaryDelta = 0, rootDelta = 0, rootOffsetToTop = 0;
        try
        {
            Application.function__6(root, 5); // Retained Root C2 forwarding to Root C1.
            rootConstructed = true;
            Require(Application.lifetime_live() == 1 && root->id == 5 && root->__vptr == RootTable + 2 &&
                (nint)root->__vptr[0] == slots[0], "Actual prefix Root initialization is incorrect.");
            rootCallStart = Application.lifetime_count();
            var rootCall = (delegate* unmanaged[Cdecl]<RootRecord*, int, int>)(nint)root->__vptr[0];
            rootResult = rootCall(root, 3);
            // Native fixture observes Root id, but does not invoke Root's own
            // virtual body. This is its separate captured scalar/ABI check.
            Require(rootResult == NativeInteger(native, "values", 4) + 3 && Application.lifetime_count() == rootCallStart + 1 &&
                Application.lifetime_event(rootCallStart) == 700 + NativeInteger(native, "values", 4), "Root UCO scalar contract differs.");

            Application.Branch(branch, vtt, 5, 7); // Actual selected Branch C2 and Final VTT slice.
            branchConstructed = true;
            Require(Application.lifetime_live() == 2 && branch->branch_id == 7 && root->id == 5 &&
                branch->__vptr == BranchConstructionTable + 3 && root->__vptr == RootConstructionTable + 3 &&
                branch->__vptr == vtt[0] && root->__vptr == vtt[1] &&
                branch->__vptr[-3] == 24 && root->__vptr[-3] == -24 && root->__vptr[-2] == -24 &&
                Application.lifetime_value(0) == NativeInteger(native, "values", 0), "Retained prefix construction did not select actual phase tables.");
            Require((nint)root->__vptr[0] == slots[2], "Prefix phase does not use the same canonical Branch thunk.");
            primaryDelta = branch->__vptr[-3];
            rootDelta = root->__vptr[-3];
            rootOffsetToTop = root->__vptr[-2];
            branchCallStart = Application.lifetime_count();
            var branchThunk = (delegate* unmanaged[Cdecl]<RootRecord*, int, int>)(nint)root->__vptr[0];
            // Root points at owner +24. Only the thunk body applies the -24
            // value loaded from its active construction vtable.
            thunkResult = branchThunk(root, 3);
            Require(thunkResult == NativeInteger(native, "values", 0) && Application.lifetime_count() == branchCallStart + 1 &&
                Application.lifetime_event(branchCallStart) == NativeInteger(native, "events", 3), "Construction-phase Branch thunk differs.");
        }
        finally
        {
            try
            {
                if (branchConstructed) Application._Branch(branch, vtt); // Subobject D2 only.
                afterBranchLive = Application.lifetime_live();
            }
            finally
            {
                if (rootConstructed) Application._Root(root); // Complete Root D1, exactly once.
            }
        }
        Require(afterBranchLive == 1 && Application.lifetime_live() == 0 &&
            Application.lifetime_value(3) == NativeInteger(native, "values", 3), "Prefix-only destruction did not release exactly Branch then Root.");
        return new JsonObject
        {
            ["phase"] = "prefix-only-in-final-storage", ["rootConstructor"] = "Root C2/function__6",
            ["branchConstructor"] = "Branch C2/Branch", ["branchDestructor"] = "Branch D2/_Branch",
            ["rootDestructor"] = "Root D1/_Root", ["finalConstructorOrDestructorCalled"] = false,
            ["branchReceiver"] = AddressText((nint)branch), ["rootReceiver"] = AddressText((nint)root),
            ["rootOffset"] = 24, ["vttArgument"] = AddressText((nint)vtt), ["vttElementOffset"] = 1,
            ["primaryVbaseDelta"] = primaryDelta, ["rootVcallDelta"] = rootDelta, ["rootOffsetToTop"] = rootOffsetToTop,
            ["rootMethodAddress"] = AddressText(slots[0]), ["rootMethodResult"] = rootResult,
            ["rootMethodHasNativeFixtureInvocation"] = false, ["rootMethodEvent"] = Application.lifetime_event(rootCallStart),
            ["branchThunkAddress"] = AddressText(slots[2]), ["branchThunkResult"] = thunkResult,
            ["branchThunkEvent"] = Application.lifetime_event(branchCallStart),
            ["afterBranchDestructorLive"] = afterBranchLive, ["finalLive"] = 0,
            ["events"] = Events(start, Application.lifetime_count() - start)
        };
    }

    private static JsonObject CheckCompleteFinal(JsonNode native, nint[] slots)
    {
        FinalRecord value = default;
        var owner = &value;
        var root = (RootRecord*)&value.__v_4Root;
        Require(((nuint)owner & 7) == 0 && (byte*)root - (byte*)owner == 24, "Complete Final owner/Root view is incorrect.");
        var start = Application.lifetime_count();
        var constructed = false;
        JsonObject? calls = null;
        try
        {
            Application.function__5(owner, 5, 7, 11); // Actual complete Final C1.
            constructed = true;
            Require(Application.lifetime_live() == 3 && root->id == 5 && owner->__b_6Branch.branch_id == 7 && owner->bonus == 11 &&
                owner->__b_6Branch.__vptr == FinalVtt[0] && root->__vptr == FinalVtt[3] &&
                owner->__b_6Branch.__vptr[-3] == 24 && root->__vptr[-3] == -24 &&
                Application.lifetime_value(0) == NativeInteger(native, "values", 0), "Complete Final C1 did not preserve its actual phase transition.");
            Require((nint)owner->__b_6Branch.__vptr[0] == slots[3] && (nint)root->__vptr[0] == slots[4], "Complete Final loaded entries differ.");
            var callStart = Application.lifetime_count();
            var direct = (delegate* unmanaged[Cdecl]<FinalRecord*, int, int>)(nint)owner->__b_6Branch.__vptr[0];
            var thunk = (delegate* unmanaged[Cdecl]<RootRecord*, int, int>)(nint)root->__vptr[0];
            var directResult = direct(owner, 13);
            var thunkResult = thunk(root, 13);
            Require(directResult == NativeInteger(native, "values", 1) && thunkResult == directResult &&
                Application.lifetime_count() == callStart + 2 &&
                Application.lifetime_event(callStart) == NativeInteger(native, "events", 6) &&
                Application.lifetime_event(callStart + 1) == NativeInteger(native, "events", 6), "Final direct/thunk UCO results/events differ.");
            calls = new JsonObject { ["directAddress"] = AddressText(slots[3]), ["thunkAddress"] = AddressText(slots[4]),
                ["directResult"] = directResult, ["thunkResult"] = thunkResult, ["rootReceiver"] = AddressText((nint)root),
                ["vcallDelta"] = root->__vptr[-3], ["events"] = Events(callStart, 2) };
        }
        finally
        {
            if (constructed) Application.function__3(owner); // Actual complete Final D1.
        }
        Require(Application.lifetime_live() == 0 && Application.lifetime_value(2) == NativeInteger(native, "values", 2) &&
            Application.lifetime_value(3) == NativeInteger(native, "values", 3), "Final D1 did not preserve native destructor phase observations.");
        return new JsonObject { ["phase"] = "complete-final", ["constructor"] = "Final C1/function__5",
            ["destructor"] = "Final D1/function__3", ["rootOffset"] = 24,
            ["vttArgument"] = AddressText((nint)FinalVtt), ["calls"] = calls,
            ["finalDestructorDispatch"] = Application.lifetime_value(2), ["branchDestructorDispatch"] = Application.lifetime_value(3),
            ["finalLive"] = 0, ["events"] = Events(start, Application.lifetime_count() - start) };
    }

    private static JsonArray CheckProviders()
    {
        var resolutions = BorrowedNativeRttiData.GetDiagnostics();
        Require(resolutions.Count == 3, "Expected exactly the three existing class/VMI/SI data descriptors.");
        Require(sizeof(Translated.__class_type_info) == 16 && sizeof(Translated.__vmi_class_type_info) == 40 &&
            sizeof(Translated.__si_class_type_info) == 24 && ((nuint)Application.storage & 7) == 0 &&
            ((nuint)Application.storage__4 & 7) == 0 && ((nuint)Application.storage__8 & 7) == 0,
            "Generated RTTI storage size/alignment differs from capture.");
        var baseInfo = (Translated.Record*)&Application.storage__4->base_info;
        Require(Application.storage__4->flags == 0 && Application.storage__4->base_count == 1 &&
            baseInfo->tinfo == Application.storage && baseInfo->offset_flags == -6141 &&
            Application.storage__8->base_type == (Translated.__class_type_info*)Application.storage__4,
            "Generated RTTI raw constants or base-typeinfo identities differ.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var rows = new JsonArray();
        foreach (var row in resolutions)
        {
            Require(seen.Add(row.ExportName), "Repeated provider diagnostic.");
            var rva = row.ExportName switch
            {
                ClassExport => 0x26dd18UL, VmiExport => 0x26ec98UL, SiExport => 0x26ebd8UL,
                _ => throw new InvalidOperationException("Unexpected native data export.")
            };
            Require(row.CanonicalLibraryPath == ProviderPath && row.LibrarySha256 == ProviderHash &&
                row.PinnedProviderElfSymbolVersion == "CXXABI_1.3" && row.ExportRelativeVirtualAddress == rva && row.SymbolSizeBytes == 88 &&
                row.LoadedOwnerVerified && row.ReadOnlyMappingVerified && row.MappingPermissions == "r--p" &&
                row.SymbolBaseAddress > 0 && row.ModuleLoadBaseAddress > 0 &&
                (ulong)(nuint)row.SymbolBaseAddress == checked((ulong)(nuint)row.ModuleLoadBaseAddress + rva) &&
                row.MappingStartAddress <= row.SymbolBaseAddress && checked((ulong)(nuint)row.SymbolBaseAddress + 88) <= (ulong)(nuint)row.MappingEndAddress,
                "Provider diagnostics differ from the exact existing descriptor and owner mapping.");
            using (var file = File.OpenRead(row.CanonicalLibraryPath))
                Require(Convert.ToHexStringLower(SHA256.HashData(file)) == ProviderHash, "Provider file hash changed.");
            var handle = NativeLibrary.Load(row.CanonicalLibraryPath); // Held until process exit.
            Require(NativeLibrary.GetExport(handle, row.ExportName) == row.SymbolBaseAddress, "Independent GetExport identity differs.");
            VerifyMapping(row);
            var stored = row.ExportName switch
            {
                ClassExport => (nint)Application.storage->@base.__vptr,
                VmiExport => (nint)Application.storage__4->@base.@base.__vptr,
                SiExport => (nint)Application.storage__8->@base.@base.__vptr,
                _ => throw new InvalidOperationException("Unexpected generated RTTI root.")
            };
            Require(stored == row.SymbolBaseAddress + 16, "Captured RTTI address offset was not applied exactly once.");
            rows.Add((JsonNode)new JsonObject
            {
                ["canonicalLibraryPath"] = row.CanonicalLibraryPath, ["librarySha256"] = row.LibrarySha256,
                ["exportName"] = row.ExportName, ["pinnedProviderElfSymbolVersion"] = row.PinnedProviderElfSymbolVersion,
                ["symbolBaseAddress"] = AddressText(row.SymbolBaseAddress), ["moduleLoadBaseAddress"] = AddressText(row.ModuleLoadBaseAddress),
                ["exportRelativeVirtualAddress"] = rva, ["providerElfSymbolSizeBytes"] = row.SymbolSizeBytes,
                ["mappingStartAddress"] = AddressText(row.MappingStartAddress), ["mappingEndAddress"] = AddressText(row.MappingEndAddress),
                ["mappingPermissions"] = row.MappingPermissions, ["loadedOwnerVerified"] = true, ["readOnlyMappingVerified"] = true,
                ["independentExportMatch"] = true, ["independentMappingMatch"] = true,
                ["capturedAddressByteOffset"] = 16, ["generatedTypeinfoVptr"] = AddressText(stored)
            });
        }
        Require(seen.Contains(ClassExport) && seen.Contains(VmiExport) && seen.Contains(SiExport), "Missing selected provider descriptor.");
        return rows;
    }

    private static void VerifyMapping(BorrowedNativeRttiDataResolution resolution)
    {
        var symbol = (ulong)(nuint)resolution.SymbolBaseAddress;
        var module = (ulong)(nuint)resolution.ModuleLoadBaseAddress;
        var foundSymbol = false;
        var foundBase = false;
        foreach (var line in File.ReadLines("/proc/self/maps"))
        {
            var fields = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 6 || fields[5].TrimStart(' ') != resolution.CanonicalLibraryPath) continue;
            var bounds = fields[0].Split('-');
            Require(bounds.Length == 2, "Malformed provider mapping.");
            var start = ulong.Parse(bounds[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            var end = ulong.Parse(bounds[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            var offset = ulong.Parse(fields[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            if (start == module && offset == 0) foundBase = true;
            if (start <= symbol && end >= checked(symbol + 88) && fields[1] == "r--p" &&
                start == (ulong)(nuint)resolution.MappingStartAddress && end == (ulong)(nuint)resolution.MappingEndAddress)
                foundSymbol = true;
        }
        Require(foundBase && foundSymbol, "Independent mapping inspection did not confirm exact provider owner/extent.");
    }

    private static JsonArray Events(int start, int count)
    {
        Require(start >= 0 && count >= 0 && start + count <= Application.lifetime_count() && start + count <= 64, "Invalid finite event slice.");
        var values = new JsonArray();
        for (var index = 0; index < count; ++index) values.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(start + index)));
        return values;
    }

    private static int NativeInteger(JsonNode native, string array, int index) =>
        native["runs"]?[0]?[array]?[index]?.GetValue<int>() ?? throw new InvalidOperationException("Missing native observation.");
    private static string AddressText(nint address) => "0x" + ((ulong)(nuint)address).ToString("x", CultureInfo.InvariantCulture);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
