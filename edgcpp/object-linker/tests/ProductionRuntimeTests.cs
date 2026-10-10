// The production provider API, without fixture providers or translated internals.
using System;
using System.Threading;
using System.Threading.Tasks;
using Edg.Runtime;

internal static unsafe class ProductionRuntimeTests
{
    private static int destroyed, assertions;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Interlocked.Increment(ref assertions);
    }
    private static void Destroy(void* payload) => Interlocked.Increment(ref destroyed);
    private static void ThrowingDestroy(void* payload) => throw new InvalidOperationException("destructor test");
    private static CppExceptionType Type(string identity = "runtime:derived", nuint alignment = 8,
        int[]? qualifiers = null, string leaf = "runtime:derived", string kind = "record")
        => new(identity, leaf, kind, qualifiers ?? [], qualifiers is { Length: > 0 } ? (nuint)sizeof(void*) : 64, alignment, qualifiers is not { Length: > 0 },
            new CppExceptionBase[] { new("runtime:base", 16, true, false),
                new("runtime:private", 24, false, false), new("runtime:ambiguous", 32, true, true) });
    private static CppCatchType Catch(string identity = "runtime:derived", int[]? qualifiers = null,
        string leaf = "runtime:derived", string kind = "record") => new(identity, leaf, kind, qualifiers ?? []);
    private static CppException Payload(CppExceptionType? type = null)
    {
        type ??= Type();
        var payload = CppExceptions.Allocate(type, type.RequiresDestruction ? &Destroy : null);
        CppExceptions.EnsureStarted(payload);
        CppExceptions.MarkComplete(payload);
        return payload;
    }
    private static CppCatch Enter(CppException payload)
    {
        var frame = CppExceptions.EnterCatch(payload);
        CppExceptions.MarkCaught(frame);
        return frame;
    }
    internal static int Run()
    {
        Check(CppExceptions.UncaughtExceptions == 0, "initial count");
        for (nuint alignment = 1; alignment <= 4096; alignment *= 2)
        {
            var payload = CppExceptions.Allocate(Type(alignment: alignment), &Destroy);
            Check((nuint)payload.ObjectAddress % alignment == 0, "payload alignment");
            Check(CppExceptions.FilterInitializationFailure(payload, new Exception()), "pre-start failure allowed");
            CppExceptions.ReleaseIncomplete(payload);
            Check(payload.ObjectAddress == null, "incomplete freed");
        }
        Check(destroyed == 0, "incomplete destructor disarmed");
        var value = Payload();
        Check(CppExceptions.UncaughtExceptions == 1, "throw count");
        CppExceptions.EnsureStarted(value);
        Check(CppExceptions.UncaughtExceptions == 1, "start is idempotent");
        Check(!CppExceptions.FilterInitializationFailure(value, value), "own throw is not init failure");
        Check(CppExceptions.FilterTypedCatch(value, Catch()), "exact identity");
        Check(CppExceptions.FilterTypedCatch(value, Catch("runtime:base", leaf: "runtime:base")), "public base");
        Check(!CppExceptions.FilterTypedCatch(value, Catch("runtime:private", leaf: "runtime:private")), "private rejected");
        Check(!CppExceptions.FilterTypedCatch(value, Catch("runtime:ambiguous", leaf: "runtime:ambiguous")), "ambiguous rejected");
        Check(!CppExceptions.FilterTypedCatch(value, Catch("int", leaf: "int", kind: "int")), "no numeric promotion");
        var typed = CppExceptions.EnterTypedCatch(value, Catch("runtime:base", leaf: "runtime:base"));
        Check(CppExceptions.UncaughtExceptions == 1, "count remains during parameter init");
        CppExceptions.MarkCaught(typed);
        Check(CppExceptions.UncaughtExceptions == 0, "count ends after parameter init");
        Check(typed.ObjectAddress == (byte*)value.ObjectAddress + 16, "base address adjustment");
        CppExceptions.LeaveCatch(typed);
        CppExceptions.LeaveCatch(typed);
        Check(!typed.Active && value.ObjectAddress == null && destroyed == 1, "one idempotent destruction");
        PointerCase(false); PointerCase(true);
        NestedRethrow();
        RuntimeTransfers();
        var foreign = new InvalidOperationException("foreign");
        Check(CppExceptions.FilterCatchAll(foreign), "foreign catch-all");
        Check(!CppExceptions.FilterTypedCatch(foreign, Catch()), "foreign typed miss");
        var foreignFrame = CppExceptions.EnterCatch(foreign); CppExceptions.MarkCaught(foreignFrame);
        try { throw CppExceptions.Rethrow(); }
        catch (InvalidOperationException caught) { Check(ReferenceEquals(caught, foreign), "foreign rethrow identity"); }
        CppExceptions.LeaveCatch(foreignFrame);
        using (var barrier = new Barrier(4))
        {
            Task[] tasks = new Task[4];
            for (int i = 0; i != tasks.Length; ++i)
                tasks[i] = Task.Factory.StartNew(() => ThreadCase(barrier), TaskCreationOptions.LongRunning);
            Task.WaitAll(tasks);
        }
        Check(CppExceptions.UncaughtExceptions == 0, "caller thread unchanged");
        Check(destroyed == 7, "exact complete payload destruction count");
        Console.WriteLine("production-runtime checks=" + assertions + " destroyed=" + destroyed);
        return 0;
    }
    private static void PointerCase(bool isNull)
    {
        var value = Payload(Type("derived*", qualifiers: [0]));
        void* original = isNull ? null : (void*)0x10000;
        *(void**)value.ObjectAddress = original;
        var target = Catch("const-base*", [1], "runtime:base");
        Check(CppExceptions.FilterTypedCatch(value, target), "pointer public base and cv");
        Check(CppExceptions.FilterTypedCatch(value, Catch("void*", [0], "void", "void")), "object pointer to void");
        var first = CppExceptions.EnterTypedCatch(value, target); CppExceptions.MarkCaught(first);
        Check(*(void**)first.ObjectAddress == (isNull ? null : (byte*)original + 16), "pointer value adjustment");
        Check(*(void**)value.ObjectAddress == original, "original pointer retained");
        *(void**)first.ObjectAddress = null;
        try { throw CppExceptions.Rethrow(); }
        catch (CppException error)
        {
            Check(ReferenceEquals(error, value), "pointer rethrow identity");
            var second = CppExceptions.EnterTypedCatch(error, Catch("derived*", [0])); CppExceptions.MarkCaught(second);
            Check(*(void**)second.ObjectAddress == original, "rethrow ignores converted cell mutation");
            CppExceptions.LeaveCatch(second);
        }
        CppExceptions.LeaveCatch(first);
        Check(value.ObjectAddress == null, "pointer carrier freed");
        var nested = Payload(Type("derived**", qualifiers: [0, 0]));
        Check(!CppExceptions.FilterTypedCatch(nested, Catch("const-derived**", [0, 1])), "unsafe nested cv rejected");
        Check(CppExceptions.FilterTypedCatch(nested, Catch("const-derived*const*", [1, 1])), "safe nested cv accepted");
        Check(!CppExceptions.FilterTypedCatch(nested, Catch("base**", [0, 0], "runtime:base")), "nested base conversion rejected");
        CppExceptions.LeaveCatch(Enter(nested));
    }
    private static void NestedRethrow()
    {
        var value = Payload(); var first = Enter(value);
        try
        {
            try { throw CppExceptions.Rethrow(); }
            finally
            {
                Check(CppExceptions.UncaughtExceptions == 1, "outer rethrow still pending");
                // A destructor can rethrow/catch the same active object while
                // the first rethrow is still unwinding. Both must be owned.
                try { throw CppExceptions.Rethrow(); }
                catch (CppException nested)
                {
                    Check(CppExceptions.UncaughtExceptions == 2, "two simultaneous rethrows");
                    var second = Enter(nested);
                    Check(CppExceptions.UncaughtExceptions == 1, "inner catch preserves outer propagation");
                    CppExceptions.LeaveCatch(second);
                }
                Check(value.ObjectAddress != null, "inner handler retains pending outer payload");
            }
        }
        catch (CppException error)
        {
            var third = Enter(error);
            Check(CppExceptions.UncaughtExceptions == 0, "outer catch consumes last propagation");
            CppExceptions.LeaveCatch(third);
        }
        CppExceptions.LeaveCatch(first);
        Check(value.ObjectAddress == null, "nested rethrow freed once");
    }
    private static void RuntimeTransfers()
    {
        var transfer = new ProductionRuntimeTransfer();
        Check(!CppExceptions.FilterCatchAll(transfer), "runtime transfer bypasses catch-all");
        Check(!CppExceptions.FilterTypedCatch(transfer, Catch()), "runtime transfer bypasses typed catch");
        var incomplete = CppExceptions.Allocate(Type(), &Destroy);
        CppExceptions.EnsureStarted(incomplete);
        Check(CppExceptions.FilterInitializationFailure(incomplete, transfer), "transfer escapes started initialization");
        CppExceptions.ReleaseIncomplete(incomplete);
        Check(incomplete.ObjectAddress == null && CppExceptions.UncaughtExceptions == 0, "transfer releases incomplete payload");
        var complete = Payload(); var frame = CppExceptions.EnterCatch(complete);
        Check(!CppExceptions.FilterHandlerFailure(frame, transfer), "transfer escapes parameter initialization");
        CppExceptions.LeaveCatch(frame);
        Check(complete.ObjectAddress == null && CppExceptions.UncaughtExceptions == 0, "transfer releases entered handler");
    }
    private static void ThreadCase(Barrier barrier)
    {
        Check(CppExceptions.UncaughtExceptions == 0, "worker initial count");
        var value = Payload(); var first = Enter(value); barrier.SignalAndWait();
        try { throw CppExceptions.Rethrow(); }
        catch (CppException error)
        {
            Check(ReferenceEquals(error, value), "thread-local rethrow");
            var second = Enter(error); barrier.SignalAndWait(); CppExceptions.LeaveCatch(second);
        }
        CppExceptions.LeaveCatch(first);
        Check(value.ObjectAddress == null && CppExceptions.UncaughtExceptions == 0, "worker ownership balanced");
    }
    internal static int Fail(string scenario)
    {
        if (scenario == "rethrow") throw CppExceptions.Rethrow();
        if (scenario == "throw-init")
        {
            var value = CppExceptions.Allocate(Type(), &Destroy); CppExceptions.EnsureStarted(value);
            CppExceptions.FilterInitializationFailure(value, new Exception("copy"));
        }
        else if (scenario == "catch-init")
        {
            var value = Payload(); var frame = CppExceptions.EnterCatch(value);
            CppExceptions.FilterHandlerFailure(frame, new Exception("catch copy"));
        }
        else if (scenario == "destructor")
        {
            var value = CppExceptions.Allocate(Type(), &ThrowingDestroy);
            CppExceptions.EnsureStarted(value); CppExceptions.MarkComplete(value);
            CppExceptions.LeaveCatch(Enter(value));
        }
        return 0;
    }
}

internal sealed class ProductionRuntimeTransfer : Exception {}
namespace Edg.Runtime
{
    public static unsafe partial class ObjectProgram
    {
        static partial void ClassifyTransfer(Exception error, ref bool isRuntimeTransfer)
            => isRuntimeTransfer = error is ProductionRuntimeTransfer;
    }
}
