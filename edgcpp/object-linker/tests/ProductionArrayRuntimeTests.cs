// Standalone contract tests for the shared array runtime, without a fixture provider.
#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Edg.Runtime;

internal sealed class ArrayRuntimeTransfer(int code) : Exception { internal int Code { get; } = code; }
internal sealed class ArraySourceFailure(int code) : Exception { internal int Code { get; } = code; }
internal static unsafe class ProductionArrayRuntimeTests
{
    private const ulong Count = 4, Stride = 8;
    private static byte* destination, source;
    private static int ctorFailure, destructorFailures, destructorTransfers, assertions;
    private static bool ctorTransfer, printEvents;
    private static readonly List<string> trace = [];
    private static ArrayRuntimeTransfer? lastTransfer;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name + ": " + string.Join(" ", trace));
        ++assertions;
    }
    private static void Event(string value)
    {
        trace.Add(value);
        if (printEvents) { Console.WriteLine(value); Console.Out.Flush(); }
    }
    private static int Index(void* pointer) => checked((int)(((byte*)pointer - destination) / (long)Stride));
    private static void Construct(void* pointer)
    {
        int index = Index(pointer); Event("C" + index);
        if (index == ctorFailure)
        {
            if (ctorTransfer) throw lastTransfer = new ArrayRuntimeTransfer(100 + index);
            throw new ArraySourceFailure(100 + index);
        }
        *(int*)pointer = 100 + index;
    }
    private static void Copy(void* pointer, void* original)
    {
        int index = Index(pointer); Event("X" + index);
        Check((byte*)original == source + (ulong)index * Stride, "copy source stride");
        if (index == ctorFailure)
        {
            if (ctorTransfer) throw lastTransfer = new ArrayRuntimeTransfer(100 + index);
            throw new ArraySourceFailure(100 + index);
        }
        *(int*)pointer = *(int*)original;
    }
    private static void Destroy(void* pointer)
    {
        int index = Index(pointer); Event("D" + index);
        if ((destructorTransfers & (1 << index)) != 0)
            throw lastTransfer = new ArrayRuntimeTransfer(200 + index);
        if ((destructorFailures & (1 << index)) != 0) throw new ArraySourceFailure(200 + index);
    }
    private static void Reset(int constructor = -1, bool transfer = false, int failDestructors = 0, int transferDestructors = 0)
    {
        ctorFailure = constructor; ctorTransfer = transfer;
        destructorFailures = failDestructors; destructorTransfers = transferDestructors;
        trace.Clear(); lastTransfer = null;
        for (ulong i = 0; i < Count; ++i) { *(int*)(destination + i * Stride) = -1; *(int*)(source + i * Stride) = 100 + (int)i; }
    }
    private static void Expected(string events) => Check(string.Join(" ", trace) == events, "exact element order");
    private static void Invoke(string operation)
    {
        if (operation == "construct") CppArrays.Construct(destination, Count, Stride, &Construct, &Destroy);
        else if (operation == "copy") CppArrays.CopyConstruct(destination, source, Count, Stride, &Copy, &Destroy);
        else if (operation == "destroy") CppArrays.Destroy(destination, Count, Stride, &Destroy);
        else CppArrays.Cleanup(destination, Count, Stride, &Destroy);
    }
    private static void Failure(string operation, bool transfer, int code, string events)
    {
        Exception? caught = null;
        try { Invoke(operation); } catch (Exception error) { caught = error; }
        if (transfer)
            Check(caught is ArrayRuntimeTransfer actual && actual.Code == code && ReferenceEquals(caught, lastTransfer), "latest transfer identity");
        else Check(caught is ArraySourceFailure actual && actual.Code == code, "original source failure preserved");
        Expected(events);
    }
    public static int Main(string[] args)
    {
        destination = (byte*)NativeMemory.Alloc((nuint)(Count * Stride));
        source = (byte*)NativeMemory.Alloc((nuint)(Count * Stride));
        try
        {
            if (args.Length != 0) return FailFastCase(args[0]);
            foreach (string operation in new[] { "construct", "copy" })
            {
                string c = operation == "construct" ? "C" : "X";
                Reset(); Invoke(operation); Expected(c + "0 " + c + "1 " + c + "2 " + c + "3");
                for (ulong i = 0; i < Count; ++i) Check(*(int*)(destination + i * Stride) == 100 + (int)i, "constructed or copied value");
                trace.Clear(); CppArrays.Destroy(destination, Count, Stride, &Destroy); Expected("D3 D2 D1 D0");
                for (int failure = 0; failure != 4; ++failure)
                {
                    var events = new List<string>();
                    for (int i = 0; i <= failure; ++i) events.Add(c + i);
                    for (int i = failure - 1; i >= 0; --i) events.Add("D" + i);
                    Reset(constructor: failure); Failure(operation, false, 100 + failure, string.Join(" ", events));
                    Reset(constructor: failure, transfer: true); Failure(operation, true, 100 + failure, string.Join(" ", events));
                }
                Reset(constructor: 3, transferDestructors: (1 << 2) | (1 << 1));
                Failure(operation, true, 201, c + "0 " + c + "1 " + c + "2 " + c + "3 D2 D1 D0");
                Reset(constructor: 3, transfer: true, transferDestructors: (1 << 2) | (1 << 1));
                Failure(operation, true, 201, c + "0 " + c + "1 " + c + "2 " + c + "3 D2 D1 D0");
            }
            for (int failure = 0; failure != 4; ++failure)
            {
                Reset(failDestructors: 1 << failure); Failure("destroy", false, 200 + failure, "D3 D2 D1 D0");
                Reset(transferDestructors: 1 << failure); Failure("destroy", true, 200 + failure, "D3 D2 D1 D0");
                Reset(transferDestructors: 1 << failure); Failure("cleanup", true, 200 + failure, "D3 D2 D1 D0");
            }
            Reset(transferDestructors: (1 << 3) | (1 << 1)); Failure("destroy", true, 201, "D3 D2 D1 D0");
            Reset(transferDestructors: (1 << 3) | (1 << 1)); Failure("cleanup", true, 201, "D3 D2 D1 D0");
            Reset(failDestructors: 1 << 3, transferDestructors: (1 << 2) | (1 << 1));
            Failure("destroy", true, 201, "D3 D2 D1 D0");
            Reset();
            CppArrays.Construct(destination, 0, Stride, &Construct, &Destroy);
            CppArrays.CopyConstruct(destination, source, 0, Stride, &Copy, &Destroy);
            CppArrays.Destroy(destination, 0, Stride, &Destroy); CppArrays.Cleanup(destination, 0, Stride, &Destroy);
            CppArrays.Construct(destination, Count, Stride, null, &Destroy);
            CppArrays.CopyConstruct(destination, source, Count, Stride, null, &Destroy);
            CppArrays.Destroy(null, Count, Stride, &Destroy); CppArrays.Cleanup(null, Count, Stride, &Destroy);
            CppArrays.Destroy(destination, Count, Stride, null); CppArrays.Cleanup(destination, Count, Stride, null);
            Expected("");
            Reset(constructor: 2);
            Exception? failureWithoutDestructor = null;
            try { CppArrays.Construct(destination, Count, Stride, &Construct, null); }
            catch (ArraySourceFailure error) { failureWithoutDestructor = error; }
            Check(failureWithoutDestructor is not null, "construction failure with trivial destructor"); Expected("C0 C1 C2");
            Console.WriteLine("production-array-runtime checks=" + assertions);
            return 0;
        }
        finally { NativeMemory.Free(destination); NativeMemory.Free(source); }
    }
    private static int FailFastCase(string name)
    {
        printEvents = true;
        if (name == "cleanup") { Reset(failDestructors: 1 << 2); Invoke("cleanup"); }
        else if (name == "destroy") { Reset(failDestructors: (1 << 3) | (1 << 2)); Invoke("destroy"); }
        else if (name == "construct") { Reset(constructor: 3, failDestructors: 1 << 2); Invoke("construct"); }
        else if (name == "copy") { Reset(constructor: 3, failDestructors: 1 << 2); Invoke("copy"); }
        else if (name == "transfer-then-failure") { Reset(failDestructors: 1 << 2, transferDestructors: 1 << 3); Invoke("cleanup"); }
        else throw new ArgumentException(name);
        return 0;
    }
}
namespace Edg.Runtime
{
    public static class ObjectProgram
    {
        public static bool IsRuntimeTransfer(Exception error) => error is ArrayRuntimeTransfer;
    }
}
