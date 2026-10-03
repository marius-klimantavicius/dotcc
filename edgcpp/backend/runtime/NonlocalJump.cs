using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edg.Runtime;

// Source setjmp/longjmp transport. Establishing the continuation is exclusively
// the compiler's responsibility; this runtime never captures a native stack.
public static unsafe class NonlocalJump
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Buffer
    {
        internal ulong ActivationIdentity;
        internal uint Site;
        internal uint Version;
        internal nint VlaHead;
        internal ulong VlaIdentity;
    }

    // Lives in the generated caller frame. Register/unregister in a try/finally
    // outside that routine's continuation dispatcher. No GC handle or heap
    // allocation is needed for ordinary entry or repeated setjmp calls.
    public struct Activation
    {
        internal Activation* Previous;
        internal ulong Identity;
        internal uint SiteCount;
    }

    public sealed class Transfer : Exception
    {
        public ulong ActivationIdentity { get; }
        public uint Site { get; }
        public int Value { get; }
        internal Vla.Checkpoint Checkpoint { get; }
        internal Transfer(ulong identity, uint site, int value, Vla.Checkpoint checkpoint)
            : base("Translated nonlocal jump")
        {
            ActivationIdentity = identity;
            Site = site;
            Value = value;
            Checkpoint = checkpoint;
        }
    }

    [ThreadStatic] private static Activation* head;
    private static long nextIdentity;

    private static ulong NewIdentity()
    {
        for (;;)
        {
            long previous = Volatile.Read(ref nextIdentity);
            if (previous == long.MaxValue)
                Environment.FailFast("Nonlocal jump activation identities exhausted");
            long next = previous + 1;
            if (Interlocked.CompareExchange(ref nextIdentity, next, previous) == previous)
                return (ulong)next;
        }
    }

    public static void Enter(Activation* activation, uint siteCount)
    {
        if (activation == null || activation->Identity != 0 || siteCount == 0)
            Environment.FailFast("Invalid nonlocal jump activation entry");
        activation->Identity = NewIdentity();
        activation->SiteCount = siteCount;
        activation->Previous = head;
        head = activation;
    }

    public static void Leave(Activation* activation)
    {
        if (activation == null || head != activation || activation->Identity == 0)
            Environment.FailFast("Invalid nonlocal jump activation exit");
        head = activation->Previous;
        *activation = default;
    }

    public static int Arm(Activation* activation, void* storage, uint site)
    {
        if (activation == null || head != activation || activation->Identity == 0 ||
            storage == null || site == 0 || site > activation->SiteCount)
            Environment.FailFast("Invalid setjmp continuation");
        var buffer = (Buffer*)storage;
        buffer->ActivationIdentity = activation->Identity;
        buffer->Site = site;
        buffer->Version = 2;
        var checkpoint = Vla.Capture();
        buffer->VlaHead = checkpoint.Head;
        buffer->VlaIdentity = checkpoint.Identity;
        return 0;
    }

    public static bool Matches(Transfer transfer, Activation* activation)
        => activation != null && activation->Identity != 0 &&
           transfer.ActivationIdentity == activation->Identity;

    public static int Resume(Transfer transfer, Activation* activation)
    {
        if (!Matches(transfer, activation)) Environment.FailFast("Wrong nonlocal jump resume activation");
        Vla.Restore(transfer.Checkpoint);
        return transfer.Value;
    }

    [DoesNotReturn]
    public static void LongJump(void* storage, int value)
    {
        if (storage == null) Environment.FailFast("Null longjmp buffer");
        Buffer target = *(Buffer*)storage;
        if (target.ActivationIdentity == 0 || target.Site == 0 || target.Version != 2)
            Environment.FailFast("Unarmed or corrupted longjmp buffer");
        Activation* destination = head;
        while (destination != null && destination->Identity != target.ActivationIdentity)
            destination = destination->Previous;
        if (destination == null)
            Environment.FailFast("longjmp target is inactive or belongs to another thread");
        if (target.Site > destination->SiteCount)
            Environment.FailFast("Invalid longjmp continuation site");
        var checkpoint = new Vla.Checkpoint(target.VlaHead, target.VlaIdentity);
        Vla.Validate(checkpoint);
        Transfer transfer;
        try { transfer = new Transfer(target.ActivationIdentity, target.Site, value == 0 ? 1 : value, checkpoint); }
        catch (OutOfMemoryException error)
        {
            Environment.FailFast("Unable to allocate nonlocal jump transport", error);
            throw;
        }
        throw transfer;
    }
}
