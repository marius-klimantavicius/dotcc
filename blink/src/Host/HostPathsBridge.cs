using global::System;
using global::System.Text;
using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    // Allocation requests are bounded independently of the generic C heap.
    private const int PathAllocationLimit = 4097;
    private static unsafe HostResult<string> ReadPrivatePath(byte* path)
    {
        if (path == null) return HostResult<string>.Failure((GuestError)14);
        int length = 0;
        while (length < PathLimit && path[length] != 0) ++length;
        if (length == PathLimit) return HostResult<string>.Failure(GuestError.NameTooLong);
        try { return HostResult<string>.Success(PathEncoding.GetString(new ReadOnlySpan<byte>(path, length))); }
        catch (DecoderFallbackException) { return HostResult<string>.Failure(GuestError.Invalid); }
    }
    private static unsafe byte* PathError(int error) { IoError(error); return null; }
    private static unsafe byte* WritePrivatePath(string path, byte* destination, ulong size, bool realPath)
    {
        byte[] encoded;
        try { encoded = PathEncoding.GetBytes(path); }
        catch (EncoderFallbackException) { return PathError(22); }
        int required = encoded.Length + 1;
        if (required > (realPath ? PathLimit : PathAllocationLimit)) return PathError(36);
        if (destination == null)
        {
            if (size == 0) size = (ulong)required;
            if (size < (ulong)required) return PathError(34); // ERANGE
            if (size > PathAllocationLimit) return PathError(12); // explicit per-allocation budget
            destination = (byte*)Libc.malloc((int)size);
            if (destination == null) return PathError(12);
        }
        else
        {
            if (size == 0) return PathError(22);
            if (size < (ulong)required) return PathError(34);
        }
        encoded.AsSpan().CopyTo(new Span<byte>(destination, encoded.Length));
        destination[encoded.Length] = 0;
        return destination;
    }
    public static unsafe byte* blink_host_getcwd(byte* destination, ulong size)
    {
        try
        {
            if (io == null) return PathError(19);
            var result = io.GetWorkingDirectory();
            return result.Succeeded ? WritePrivatePath(result.Value, destination, size, false) : PathError((int)result.Error);
        }
        catch (Exception error) { IoException(error); return null; }
    }
    public static unsafe byte* blink_host_realpath(byte* path, byte* destination)
    {
        try
        {
            if (io == null) return PathError(19);
            var name = ReadPrivatePath(path);
            if (!name.Succeeded) return PathError((int)name.Error);
            var result = io.CanonicalPath(name.Value);
            return result.Succeeded ? WritePrivatePath(result.Value, destination, destination == null ? 0UL : (ulong)PathLimit, true) : PathError((int)result.Error);
        }
        catch (Exception error) { IoException(error); return null; }
    }
    public static unsafe int blink_host_chdir(byte* path)
    {
        try
        {
            if (io == null) return IoError(19);
            var name = ReadPrivatePath(path);
            return name.Succeeded ? (int)IoResult(io.ChangeDirectory(name.Value)) : IoError((int)name.Error);
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_host_fchdir(int descriptor)
    {
        try { return io == null ? IoError(19) : (int)IoResult(io.ChangeDirectory(descriptor)); }
        catch (Exception error) { return IoException(error); }
    }
}
