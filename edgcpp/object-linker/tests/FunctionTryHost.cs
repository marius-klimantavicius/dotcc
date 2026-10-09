// Fixture-only provider; production C++ exception dispatch remains separate.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tests.FunctionTry;

internal static unsafe class FunctionTryHost
{
    internal static int Selected, LivePayloads, Releases;
    internal static readonly List<int> Trace = [];
    internal static void AssertHandlerExit() {}
    internal static int Mode;
    public static int Main()
    {
        global::Edg.Runtime.ObjectProgram.InitializeLibrary();
        for(Mode=0;Mode!=6;++Mode) foreach(int failure in Mode<4 ? new[]{0,1,2,3,7,10,21,22} : Mode==4 ? new[]{0,3,8} : new[]{0})
        {
            if(Mode==3 && (failure==21 || failure==22)) continue;
            Selected=failure;Trace.Clear();
            Trace.Add(400+Core.function_try_case(Mode));
            if(LivePayloads!=0 || global::Edg.Runtime.FixtureCatch.Frames.Count!=0) throw new Exception("Leaked payload or frame");
            Console.WriteLine(Mode+":"+failure+" "+string.Join(" ",Trace));
        }
        return 0;
    }

}
namespace Edg.Runtime
{
    public static unsafe partial class LibC
    {
        public static partial int current_mode() => FunctionTryHost.Mode;
        public static partial void record_event(int argument0) => FunctionTryHost.Trace.Add(argument0);
        public static partial int should_fail(int argument0) => argument0 == FunctionTryHost.Selected ? 1 : 0;
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
            ++FunctionTryHost.LivePayloads;
        }
        public override void* ObjectAddress => memory;
        internal void ReleaseIfUnused()
        {
            if (Handlers != 0 || Propagating) return;
            if (memory == null) throw new Exception("Double exception release");
            if (Complete && destructor != null) destructor(memory);
            NativeMemory.Free(memory); memory = null; --FunctionTryHost.LivePayloads; ++FunctionTryHost.Releases;
        }
    }
    internal sealed unsafe class FixtureCatch(Exception error) : CppTypedCatch
    {
        [ThreadStatic] private static Stack<FixtureCatch>? frames;
        internal static Stack<FixtureCatch> Frames => frames ??= new();
        internal readonly Exception Error = error;
        internal bool IsActive = true, Caught;
        public override bool Active => IsActive;
        internal void* Address;
        internal void* PointerCell;
        public override void* ObjectAddress => Address;
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
        private static bool Match(FixturePayload payload, CppCatchType type, out nuint offset)
        {
            offset=0;
            var from=payload.Type;
            if(from.IdentityKey==type.IdentityKey) return true;
            var fq=from.PointerQualifiers.Span; var tq=type.PointerQualifiers.Span;
            if(fq.Length!=tq.Length) return false;
            for(int i=0;i<fq.Length;++i)
            {
                if((fq[i]&~tq[i])!=0) return false;
                if(fq[i]!=tq[i]) for(int j=0;j<i;++j) if((tq[j]&1)==0) return false;
            }
            if(from.LeafIdentityKey==type.LeafIdentityKey) return true;
            if(fq.Length==1 && type.LeafKind=="void") return true;
            if(fq.Length>1 || from.LeafKind!="record" || type.LeafKind!="record") return false;
            foreach(var candidate in from.Bases.Span)
                if(candidate.IdentityKey==type.LeafIdentityKey && candidate.IsPublic && !candidate.IsAmbiguous)
                { offset=candidate.ByteOffset;return true; }
            return false;
        }
        public static partial bool FilterTypedCatch(Exception error, CppCatchType type)
            => error is FixturePayload payload && !ObjectProgram.IsRuntimeTransfer(error) && Match(payload,type,out _);
        public static partial CppTypedCatch EnterTypedCatch(Exception error, CppCatchType type)
        {
            var payload=(FixturePayload)error;
            if(!Match(payload,type,out nuint offset)) Environment.FailFast("Fixture: selected match changed");
            var frame=(FixtureCatch)EnterCatch(error);
            if(type.PointerQualifiers.Length!=0)
            {
                frame.PointerCell=NativeMemory.Alloc((nuint)sizeof(void*));
                void* pointer=*(void**)payload.ObjectAddress;
                *(void**)frame.PointerCell=pointer==null?null:(byte*)pointer+offset;
                frame.Address=frame.PointerCell;
            }
            else frame.Address=(byte*)payload.ObjectAddress+offset;
            return frame;
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
            NativeMemory.Free(f.PointerCell);
            if (f.Error is FixturePayload p)
            {
                --p.Handlers;
                // Only the original class payload owns this function's guard;
                // a nested scalar handler may end while that guard remains live.
                if (p.Type.LeafKind == "record" && p.Handlers == 0) FunctionTryHost.AssertHandlerExit();
                p.ReleaseIfUnused();
            }
        }
    }
}
