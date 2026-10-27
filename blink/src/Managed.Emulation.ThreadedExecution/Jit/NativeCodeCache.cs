using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace Managed.Emulation.Execution;

// One immutable allocation belongs to one execution owner's bounded cache. The
// cache charges AllocatedBytes (including page slack) and its managed metadata,
// and disposes only after every possible native caller has returned.
internal unsafe sealed class NativeCodeAllocation : IDisposable
{
    private nint entry;
    private readonly nuint length;
    private readonly bool windows;
    public nint Entry => Volatile.Read(ref entry);
    public long AllocatedBytes => checked((long)length);

    private NativeCodeAllocation(nint entry, nuint length, bool windows)
    { this.entry = entry; this.length = length; this.windows = windows; }

    public static bool TryCreate(ReadOnlySpan<byte> code, out NativeCodeAllocation? allocation, out string reason)
    {
        allocation = null;
        reason = "";
        // Opt-in/qualification is checked by the owner; never bypass an actual
        // mitigation denial even when it explicitly allows unverified Windows.
        string? unavailable = NativeCodePlatform.UnavailableReason(allowUnverifiedWindows: true);
        if (unavailable != null) { reason = unavailable; return false; }
        if (code.IsEmpty || code.Length > 1 << 20) { reason = "Invalid native block size"; return false; }
        nuint page = (nuint)Environment.SystemPageSize;
        nuint size = checked(((nuint)code.Length + page - 1) / page * page);
        bool win = OperatingSystem.IsWindows();
        nint address = win ? VirtualAlloc(0, size, 0x3000, 0x04) : mmap(0, size, 3, 0x22, -1, 0);
        if (win ? address == 0 : address == -1)
        { reason = $"{(win ? "VirtualAlloc" : "mmap")} failed ({Marshal.GetLastPInvokeError()})"; return false; }
        bool published = false;
        try
        {
            code.CopyTo(new Span<byte>((void*)address, code.Length));
            if (win)
            {
                // RW -> RX, never RWX. Default executable-page CFG treatment
                // is not used to bypass policy: guarded profiles fail above.
                if (!VirtualProtect(address, size, 0x20, out _))
                { reason = $"VirtualProtect RX failed ({Marshal.GetLastPInvokeError()})"; return false; }
                if (!FlushInstructionCache(GetCurrentProcess(), address, size))
                { reason = $"FlushInstructionCache failed ({Marshal.GetLastPInvokeError()})"; return false; }
            }
            else if (mprotect(address, size, 5) != 0)
            { reason = $"mprotect RX failed ({Marshal.GetLastPInvokeError()})"; return false; }
            // x86 instruction/data caches are coherent. No Linux cache flush
            // shim is needed for bytes copied before this new mapping is used.
            allocation = new NativeCodeAllocation(address, size, win);
            published = true;
            return true;
        }
        finally
        {
            if (!published) Release(address, size, win);
        }
    }

    public void Dispose()
    {
        lock (this)
        {
            if (entry == 0) return;
            Release(entry, length, windows);
            Volatile.Write(ref entry, 0);
            GC.SuppressFinalize(this);
        }
    }

    ~NativeCodeAllocation()
    {
        // The cache roots live blocks across synchronous calls. Finalization
        // is a last resort for abandoned owners, not an eviction mechanism.
        if (entry != 0)
        {
            if (windows) VirtualFree(entry, 0, 0x8000);
            else munmap(entry, length);
        }
    }

    private static void Release(nint address, nuint size, bool win)
    {
        if (win ? !VirtualFree(address, 0, 0x8000) : munmap(address, size) != 0)
            throw new Win32Exception(Marshal.GetLastPInvokeError(), "Release native code mapping");
    }

    [DllImport("libc", SetLastError = true)] private static extern nint mmap(nint address, nuint size, int prot, int flags, int fd, nint offset);
    [DllImport("libc", SetLastError = true)] private static extern int mprotect(nint address, nuint size, int prot);
    [DllImport("libc", SetLastError = true)] private static extern int munmap(nint address, nuint size);
    [DllImport("kernel32", SetLastError = true)] private static extern nint VirtualAlloc(nint address, nuint size, uint allocationType, uint protection);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualProtect(nint address, nuint size, uint protection, out uint oldProtection);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool VirtualFree(nint address, nuint size, uint freeType);
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushInstructionCache(nint process, nint address, nuint size);
    [DllImport("kernel32")] private static extern nint GetCurrentProcess();
}

internal static class NativeCodePlatform
{
    public static string? UnavailableReason(bool allowUnverifiedWindows)
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64) return "Native backend requires x64";
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows()) return "Native backend requires Linux or Windows";
        if (!X86Base.IsSupported || (uint)X86Base.CpuId(unchecked((int)0x80000000), 0).Eax < 0x80000001u ||
            (X86Base.CpuId(unchecked((int)0x80000001), 0).Ecx & 1) == 0)
            return "Native backend requires LAHF/SAHF in 64-bit mode";
        try
        {
            if (OperatingSystem.IsLinux())
            {
                // A read mapping must not implicitly acquire execute access.
                int personality = GetPersonality(0xffffffff);
                if (personality < 0) return $"Cannot query Linux personality ({Marshal.GetLastPInvokeError()})";
                return (personality & 0x0400000) != 0 ? "Linux READ_IMPLIES_EXEC prevents W^X publication" : null;
            }
            if (!allowUnverifiedWindows) return "Windows x64 native backend is unverified; explicit opt-in required";
            nint process = GetCurrentProcess();
            if (!GetProcessMitigationPolicy(process, 2, out uint dynamicCode, 4)) return PolicyError("dynamic code");
            if ((dynamicCode & 1) != 0) return "Windows prohibits dynamic code";
            if (!GetProcessMitigationPolicy(process, 7, out uint cfg, 4)) return PolicyError("CFG");
            // CFG/XFG and hardware stack protection need a qualified deployment
            // profile. Do not disable mitigations or opt a thread out of them.
            // Include EnableXfgAuditMode (bit 4) and fail closed on future bits.
            if (cfg != 0) return "Windows CFG/XFG profile is not qualified";
            if (!GetProcessMitigationPolicy(process, 15, out uint shadowStack, 4)) return PolicyError("shadow stack");
            if (shadowStack != 0) return "Windows hardware stack protection profile is not qualified";
            return null;
        }
        catch (Exception error) when (error is DllNotFoundException or EntryPointNotFoundException)
        { return $"Native capability query unavailable: {error.GetType().Name}"; }
    }

    private static string PolicyError(string name) => $"Cannot query Windows {name} policy ({Marshal.GetLastPInvokeError()})";
    [DllImport("libc", EntryPoint = "personality", SetLastError = true)] private static extern int GetPersonality(nuint value);
    [DllImport("kernel32")] private static extern nint GetCurrentProcess();
    [DllImport("kernel32", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetProcessMitigationPolicy(nint process, int policy, out uint flags, nuint size);
}
