using System;
using System.Runtime.InteropServices;

namespace Edg.Runtime;

// Physical coroutine ABI. The compiler emits promise construction, await
// protocols and destruction; this helper owns neither C++ object lifetimes
// nor the allocator selected by the promise type.
public static unsafe class Coroutine
{
    private const ulong Live = 0x454447434F524F31;
    [StructLayout(LayoutKind.Sequential)]
    public struct Frame
    {
        public delegate* managed<void*, void*> Resume;
        public delegate* managed<void*, void> Destroy;
        public void* Promise;
        public nuint AllocationSize;
        public int State;
        public int Cleanup;
        internal ulong Magic;
        internal nint RegistryHandle;
        internal Frame* Previous;
        internal Frame* Next;
    }

    // A process-global, immortal noop coroutine has no source owner or heap
    // lifetime. A pinned global array provides a stable NativeAOT-safe address.
    private static readonly byte[] noopStorage = GC.AllocateArray<byte>(sizeof(Frame) + sizeof(void*) + 1, pinned: true);
    private static readonly nint noopAddress = InitializeNoop();
    private static nint InitializeNoop()
    {
        byte* storage = (byte*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(noopStorage));
        Frame* frame = (Frame*)storage;
        frame->Resume = &ResumeNoop; frame->Destroy = &DestroyNoop;
        frame->Promise = storage + sizeof(Frame) + sizeof(void*);
        frame->Magic = Live;
        ((Frame**)frame->Promise)[-1] = frame;
        return (nint)frame;
    }
    private static void* ResumeNoop(void* address) => null;
    private static void DestroyNoop(void* address) { }
    public static void* Noop() => (void*)noopAddress;

    // One rooting handle per owner with live frames, not per resume or await.
    // Abandon runs after the owner's global/TLS destructors and before its
    // native allocation arena is released. Leaked frames do not acquire an
    // invented C++ promise destructor or user operator-delete invocation.
    public sealed class Registry
    {
        private readonly object sync = new();
        internal readonly object? Owner;
        private Frame* head;
        private GCHandle root;
        private bool closed;
        public Registry(object? owner) { Owner = owner; }
        public void Attach(Frame* frame, void* promise, nuint size,
                           delegate* managed<void*, void*> resume,
                           delegate* managed<void*, void> destroy)
        {
            if (frame == null || promise == null || resume == null || destroy == null)
                throw new ArgumentException("Invalid coroutine frame layout");
            lock (sync)
            {
                ObjectDisposedException.ThrowIf(closed, this);
                if (head == null) root = GCHandle.Alloc(this);
                *frame = default;
                frame->Resume = resume; frame->Destroy = destroy;
                frame->Promise = promise; frame->AllocationSize = size;
                frame->Magic = Live; frame->RegistryHandle = GCHandle.ToIntPtr(root);
                frame->Next = head;
                if (head != null) head->Previous = frame;
                head = frame;
                // Compiler reserves this slot immediately before the promise,
                // independently of its alignment or source record layout.
                ((Frame**)promise)[-1] = frame;
            }
        }
        internal void Detach(Frame* frame)
        {
            lock (sync)
            {
                Check(frame);
                if (frame->Previous == null) head = frame->Next;
                else frame->Previous->Next = frame->Next;
                if (frame->Next != null) frame->Next->Previous = frame->Previous;
                frame->Magic = 0; frame->RegistryHandle = 0;
                frame->Previous = null; frame->Next = null;
                if (head == null) root.Free();
            }
        }
        public void Abandon()
        {
            lock (sync)
            {
                if (closed) return;
                closed = true;
                if (head == null) return;
                while (head != null)
                {
                    Frame* next = head->Next;
                    head->Magic = 0; head->RegistryHandle = 0;
                    head->Previous = null; head->Next = null;
                    head = next;
                }
                root.Free();
            }
        }
    }
    private static Frame* Check(void* address)
    {
        var frame = (Frame*)address;
        if (frame == null || frame->Magic != Live)
            Environment.FailFast("Invalid or disposed coroutine frame");
        return frame;
    }
    private static Registry Registration(Frame* frame)
        => (Registry)GCHandle.FromIntPtr(frame->RegistryHandle).Target!;
    public static object? Owner(void* address)
    {
        Frame* frame = Check(address);
        return address == (void*)noopAddress ? null : Registration(frame).Owner;
    }
    public static void Detach(void* address)
    {
        Frame* frame = Check(address);
        Registration(frame).Detach(frame);
    }
    public static bool Done(void* address) => Check(address)->Resume == null;
    public static void* Promise(void* address, nuint alignment, bool fromPromise)
    {
        if (address == null || alignment == 0 || (alignment & (alignment - 1)) != 0)
            Environment.FailFast("Invalid coroutine promise alignment");
        Frame* frame = Check(fromPromise ? ((Frame**)address)[-1] : address);
        if (fromPromise && frame->Promise != address)
            Environment.FailFast("Invalid coroutine promise address");
        return fromPromise ? frame : frame->Promise;
    }
    public static void Resume(void* address)
    {
        for (;;)
        {
            Frame* frame = Check(address);
            var callback = frame->Resume;
            if (callback == null) Environment.FailFast("Cannot resume a completed coroutine");
            // The callback may destroy its own suspended frame. Never read
            // frame again after invoking it. Symmetric transfers are iterative.
            address = callback(frame);
            if (address == null) return;
        }
    }
    public static void Destroy(void* address)
    {
        Frame* frame = Check(address);
        var callback = frame->Destroy;
        if (callback == null) Environment.FailFast("Invalid coroutine destroy entry");
        callback(frame);
    }
}
