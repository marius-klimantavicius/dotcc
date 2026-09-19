namespace Managed.Emulation;
#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static int ResourceError(int error) { Libc.errno = error; return -1; }
    private static int ResourceOwner(out ulong descriptors)
    {
        descriptors = 0;
        if (io == null) return ResourceError(19);
        var result = io.DescriptorCapacity();
        if (!result.Succeeded) return ResourceError((int)result.Error);
        descriptors = (ulong)result.Value;
        return 0;
    }
    private static int ResourceLimit(int resource, out ulong limit)
    {
        limit = 0;
        if (ResourceOwner(out var descriptors) != 0) return -1;
        if ((uint)resource >= 16) return ResourceError(22);
        if (resource == 7) { limit = descriptors; return 0; }
        if (resource is 2 or 9)
        {
            // Private AS/DATA policy: the actual C mapping-owner budget,
            // including its counted records; not OS/process-wide memory usage.
            limit = BlinkHostMemoryLimit();
            return limit != 0 ? 0 : ResourceError(19);
        }
        // Recognized selectors need actual accounting/enforcement contracts.
        // No native process limit or guessed infinity is substituted.
        return ResourceError(95);
    }
    public static unsafe int blink_host_getrlimit(int resource, rlimit* result)
    {
        if (ResourceLimit(resource, out var limit) != 0) return -1;
        if (result == null) return ResourceError(14);
        rlimit value = default;
        value.rlim_cur = limit; value.rlim_max = limit;
        *result = value;
        return 0;
    }
    public static unsafe int blink_host_setrlimit(int resource, rlimit* requested)
    {
        if (ResourceLimit(resource, out var limit) != 0) return -1;
        if (requested == null) return ResourceError(14);
        if (requested->rlim_cur > requested->rlim_max) return ResourceError(22);
        // Idempotent requests are real successes; every requested change is refused.
        return requested->rlim_cur == limit && requested->rlim_max == limit ? 0 : ResourceError(1);
    }
    private static int PriorityTarget(int which, uint who)
    {
        if (ResourceOwner(out _) != 0) return -1;
        if (identity == null) return ResourceError(19);
        if ((uint)which > 2) return ResourceError(22);
        uint target = which == 2 ? identity.UserId : (uint)identity.ProcessId;
        return who == 0 || who == target ? 0 : ResourceError(3);
    }
    public static int blink_host_getpriority(int which, uint who)
        => PriorityTarget(which, who) == 0 ? 0 : -1;
    public static int blink_host_setpriority(int which, uint who, int priority)
    {
        if (PriorityTarget(which, who) != 0) return -1;
        return priority == 0 ? 0 : ResourceError(1);
    }
}
