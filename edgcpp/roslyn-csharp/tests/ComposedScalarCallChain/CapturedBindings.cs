using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Application = Translated.Program;
using OwnerView = Translated.ScalarOwner_BorrowedMemberView;
using InputView = Translated.ScalarInput_BorrowedReadView;
using Proxy = Translated.ScalarProxy;
using Derived = Translated.ScalarDerivedStorage;
using Storage = Translated.ScalarStorage;
using RefCount = Translated.ScalarRefCount;
using Access = Translated.ScalarAccess;

internal static unsafe partial class CapturedBindings
{
    internal static partial int Fixture(void* firstOwner, void* secondOwner, void* firstInput, void* secondInput) =>
        Application.fixture((OwnerView*)firstOwner, (OwnerView*)secondOwner, (InputView*)firstInput, (InputView*)secondInput);
    internal static partial int Count() => Application.lifetime_count();
    internal static partial int Overflow() => Application.lifetime_overflow();
    internal static partial int Live() => Application.lifetime_live();
    internal static partial int Event(int index) => Application.lifetime_event(index);
    internal static partial int Value(int index) => Application.lifetime_value(index);
    internal static partial int ReadKind(void* input) => Application.read((InputView*)input);
    internal static partial void* View(void* owner) => Application.view((OwnerView*)owner);

    [StructLayout(LayoutKind.Sequential)] private struct OwnerPrefix { public byte Prefix; public OwnerView Value; }
    [StructLayout(LayoutKind.Sequential)] private struct InputPrefix { public byte Prefix; public InputView Value; }
    [StructLayout(LayoutKind.Sequential)] private struct ProxyPrefix { public byte Prefix; public Proxy Value; }
    [StructLayout(LayoutKind.Sequential)] private struct DerivedPrefix { public byte Prefix; public Derived Value; }
    [StructLayout(LayoutKind.Sequential)] private struct StoragePrefix { public byte Prefix; public Storage Value; }
    [StructLayout(LayoutKind.Sequential)] private struct RefCountPrefix { public byte Prefix; public RefCount Value; }
    [StructLayout(LayoutKind.Sequential)] private struct AccessPrefix { public byte Prefix; public Access Value; }

    internal static partial JsonObject PhysicalHooks(void* firstOwner, void* secondOwner, void* firstInput, void* secondInput)
    {
        OwnerView* ownerViews = stackalloc OwnerView[2];
        InputView* inputViews = stackalloc InputView[2];
        Proxy* proxies = stackalloc Proxy[2];
        Derived* derived = stackalloc Derived[2];
        Storage* storage = stackalloc Storage[2];
        RefCount* counts = stackalloc RefCount[2];
        Access* accesses = stackalloc Access[2];
        OwnerPrefix ownerPrefix = default;
        InputPrefix inputPrefix = default;
        ProxyPrefix proxyPrefix = default;
        DerivedPrefix derivedPrefix = default;
        StoragePrefix storagePrefix = default;
        RefCountPrefix countPrefix = default;
        AccessPrefix accessPrefix = default;
        var sizes = Integers(sizeof(OwnerView), sizeof(InputView), sizeof(Proxy), sizeof(Derived), sizeof(Storage), sizeof(RefCount), sizeof(Access));
        Sequence(sizes, [56,56,24,16,16,8,1], "Actual emitted view/carrier sizes differ.");
        var prefixes = Integers(
            (int)((byte*)(&ownerPrefix.Value)-(byte*)(&ownerPrefix)), (int)((byte*)(&inputPrefix.Value)-(byte*)(&inputPrefix)),
            (int)((byte*)(&proxyPrefix.Value)-(byte*)(&proxyPrefix)), (int)((byte*)(&derivedPrefix.Value)-(byte*)(&derivedPrefix)),
            (int)((byte*)(&storagePrefix.Value)-(byte*)(&storagePrefix)), (int)((byte*)(&countPrefix.Value)-(byte*)(&countPrefix)),
            (int)((byte*)(&accessPrefix.Value)-(byte*)(&accessPrefix)));
        Sequence(prefixes, [8,8,8,8,8,8,1], "Actual sequential-prefix alignments differ.");
        var strides = Integers(
            (int)((byte*)(ownerViews+1)-(byte*)ownerViews), (int)((byte*)(inputViews+1)-(byte*)inputViews),
            (int)((byte*)(proxies+1)-(byte*)proxies), (int)((byte*)(derived+1)-(byte*)derived),
            (int)((byte*)(storage+1)-(byte*)storage), (int)((byte*)(counts+1)-(byte*)counts),
            (int)((byte*)(accesses+1)-(byte*)accesses));
        Sequence(strides, [56,56,24,16,16,8,1], "Actual typed array strides differ.");
        var offsets = Integers(
            (int)((byte*)(&ownerViews[0].__borrowed_alignment)-(byte*)ownerViews), (int)((byte*)(&ownerViews[0].payload)-(byte*)ownerViews),
            (int)((byte*)(&inputViews[0].__borrowed_alignment)-(byte*)inputViews), (int)((byte*)(&inputViews[0].kind)-(byte*)inputViews),
            (int)((byte*)(&proxies[0].@internal)-(byte*)proxies), (int)((byte*)(&proxies[0].use_direct)-(byte*)proxies),
            (int)((byte*)(&derived[0].__b_13ScalarStorage)-(byte*)derived),
            (int)((byte*)(&storage[0].pointer)-(byte*)storage), (int)((byte*)(&storage[0].refcount)-(byte*)storage),
            (int)((byte*)(&counts[0].cookie)-(byte*)counts));
        Sequence(offsets, [0,24,0,24,0,16,0,0,8,0], "Actual fields or generated-base offsets differ.");
        var aligned = Aligned8(ownerViews) && Aligned8(ownerViews+1) && Aligned8(inputViews) && Aligned8(inputViews+1) &&
            Aligned8(proxies) && Aligned8(proxies+1) && Aligned8(derived) && Aligned8(derived+1) &&
            Aligned8(storage) && Aligned8(storage+1) && Aligned8(counts) && Aligned8(counts+1) &&
            Aligned8(firstOwner) && Aligned8(secondOwner) && Aligned8(firstInput) && Aligned8(secondInput);
        Check(aligned, "An actual view/carrier or supplied raw buffer is not naturally 8-byte aligned.");
        var suppliedFirstView = Application.view((OwnerView*)firstOwner);
        var suppliedSecondView = Application.view((OwnerView*)secondOwner);
        var suppliedIdentities = suppliedFirstView == &((OwnerView*)firstOwner)->payload &&
            suppliedSecondView == &((OwnerView*)secondOwner)->payload && suppliedFirstView != suppliedSecondView &&
            (byte*)suppliedFirstView-(byte*)firstOwner == 24 && (byte*)suppliedSecondView-(byte*)secondOwner == 24;
        Check(suppliedIdentities, "Actual supplied-owner getters lost their exact payload addresses.");

        // Additional borrowed byte owners permit independent pointer transitions.
        // They do not construct the unrelated polymorphic C++ prefix.
        using var rawFirst = new RawLayoutBytes(0x35);
        using var rawSecond = new RawLayoutBytes(0xc3);
        rawFirst.WriteInt64(8,-0x2123456789ABCDE0L);
        rawFirst.WriteInt64(16,0x1123456789ABCDE0L);
        rawFirst.WriteInt64(48,0x2132435465760718L);
        rawSecond.WriteInt64(8,-0x3123456789ABCDE0L);
        rawSecond.WriteInt64(16,0x2123456789ABCDE0L);
        rawSecond.WriteInt64(48,-0x3142536475061728L);
        var inputFirst = (InputView*)firstInput;
        var inputSecond = (InputView*)secondInput;
        var first = Application.view((OwnerView*)rawFirst.Data);
        var second = Application.view((OwnerView*)rawSecond.Data);
        var firstStorage = &first->@internal.__b_13ScalarStorage;
        var secondStorage = &second->@internal.__b_13ScalarStorage;
        firstStorage->pointer = null;
        firstStorage->refcount.cookie = inputSecond;
        secondStorage->pointer = inputSecond;
        secondStorage->refcount.cookie = inputFirst;
        first->use_direct = 1;
        second->use_direct = 0;
        var firstDelta = (int)((byte*)first-(byte*)rawFirst.Data);
        var secondDelta = (int)((byte*)second-(byte*)rawSecond.Data);
        var physicalIdentities = first == &((OwnerView*)rawFirst.Data)->payload && second == &((OwnerView*)rawSecond.Data)->payload &&
            first != second && firstDelta == 24 && secondDelta == 24 && Aligned8(rawFirst.Data) && Aligned8(rawSecond.Data);
        var baseViews = (byte*)firstStorage == (byte*)(&first->@internal) && (byte*)secondStorage == (byte*)(&second->@internal);
        Check(physicalIdentities && baseViews, "Independent borrowed owner/base views do not preserve actual captured coordinates.");
        Check(Application.read(inputFirst) == 128 && Application.read(inputSecond) == 255,
            "The final supplied high-bit scalar bytes were not freshly read as unsigned values.");
        var expectBefore = *Application.expect_count;
        var guardBefore = *Application.guard_count;
        var missingBefore = *Application.missing_count;
        var directPointers = new JsonArray();
        var basePointers = new JsonArray();
        var getters = new JsonArray();
        var delegated = new JsonArray();
        var reverse = new JsonArray();
        var proxyTruth = new JsonArray();
        var storageTruth = new JsonArray();
        var composedDirect = new JsonArray();
        var composedBase = new JsonArray();
        var loadedKinds = new JsonArray();
        var effects = new JsonArray();
        for (var phase = 0; phase < 4; ++phase)
        {
            InputView* expected = phase == 1 ? inputFirst : phase == 2 ? inputSecond : null;
            var truth = expected == null ? 0 : 1;
            var expectedKind = phase == 1 ? 128 : phase == 2 ? 255 : 0;
            firstStorage->pointer = expected;
            first->use_direct = 1;
            var firstBefore = rawFirst.Snapshot();
            var secondBefore = rawSecond.Snapshot();
            var expectation = *Application.expect_count;
            var guard = *Application.guard_count;
            var missing = *Application.missing_count;
            Add(getters, Identify(Application.get(firstStorage),inputFirst,inputSecond));
            Add(delegated, Identify(Application.operator____2((Access*)firstStorage),inputFirst,inputSecond));
            Add(reverse, Identify(Application.base_pointer((Access*)firstStorage),inputFirst,inputSecond));
            var proxyBoolean = Application.operator_bool(first);
            var storageBoolean = Application.operator_bool__2(firstStorage);
            Check(proxyBoolean == truth && storageBoolean == truth, "Actual Boolean methods used stale stored pointer bits.");
            Add(proxyTruth,proxyBoolean);
            Add(storageTruth,storageBoolean);
            var direct = Application.operator__(first);
            Check(direct == expected, "Direct local/return path changed the supplied pointer identity.");
            Add(directPointers,Identify(direct,inputFirst,inputSecond));
            var directComposed = Application.composed((OwnerView*)rawFirst.Data);
            Check(directComposed == expectedKind, "Composed direct scalar call ignored current null/pointer/high-bit state.");
            Add(composedDirect,directComposed);
            if (direct != null) Add(loadedKinds,Application.read(direct));
            var expectDelta = *Application.expect_count-expectation;
            var guardDelta = *Application.guard_count-guard;
            var missingDelta = *Application.missing_count-missing;
            Check(expectDelta == 1+truth && guardDelta == 1+truth && missingDelta == 1-truth &&
                rawFirst.Unchanged(firstBefore) && rawSecond.Unchanged(secondBefore),
                "Direct and composed branches did not have exact once-only effects, or modified borrowed bytes/guards.");

            first->use_direct = 0;
            firstBefore = rawFirst.Snapshot();
            secondBefore = rawSecond.Snapshot();
            expectation = *Application.expect_count;
            guard = *Application.guard_count;
            missing = *Application.missing_count;
            var baseResult = Application.operator__(first);
            Check(baseResult == expected, "Actual empty-base delegation returned a stale or foreign pointer.");
            Add(basePointers,Identify(baseResult,inputFirst,inputSecond));
            var baseComposed = Application.composed((OwnerView*)rawFirst.Data);
            Check(baseComposed == expectedKind, "Composed base scalar call ignored current null/pointer/high-bit state.");
            Add(composedBase,baseComposed);
            Check(*Application.expect_count == expectation && *Application.guard_count == guard && *Application.missing_count == missing &&
                rawFirst.Unchanged(firstBefore) && rawSecond.Unchanged(secondBefore),
                "Base/no-call branches invoked the expectation/guard or modified borrowed bytes/guards.");
            effects.Add((JsonNode)new JsonObject
            {
                ["pointerState"] = phase == 3 ? 0 : phase, ["directAndComposedExpectationDelta"] = expectDelta,
                ["directAndComposedGuardDelta"] = guardDelta, ["directMissingDelta"] = missingDelta,
                ["baseExpectationDelta"] = 0, ["baseGuardDelta"] = 0,
                ["composedNullBranchSkippedPointerCall"] = truth == 0, ["bothWholeOwnersAndGuardsUnchanged"] = true
            });
        }
        Sequence(directPointers,[0,1,2,0],"Direct pointer transitions differ.");
        Sequence(basePointers,[0,1,2,0],"Base pointer transitions differ.");
        Sequence(getters,[0,1,2,0],"Stored pointer getter transitions differ.");
        Sequence(delegated,[0,1,2,0],"Delegated result-local transitions differ.");
        Sequence(reverse,[0,1,2,0],"Reverse receiver-view transitions differ.");
        Sequence(proxyTruth,[0,1,1,0],"Proxy Boolean transitions differ.");
        Sequence(storageTruth,[0,1,1,0],"Storage Boolean transitions differ.");
        Sequence(composedDirect,[0,128,255,0],"Composed direct scalar transitions differ.");
        Sequence(composedBase,[0,128,255,0],"Composed base scalar transitions differ.");
        Sequence(loadedKinds,[128,255],"Actual selected high-bit field reads differ.");
        var beforeSecondFirst = rawFirst.Snapshot();
        var beforeSecondSecond = rawSecond.Snapshot();
        var independentSecond = Application.view((OwnerView*)rawSecond.Data) == second &&
            Application.get(secondStorage) == inputSecond && Application.operator__(second) == inputSecond &&
            Application.composed((OwnerView*)rawSecond.Data) == 255;
        Check(independentSecond && rawFirst.Unchanged(beforeSecondFirst) && rawSecond.Unchanged(beforeSecondSecond),
            "The independent second owner lost its pointer/result or borrowed byte immutability.");
        var finalExpect = *Application.expect_count-expectBefore;
        var finalGuard = *Application.guard_count-guardBefore;
        var finalMissing = *Application.missing_count-missingBefore;
        Check(finalExpect == 6 && finalGuard == 6 && finalMissing == 2, "Physical expectation/guard/missing totals differ.");
        var canaries = *(long*)((byte*)rawFirst.Data+8) == -0x2123456789ABCDE0L &&
            *(long*)((byte*)rawFirst.Data+16) == 0x1123456789ABCDE0L && *(long*)((byte*)rawFirst.Data+48) == 0x2132435465760718L &&
            *(long*)((byte*)rawSecond.Data+8) == -0x3123456789ABCDE0L &&
            *(long*)((byte*)rawSecond.Data+16) == 0x2123456789ABCDE0L && *(long*)((byte*)rawSecond.Data+48) == -0x3142536475061728L;
        var cookies = firstStorage->refcount.cookie == inputSecond && secondStorage->refcount.cookie == inputFirst;
        Check(canaries && cookies && Application.lifetime_overflow() == 0 && Application.lifetime_live() == 0 && Application.lifetime_count() <= 64,
            "Full-width untouched fields, pointer cookies or scalar event bounds changed.");
        return new JsonObject
        {
            ["typeOrder"] = "borrowed Owner view,borrowed Input view,Proxy,DerivedStorage,Storage,RefCount,Access",
            ["sizes"] = sizes, ["sequentialPrefixOffsets"] = prefixes, ["arrayStrides"] = strides,
            ["fieldOffsetOrder"] = "Owner.alignment,payload;Input.alignment,kind;Proxy.internal,use_direct;Derived.base;Storage.pointer,refcount;RefCount.cookie",
            ["fieldOffsets"] = offsets, ["actualAddressesAligned"] = aligned,
            ["suppliedOwnerGetterIdentities"] = suppliedIdentities, ["independentRawOwnerGetterIdentities"] = physicalIdentities,
            ["borrowedOwnerDeltas"] = Integers(firstDelta,secondDelta), ["nestedBaseViews"] = baseViews,
            ["directPointerStates"] = directPointers, ["basePointerStates"] = basePointers,
            ["getterPointerStates"] = getters, ["delegatedPointerStates"] = delegated, ["reversePointerStates"] = reverse,
            ["proxyTruthValues"] = proxyTruth, ["storageTruthValues"] = storageTruth,
            ["composedDirectValues"] = composedDirect, ["composedBaseValues"] = composedBase,
            ["actualSelectedReadKinds"] = loadedKinds, ["phaseEffects"] = effects,
            ["expectArgumentDelta"] = finalExpect, ["guardDelta"] = finalGuard, ["missingDelta"] = finalMissing,
            ["secondOwnerPointerAndScalarResult"] = independentSecond, ["fullWidthUntouchedFieldsUnchanged"] = canaries,
            ["pointerCookieIdentitiesUnchanged"] = cookies, ["cppConstructionOrVirtualExecutionClaim"] = false,
            ["ownerAndInputAreBorrowedLayoutViews"] = true, ["generatedPolymorphicPrefixClaim"] = false
        };
    }

    private static bool Aligned8(void* value) => ((nuint)value)%8 == 0;
    private static int Identify(InputView* value, InputView* first, InputView* second) =>
        value == null ? 0 : value == first ? 1 : value == second ? 2 : throw new InvalidOperationException("Foreign pointer bits were returned.");
    private static void Add(JsonArray array,int value) => array.Add((JsonNode)JsonValue.Create(value)!);
    private static JsonArray Integers(params int[] values) { var result = new JsonArray(); foreach(var value in values) Add(result,value); return result; }
    private static void Sequence(JsonArray values,int[] expected,string message)
    { Check(values.Count == expected.Length,message); for(var index=0;index<expected.Length;++index) Check(values[index]?.GetValue<int>() == expected[index],message); }
    private static void Check(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
}
