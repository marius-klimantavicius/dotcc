// Fixture-only provider; production C++ exception dispatch remains separate.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tests.CatchAll;

internal static unsafe class CatchAllHost
{
    internal static int Selected, LivePayloads, Releases;
    internal static readonly List<int> Trace = [];
    internal static void AssertHandlerExit()
    {
        foreach (int id in new[] { 2, 4 })
            if (Trace.Contains(id) && Selected != id && Trace.LastIndexOf(id) > Trace.LastIndexOf(-id))
                throw new Exception("Caught payload released before handler local " + id);
    }
    public static int Main()
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        for (int mode = 0; mode != 15; ++mode)
            foreach (int failure in new[] { 0, 1, 2, 3, 4, 6, 9 })
            {
                Selected = failure; Trace.Clear();
                try
                {
                    int result = mode < 9 ? Core.catch_all_case(mode) : mode < 11 ? Core.catch_all_without_locals(mode-9) : mode < 13 ? Core.catch_all_loop(mode-11) : mode == 13 ? Core.catch_exit_boundary() : Core.catch_handler_exit_failure();
                    Trace.Add(300 + result);
                }
                catch (global::Edg.Runtime.FixturePayload payload)
                {
                    Trace.Add((payload.Type.LeafKind == "record" ? 500 : 400) + *(int*)payload.ObjectAddress);
                    payload.Propagating = false; payload.ReleaseIfUnused();
                }
                if (LivePayloads != 0 || global::Edg.Runtime.FixtureCatch.Frames.Count != 0) throw new Exception("Leaked payload or handler frame");
                Console.WriteLine(mode + ":" + failure + " " + string.Join(" ", Trace));
            }
        if (Releases == 0) throw new Exception("Handler release was not exercised");
        return 0;
    }
}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void record_event(int argument0) => CatchAllHost.Trace.Add(argument0);
        public static partial int should_fail(int argument0) => argument0 == CatchAllHost.Selected ? 1 : 0;
    }
    internal sealed unsafe class FixturePayload : CppException
    {
        private void* memory;
        private readonly delegate* managed<void*, void> destructor;
        internal readonly CppExceptionType Type;
        internal bool Started, Complete, Propagating;
        internal int Handlers;
        internal FixturePayload(CppExceptionType type, delegate* managed<void*, void> destroy)
        {
            if (type.PayloadAlignment > 8) throw new Exception("Fixture alignment exceeded");
            Type = type; destructor = destroy; memory = NativeMemory.Alloc(type.PayloadSize);
            ++CatchAllHost.LivePayloads;
        }
        public override void* ObjectAddress => memory;
        internal void ReleaseIfUnused()
        {
            if (Handlers != 0 || Propagating) return;
            if (memory == null) throw new Exception("Double exception release");
            if (Complete && destructor != null) destructor(memory);
            NativeMemory.Free(memory); memory = null; --CatchAllHost.LivePayloads; ++CatchAllHost.Releases;
        }
    }
    internal sealed class FixtureCatch(Exception error) : CppCatch
    {
        [ThreadStatic] private static Stack<FixtureCatch>? frames;
        internal static Stack<FixtureCatch> Frames => frames ??= new();
        internal readonly Exception Error = error;
        internal bool IsActive = true, Caught;
        public override bool Active => IsActive;
    }
    public static unsafe partial class CppExceptions
    {
        public static partial CppException Allocate(CppExceptionType type, delegate* managed<void*, void> completeDestructor) => new FixturePayload(type, completeDestructor);
        public static partial void EnsureStarted(CppException payload) => ((FixturePayload)payload).Started = true;
        public static partial void MarkComplete(CppException payload)
        { var p = (FixturePayload)payload; p.Complete = true; p.Propagating = true; }
        public static partial bool FilterInitializationFailure(CppException payload, Exception error)
        {
            if (ReferenceEquals(payload, error)) return false;
            if (((FixturePayload)payload).Started) Environment.FailFast("Fixture: copy failed after start", error);
            return true;
        }
        public static partial void ReleaseIncomplete(CppException payload)
        { var p = (FixturePayload)payload; if (p.Complete) throw new Exception("Completed payload on failure path"); p.ReleaseIfUnused(); }
        public static partial Exception Rethrow()
        {
            var frame = FixtureCatch.Frames.Peek();
            if (frame.Error is FixturePayload p) p.Propagating = true;
            throw frame.Error;
        }
        public static partial bool FilterCatchAll(Exception error) => !ObjectProgram.IsRuntimeTransfer(error);
        public static partial CppCatch EnterCatch(Exception error)
        {
            var frame = new FixtureCatch(error);
            if (error is FixturePayload p)
            { if (!p.Complete) throw new Exception("Caught unfinished payload"); ++p.Handlers; p.Propagating = false; }
            FixtureCatch.Frames.Push(frame); return frame;
        }
        public static partial void MarkCaught(CppCatch frame) => ((FixtureCatch)frame).Caught = true;
        public static partial bool FilterHandlerFailure(CppCatch frame, Exception error)
        {
            if (!((FixtureCatch)frame).Caught) Environment.FailFast("Fixture: catch initialization failure", error);
            return !ObjectProgram.IsRuntimeTransfer(error);
        }
        public static partial void LeaveCatch(CppCatch frame)
        {
            var f = (FixtureCatch)frame; if (!f.IsActive) return;
            if (!ReferenceEquals(FixtureCatch.Frames.Peek(), f)) throw new Exception("Handler stack order");
            f.IsActive = false; FixtureCatch.Frames.Pop();
            if (f.Error is FixturePayload p)
            {
                --p.Handlers;
                // Only the original class payload owns this function's guard;
                // a nested scalar handler may end while that guard remains live.
                if (p.Type.LeafKind == "record" && p.Handlers == 0) CatchAllHost.AssertHandlerExit();
                p.ReleaseIfUnused();
            }
        }
    }
}
