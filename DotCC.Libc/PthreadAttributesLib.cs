#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public const int PTHREAD_STACK_MIN = 16384;
    // Unlike Thread's zero/default option, this is an explicit size for both
    // null attributes and initialized attributes. The OS may round it upward.
    public const int DOTCC_PTHREAD_DEFAULT_STACK_SIZE = 1024 * 1024;

    /// <summary>Returns the existing program owner, including the legacy owner
    /// when no explicit binding is active. Capture a RetainLease before moving
    /// this identity to another thread; this never creates another context.</summary>
    public static RuntimeContext CaptureRuntimeContext() => RuntimeState;

    public sealed partial class RuntimeContext
    {
        private PthreadStackPolicy? pthreadStackPolicy;
        private int pthreadAdmissions;

        /// <summary>Opt into a minimum physical managed-thread stack request.
        /// Logical pthread attributes and their validation remain unchanged.
        /// Only one scope may exist and acquisition requires drained workers.
        /// Disposing the scope retires it: already admitted workers and their
        /// descendants keep the policy until the last worker exits, including
        /// TLS destructors. A new scope cannot replace an undrained policy.</summary>
        public IDisposable UseMinimumPthreadStackSize(int minimumBytes)
        {
            if (minimumBytes < PTHREAD_STACK_MIN) throw new ArgumentOutOfRangeException(nameof(minimumBytes));
            var policy = new PthreadStackPolicy(this, minimumBytes);
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                if (pthreadStackPolicy != null || pthreadAdmissions != 0)
                    throw new InvalidOperationException("The pthread stack policy requires a drained context and no existing scope.");
                Retain();
                pthreadStackPolicy = policy;
            }
            return policy;
        }

        internal PthreadAdmission AdmitPthread(int requestedBytes)
        {
            lock (gate)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                var admission = new PthreadAdmission(this, Math.Max(requestedBytes, pthreadStackPolicy?.MinimumBytes ?? 0));
                int count = checked(pthreadAdmissions + 1);
                Retain();
                pthreadAdmissions = count;
                return admission;
            }
        }

        internal void RetirePthreadPolicy(PthreadStackPolicy policy)
        {
            lock (gate)
            {
                if (policy.Retired) return;
                policy.Retired = true;
                ReleaseRetiredPthreadPolicy();
            }
        }

        private void ReleaseRetiredPthreadPolicy()
        {
            if (pthreadAdmissions == 0 && pthreadStackPolicy is { Retired: true })
            {
                pthreadStackPolicy = null;
                Release();
            }
        }

        internal void ReleasePthreadAdmission()
        {
            lock (gate)
            {
                if (pthreadAdmissions <= 0) throw new InvalidOperationException("Unbalanced pthread admission.");
                pthreadAdmissions--;
                ReleaseRetiredPthreadPolicy();
                Release();
            }
        }
    }

    internal sealed class PthreadStackPolicy(RuntimeContext owner, int minimumBytes) : IDisposable
    {
        internal readonly int MinimumBytes = minimumBytes;
        internal bool Retired;
        public void Dispose() => owner.RetirePthreadPolicy(this);
    }

    internal sealed class PthreadAdmission(RuntimeContext owner, int stackSize) : IDisposable
    {
        private RuntimeContext? retained = owner;
        internal readonly int StackSize = stackSize;
        public void Dispose() => Interlocked.Exchange(ref retained, null)?.ReleasePthreadAdmission();
    }

    private sealed class PthreadAttributes
    {
        internal int StackSize = DOTCC_PTHREAD_DEFAULT_STACK_SIZE;
        internal bool Detached;
    }
    private sealed class PthreadAttributeRegistry
    {
        internal readonly Dictionary<int, PthreadAttributes> Attributes = new();
    }
    private static readonly ConditionalWeakTable<RuntimeContext, PthreadAttributeRegistry> pthreadAttributeOwners = new();
    private static long nextPthreadAttribute;

    private static PthreadAttributeRegistry PthreadAttributesFor(RuntimeContext owner) =>
        pthreadAttributeOwners.GetValue(owner, static _ => new());

    public static int pthread_attr_init(int* attribute)
    {
        if (attribute == null) return EINVAL;
        long id = Interlocked.Increment(ref nextPthreadAttribute);
        if (id <= 0 || id > int.MaxValue) return EAGAIN;
        try
        {
            var registry = PthreadAttributesFor(RuntimeState);
            lock (registry) registry.Attributes.Add((int)id, new());
            *attribute = (int)id;
            return 0;
        }
        catch (OutOfMemoryException) { return ENOMEM; }
    }

    public static int pthread_attr_destroy(int* attribute)
    {
        if (attribute == null) return EINVAL;
        var registry = PthreadAttributesFor(RuntimeState);
        lock (registry)
        {
            if (!registry.Attributes.Remove(*attribute)) return EINVAL;
            *attribute = -1;
            return 0;
        }
    }

    public static int pthread_attr_setdetachstate(int* attribute, int state)
    {
        if (attribute == null || state is not (PTHREAD_CREATE_JOINABLE or PTHREAD_CREATE_DETACHED)) return EINVAL;
        var registry = PthreadAttributesFor(RuntimeState);
        lock (registry)
        {
            if (!registry.Attributes.TryGetValue(*attribute, out var value)) return EINVAL;
            value.Detached = state == PTHREAD_CREATE_DETACHED;
            return 0;
        }
    }

    public static int pthread_attr_getdetachstate(int* attribute, int* state)
    {
        if (attribute == null || state == null) return EINVAL;
        var registry = PthreadAttributesFor(RuntimeState);
        lock (registry)
        {
            if (!registry.Attributes.TryGetValue(*attribute, out var value)) return EINVAL;
            *state = value.Detached ? PTHREAD_CREATE_DETACHED : PTHREAD_CREATE_JOINABLE;
            return 0;
        }
    }

    public static int pthread_attr_getstacksize(int* attribute, ulong* size)
    {
        if (attribute == null || size == null) return EINVAL;
        var registry = PthreadAttributesFor(RuntimeState);
        lock (registry)
        {
            if (!registry.Attributes.TryGetValue(*attribute, out var value)) return EINVAL;
            *size = (ulong)value.StackSize;
            return 0;
        }
    }

    public static int pthread_attr_setstacksize(int* attribute, ulong size)
    {
        if (attribute == null || size < PTHREAD_STACK_MIN || size > int.MaxValue) return EINVAL;
        var registry = PthreadAttributesFor(RuntimeState);
        lock (registry)
        {
            if (!registry.Attributes.TryGetValue(*attribute, out var value)) return EINVAL;
            value.StackSize = (int)size;
            return 0;
        }
    }

    private static bool TryPthreadAttributes(RuntimeContext owner, int* attribute, out int stackSize, out bool detached)
    {
        stackSize = DOTCC_PTHREAD_DEFAULT_STACK_SIZE;
        detached = false;
        if (attribute == null) return true;
        var registry = PthreadAttributesFor(owner);
        lock (registry)
        {
            if (!registry.Attributes.TryGetValue(*attribute, out var value)) return false;
            stackSize = value.StackSize;
            detached = value.Detached;
            return true;
        }
    }
}
