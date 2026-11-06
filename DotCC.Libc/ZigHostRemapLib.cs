#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Source libc mremap for an entire owner-local mapping, with
    /// optional kernel relocation. Fixed replacement and retained aliases are
    /// rejected because they can overwrite foreign managed runtime storage.</summary>
    public static void* __dotcc_zig_c_mremap(void* address, ulong oldLength, ulong newLength, uint flags, void* newAddress)
    {
        void* failed = (void*)(nint)(-1);
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || (flags & ~1U) != 0)
        { errno = ENOTSUP; return failed; }
        ulong oldRounded = RoundMappingLength(oldLength), newRounded = RoundMappingLength(newLength);
        ulong start = (ulong)address;
        if (oldLength == 0 || newLength == 0 || oldRounded < oldLength || newRounded < newLength
            || start > ulong.MaxValue - oldRounded || start % (ulong)Environment.SystemPageSize != 0)
        { errno = EINVAL; return failed; }
        lock (mappingGate)
        {
            int index = posixMappings.FindIndex(mapping => mapping.Start == start && mapping.End == start + oldRounded
                && ReferenceEquals(mapping.Owner, RuntimeContext.Current));
            if (index < 0) { errno = EINVAL; return failed; }
            void* result = SourceMremap(address, oldLength, newLength, flags, newAddress);
            if (result == failed) { errno = Marshal.GetLastPInvokeError(); return failed; }
            posixMappings[index] = new((ulong)result, (ulong)result + newRounded, RuntimeContext.Current);
            return result;
        }
    }

    [DllImport("libc", EntryPoint = "mremap", SetLastError = true)]
    private static extern void* SourceMremap(void* address, ulong oldLength, ulong newLength, uint flags, void* newAddress);
}
