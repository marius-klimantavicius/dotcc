// Fixture-only exception provider. This is not the shared C++ runtime.
using Tests.ThrowPayload;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal static unsafe class ThrowPayloadHost
{
    internal static int Selected, LivePayloads, Releases;
    internal static readonly List<int> Trace = [];
    internal static void CheckRelease()
    {
        // Release must follow payload-subobject cleanup, while the enclosing
        // source object remains live until the initializer failure propagates.
        foreach (int id in new[] {1, 2, 3})
            if (Trace.Contains(id) && Selected != id && !Trace.Contains(-id))
                throw new Exception("Payload released before constructed member " + id);
        if (Trace.Contains(-9)) throw new Exception("Initializer boundary destroyed outer object");
        ++Releases;
    }
    public static int Main()
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        for (int kind = 0; kind != 8; ++kind)
            foreach (int failure in new[] {0, 1, 2, 3, 9})
            {
                Selected = failure; Trace.Clear();
                try { if (kind == 7) Core.throw_single_payload(); else Core.throw_payload(kind); }
                catch (global::Edg.Runtime.FixturePayload payload)
                {
                    if (!payload.Complete) throw new Exception("Incomplete propagated payload");
                    Trace.Add(payload.Type.LeafKind == "record" ? 200 : 100 + *(int*)payload.ObjectAddress);
                    payload.Release(true);
                }
                if (LivePayloads != 0) throw new Exception("Leaked exception payload");
                Console.WriteLine(kind + ":" + failure + " " + string.Join(" ", Trace));
            }
        if (Releases == 0) throw new Exception("Failure boundary was not exercised");
        return 0;
    }
}

namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial void record_event(int argument0) => ThrowPayloadHost.Trace.Add(argument0);
        public static partial int should_fail(int argument0) => argument0 == ThrowPayloadHost.Selected ? 1 : 0;
    }
    internal sealed unsafe class FixturePayload : CppException
    {
        private void* memory;
        private readonly delegate* managed<void*, void> destructor;
        internal readonly CppExceptionType Type;
        internal bool Started, Complete;
        internal FixturePayload(CppExceptionType type, delegate* managed<void*, void> destroy)
        {
            if (type.PayloadAlignment > 8) throw new Exception("Fixture provider alignment exceeded");
            Type = type; destructor = destroy; memory = NativeMemory.Alloc(type.PayloadSize);
            ++ThrowPayloadHost.LivePayloads;
        }
        public override void* ObjectAddress => memory;
        internal void Release(bool completed)
        {
            if (memory == null || Complete != completed) throw new Exception("Invalid payload release");
            if (completed && destructor != null) destructor(memory);
            NativeMemory.Free(memory); memory = null; --ThrowPayloadHost.LivePayloads;
        }
    }
    public static unsafe partial class CppExceptions
    {
        public static partial CppException Allocate(CppExceptionType type, delegate* managed<void*, void> completeDestructor)
            => new FixturePayload(type, completeDestructor);
        public static partial void EnsureStarted(CppException payload) => ((FixturePayload)payload).Started = true;
        public static partial void MarkComplete(CppException payload) => ((FixturePayload)payload).Complete = true;
        public static partial bool FilterInitializationFailure(CppException payload, Exception error)
        {
            if (ReferenceEquals(payload, error)) return false;
            if (((FixturePayload)payload).Started) Environment.FailFast("Fixture: copy failed after exception start", error);
            return true;
        }
        public static partial void ReleaseIncomplete(CppException payload)
        { ThrowPayloadHost.CheckRelease(); ((FixturePayload)payload).Release(false); }
        public static partial Exception Rethrow() => throw new Exception("Fixture has no source rethrow");
    }
}
