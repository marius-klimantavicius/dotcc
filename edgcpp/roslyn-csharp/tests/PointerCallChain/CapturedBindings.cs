using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Application = Translated.Program;
using InputView = Translated.CellInput_BorrowedReadView;
using Owner = Translated.Owner;
using Proxy = Translated.PointerProxy;
using Derived = Translated.DerivedStorage;
using Storage = Translated.PointerStorage;
using Access = Translated.PointerAccess;
using RefCount = Translated.RefCountCarrier;

internal static unsafe partial class CapturedBindings
{
    internal static partial int Fixture(void* first, void* second) => Application.fixture((InputView*)first, (InputView*)second);
    internal static partial int Count() => Application.lifetime_count();
    internal static partial int Overflow() => Application.lifetime_overflow();
    internal static partial int Live() => Application.lifetime_live();
    internal static partial int Event(int index) => Application.lifetime_event(index);
    internal static partial int Value(int index) => Application.lifetime_value(index);
    internal static partial int ReadKind(void* input) => Application.read((InputView*)input);

    [StructLayout(LayoutKind.Sequential)] private struct OwnerPrefix { public byte Prefix; public Owner Value; }
    [StructLayout(LayoutKind.Sequential)] private struct ProxyPrefix { public byte Prefix; public Proxy Value; }
    [StructLayout(LayoutKind.Sequential)] private struct DerivedPrefix { public byte Prefix; public Derived Value; }
    [StructLayout(LayoutKind.Sequential)] private struct StoragePrefix { public byte Prefix; public Storage Value; }
    [StructLayout(LayoutKind.Sequential)] private struct RefCountPrefix { public byte Prefix; public RefCount Value; }
    [StructLayout(LayoutKind.Sequential)] private struct InputViewPrefix { public byte Prefix; public InputView Value; }
    [StructLayout(LayoutKind.Sequential)] private struct AccessPrefix { public byte Prefix; public Access Value; }

    internal static partial JsonObject PhysicalHooks(void* first, void* second)
    {
        var inputFirst = (InputView*)first;
        var inputSecond = (InputView*)second;
        Owner* owners = stackalloc Owner[2];
        Proxy* proxies = stackalloc Proxy[2];
        Derived* derived = stackalloc Derived[2];
        Storage* storage = stackalloc Storage[2];
        RefCount* counts = stackalloc RefCount[2];
        InputView* views = stackalloc InputView[2];
        Access* accesses = stackalloc Access[2];
        new Span<Owner>(owners, 2).Clear();
        OwnerPrefix ownerPrefix = default;
        ProxyPrefix proxyPrefix = default;
        DerivedPrefix derivedPrefix = default;
        StoragePrefix storagePrefix = default;
        RefCountPrefix countPrefix = default;
        InputViewPrefix viewPrefix = default;
        AccessPrefix accessPrefix = default;

        var sizes = Integers(sizeof(Owner), sizeof(Proxy), sizeof(Derived), sizeof(Storage), sizeof(RefCount), sizeof(InputView), sizeof(Access));
        CheckSequence(sizes, [32, 24, 16, 16, 8, 56, 1], "Actual complete CLR carrier sizes differ.");
        var prefixes = Integers(
            (int)((byte*)&ownerPrefix.Value - (byte*)&ownerPrefix), (int)((byte*)&proxyPrefix.Value - (byte*)&proxyPrefix),
            (int)((byte*)&derivedPrefix.Value - (byte*)&derivedPrefix), (int)((byte*)&storagePrefix.Value - (byte*)&storagePrefix),
            (int)((byte*)&countPrefix.Value - (byte*)&countPrefix), (int)((byte*)&viewPrefix.Value - (byte*)&viewPrefix),
            (int)((byte*)&accessPrefix.Value - (byte*)&accessPrefix));
        CheckSequence(prefixes, [8, 8, 8, 8, 8, 8, 1], "Actual sequential-prefix alignments differ.");
        var strides = Integers(
            (int)((byte*)&owners[1] - (byte*)&owners[0]), (int)((byte*)&proxies[1] - (byte*)&proxies[0]),
            (int)((byte*)&derived[1] - (byte*)&derived[0]), (int)((byte*)&storage[1] - (byte*)&storage[0]),
            (int)((byte*)&counts[1] - (byte*)&counts[0]), (int)((byte*)&views[1] - (byte*)&views[0]),
            (int)((byte*)&accesses[1] - (byte*)&accesses[0]));
        CheckSequence(strides, [32, 24, 16, 16, 8, 56, 1], "Actual typed array strides differ.");
        var offsets = Integers(
            (int)((byte*)&owners[0].canary - (byte*)&owners[0]), (int)((byte*)&owners[0].payload - (byte*)&owners[0]),
            (int)((byte*)&proxies[0].@internal - (byte*)&proxies[0]), (int)((byte*)&proxies[0].use_direct - (byte*)&proxies[0]),
            (int)((byte*)&derived[0].__b_14PointerStorage - (byte*)&derived[0]),
            (int)((byte*)&storage[0].pointer - (byte*)&storage[0]), (int)((byte*)&storage[0].refcount - (byte*)&storage[0]),
            (int)((byte*)&counts[0].cookie - (byte*)&counts[0]),
            (int)((byte*)&views[0].__borrowed_alignment - (byte*)&views[0]), (int)((byte*)&views[0].kind - (byte*)&views[0]));
        CheckSequence(offsets, [0, 8, 0, 16, 0, 0, 8, 0, 0, 24], "Actual field and generated-base offsets differ.");
        var aligned = Aligned8(owners) && Aligned8(&owners[1]) && Aligned8(proxies) && Aligned8(derived) &&
            Aligned8(storage) && Aligned8(counts) && Aligned8(views) && Aligned8(inputFirst) && Aligned8(inputSecond);
        Check(aligned, "An actual carrier or raw input address violates its measured natural alignment.");

        owners[0].canary = -0x6123456789ABCDE0L;
        owners[1].canary = 0x5123456789ABCDE0L;
        owners[0].payload.use_direct = 1;
        owners[1].payload.use_direct = 0;
        owners[0].payload.@internal.__b_14PointerStorage.refcount.cookie = inputSecond;
        owners[1].payload.@internal.__b_14PointerStorage.refcount.cookie = inputFirst;
        owners[1].payload.@internal.__b_14PointerStorage.pointer = inputSecond;
        var borrowedFirst = Application.view(&owners[0]);
        var borrowedSecond = Application.view(&owners[1]);
        var firstDelta = (int)((byte*)borrowedFirst - (byte*)&owners[0]);
        var secondDelta = (int)((byte*)borrowedSecond - (byte*)&owners[1]);
        var borrowedIdentities = borrowedFirst == &owners[0].payload && borrowedSecond == &owners[1].payload &&
            borrowedFirst != borrowedSecond && firstDelta == 8 && secondDelta == 8;
        Check(borrowedIdentities, "Actual getter results do not alias their two distinct owners' payload storage.");
        var firstStorage = &borrowedFirst->@internal.__b_14PointerStorage;
        var secondStorage = &borrowedSecond->@internal.__b_14PointerStorage;
        var baseViews = (byte*)firstStorage == (byte*)&borrowedFirst->@internal &&
            (byte*)secondStorage == (byte*)&borrowedSecond->@internal;
        Check(baseViews, "Actual nested generated-base storage does not share the captured zero-offset view.");

        var expectBefore = *Application.expect_count;
        var guardBefore = *Application.guard_count;
        var missingBefore = *Application.missing_count;
        var directResults = new JsonArray();
        var baseResults = new JsonArray();
        var getResults = new JsonArray();
        var delegatedResults = new JsonArray();
        var reverseResults = new JsonArray();
        var proxyTruth = new JsonArray();
        var storageTruth = new JsonArray();
        var loadedKind = new JsonArray();
        for (var phase = 0; phase < 4; ++phase)
        {
            InputView* expected = phase == 1 ? inputFirst : phase == 2 ? inputSecond : null;
            firstStorage->pointer = expected;
            Add(getResults, Identify(Application.get(firstStorage), inputFirst, inputSecond));
            Add(delegatedResults, Identify(Application.operator____2((Access*)firstStorage), inputFirst, inputSecond));
            Add(reverseResults, Identify(Application.base_pointer((Access*)firstStorage), inputFirst, inputSecond));
            borrowedFirst->use_direct = 1;
            var throughDirect = Application.operator__(borrowedFirst);
            Check(throughDirect == expected, "Direct getter/local/return path returned stale or foreign pointer bits.");
            Add(directResults, Identify(throughDirect, inputFirst, inputSecond));
            borrowedFirst->use_direct = 0;
            var throughBase = Application.operator__(borrowedFirst);
            Check(throughBase == expected, "Empty-base delegation returned stale or foreign pointer bits.");
            Add(baseResults, Identify(throughBase, inputFirst, inputSecond));
            var truth = expected == null ? 0 : 1;
            var proxyValue = Application.operator_bool(borrowedFirst);
            var storageValue = Application.operator_bool__2(firstStorage);
            Check(proxyValue == truth && storageValue == truth, "Actual Boolean methods did not freshly read the stored pointer.");
            Add(proxyTruth, proxyValue);
            Add(storageTruth, storageValue);
            if (expected != null)
            {
                var kind = Application.read(throughDirect);
                Check(kind == Application.read(expected), "Actual field reader changed the selected supplied input.");
                Add(loadedKind, kind);
            }
        }
        CheckSequence(directResults, [0, 1, 2, 0], "Direct pointer transitions differ.");
        CheckSequence(baseResults, [0, 1, 2, 0], "Base pointer transitions differ.");
        CheckSequence(getResults, [0, 1, 2, 0], "Stored pointer getter transitions differ.");
        CheckSequence(delegatedResults, [0, 1, 2, 0], "Delegated result-local transitions differ.");
        CheckSequence(reverseResults, [0, 1, 2, 0], "Reverse receiver-view transitions differ.");
        CheckSequence(proxyTruth, [0, 1, 1, 0], "Proxy Boolean transitions differ.");
        CheckSequence(storageTruth, [0, 1, 1, 0], "Storage Boolean transitions differ.");
        Check(Application.operator__(borrowedSecond) == inputSecond && Application.get(secondStorage) == inputSecond,
            "The independent second owner lost its supplied pointer.");
        var expectDelta = *Application.expect_count - expectBefore;
        var guardDelta = *Application.guard_count - guardBefore;
        var missingDelta = *Application.missing_count - missingBefore;
        Check(expectDelta == 4 && guardDelta == 4 && missingDelta == 2,
            "Physical direct calls did not evaluate the expectation argument once, or the base route invoked the guard.");
        var canaries = owners[0].canary == -0x6123456789ABCDE0L && owners[1].canary == 0x5123456789ABCDE0L;
        var cookies = firstStorage->refcount.cookie == inputSecond && secondStorage->refcount.cookie == inputFirst;
        Check(canaries && cookies && Application.lifetime_overflow() == 0 && Application.lifetime_live() == 0 && Application.lifetime_count() <= 64,
            "Full-width owner canary, cookie or bounded scalar protocol changed during physical checks.");

        return new JsonObject
        {
            ["typeOrder"] = "Owner,PointerProxy,DerivedStorage,PointerStorage,RefCountCarrier,CellInput_BorrowedReadView,PointerAccess",
            ["completeSizes"] = sizes, ["sequentialPrefixOffsets"] = prefixes, ["arrayStrides"] = strides,
            ["fieldOffsetOrder"] = "Owner.canary,payload;Proxy.internal,use_direct;Derived.base;Storage.pointer,refcount;RefCount.cookie;BorrowedView.alignment,kind",
            ["fieldOffsets"] = offsets, ["actualAddressesAligned"] = aligned,
            ["borrowedOwnerDeltas"] = Integers(firstDelta, secondDelta), ["actualBorrowedIdentities"] = borrowedIdentities,
            ["actualNestedBaseViews"] = baseViews, ["directPointerStates"] = directResults, ["basePointerStates"] = baseResults,
            ["getPointerStates"] = getResults, ["delegatedPointerStates"] = delegatedResults, ["reversePointerStates"] = reverseResults,
            ["proxyTruthValues"] = proxyTruth, ["storageTruthValues"] = storageTruth, ["actualReadKindsAfterFinalScenario"] = loadedKind,
            ["expectArgumentDelta"] = expectDelta, ["guardDelta"] = guardDelta, ["missingDelta"] = missingDelta,
            ["fullWidthCanariesUnchanged"] = canaries, ["cookieIdentitiesUnchanged"] = cookies,
            ["secondOwnerPointerUnchanged"] = true, ["cppLifetimeOrVirtualExecutionClaim"] = false
        };
    }

    private static bool Aligned8(void* address) => ((nuint)address) % 8 == 0;
    private static int Identify(InputView* value, InputView* first, InputView* second) =>
        value == null ? 0 : value == first ? 1 : value == second ? 2 : throw new InvalidOperationException("Unexpected returned pointer identity.");
    private static void Add(JsonArray array, int value) => array.Add((JsonNode)JsonValue.Create(value)!);
    private static JsonArray Integers(params int[] values)
    {
        var result = new JsonArray();
        foreach (var value in values) Add(result, value);
        return result;
    }
    private static void CheckSequence(JsonArray values, int[] expected, string message)
    {
        Check(values.Count == expected.Length, message);
        for (var index = 0; index < expected.Length; ++index) Check(values[index]?.GetValue<int>() == expected[index], message);
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
