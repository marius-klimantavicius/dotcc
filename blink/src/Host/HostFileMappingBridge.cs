using global::System;
using Managed.Emulation.Host;

namespace Managed.Emulation;
#if BLINK_FULL_CORE
public partial class BlinkCore
#else
public static partial class Blink
#endif
{
    [ThreadStatic] private static InstanceIo.MappingTransaction? mappingTransaction;
    [ThreadStatic] private static InstanceIo? mappingOwner;
    [ThreadStatic] private static int mappingDescriptor;

    public static int blink_io_mapping_begin(int fd)
    {
        try
        {
            if (io == null) return IoError(19);
            if (mappingTransaction != null) return IoError(16);
            var result = io.BeginMapping(fd, ioCancellation);
            if (!result.Succeeded) return IoError((int)result.Error);
            mappingDescriptor = fd;
            mappingOwner = io;
            mappingTransaction = result.Value;
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_mapping_end()
    {
        var transaction = mappingTransaction;
        var owner = mappingOwner;
        mappingTransaction = null;
        mappingOwner = null;
        mappingDescriptor = -1;
        try
        {
            return transaction == null || owner == null ? IoError(9) : (int)IoResult(owner.EndMapping(transaction));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe long blink_io_mapping_pread(int fd, void* destination, ulong length, long offset)
    {
        try
        {
            if (io == null) return IoError(19);
            if (ioCancellation.IsCancellationRequested) return IoError(125);
            if (length > int.MaxValue) return IoError(22);
            if (destination == null && length != 0) return IoError(14);
            var target = new Span<byte>(destination, (int)length);
            return IoResult(mappingTransaction != null && mappingDescriptor == fd
                ? io.MappingReadAt(mappingTransaction, target, offset) : io.ReadAt(fd, target, offset));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_io_mapping_read_at_length(int fd, long* length)
    {
        try
        {
            if (io == null) return IoError(19);
            if (ioCancellation.IsCancellationRequested) return IoError(125);
            if (length == null) return IoError(14);
            var result = mappingTransaction != null && mappingDescriptor == fd
                ? io.MappingReadAtLength(mappingTransaction) : io.ReadAtLength(fd);
            if (!result.Succeeded) return IoError((int)result.Error);
            *length = result.Value;
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_host_memfd_create(byte* name, uint flags)
    {
        try
        {
            if (io == null) return IoError(19);
            if (name == null) return IoError(14);
            int count = 0;
            while (count <= 249 && name[count] != 0) ++count;
            if (count > 249) return IoError(22);
            // Linux names are byte strings; the label is not a pathname and is
            // not retained. Preserve arbitrary bytes without a UTF8 restriction.
            return (int)IoResult(io.CreateMemoryFile(new ReadOnlySpan<byte>(name, count), flags, ioCancellation));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_io_shared_file_page(int fd, long offset, void** address, ulong* lease)
    {
        try
        {
            if (io == null) return IoError(19);
            if (address == null || lease == null) return IoError(14);
            var result = mappingTransaction != null && mappingDescriptor == fd
                ? io.MappingMemoryFilePage(mappingTransaction, offset, ioCancellation)
                : io.AcquireMemoryFilePage(fd, offset, ioCancellation);
            if (!result.Succeeded) return IoError((int)result.Error);
            *address = (void*)result.Value.Address;
            *lease = result.Value.Lease;
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    // These use the lease registry, not the current worker's io. Final
    // memory-owner destruction follows worker I/O unbinding.
    public static int blink_io_shared_file_release(ulong lease)
    {
        try { return (int)IoResult(InstanceIo.ReleaseMemoryFilePage(lease)); }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_shared_file_available(ulong lease)
        => InstanceIo.MemoryFilePageAvailable(lease) ? 1 : 0;
    public static int blink_io_shared_file_readonly(int fd)
    {
        try
        {
            if (io == null) return IoError(19);
            if (ioCancellation.IsCancellationRequested) return IoError(125);
            return (int)IoResult(mappingTransaction != null && mappingDescriptor == fd
                ? io.MappingSharedFileReadOnly(mappingTransaction) : io.SharedFileReadOnly(fd));
        }
        catch (Exception error) { return IoException(error); }
    }
    public static unsafe int blink_io_memory_file_budget(ulong limit, ulong bytes, ulong* token)
    {
        try
        {
            if (io == null) return IoError(19);
            if (token == null) return IoError(14);
            if (limit > long.MaxValue || bytes > long.MaxValue) return IoError(12);
            var result = io.BindMemoryFileBudget((long)limit, (long)bytes);
            if (!result.Succeeded) return IoError((int)result.Error);
            *token = result.Value;
            return 0;
        }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_memory_file_account(ulong token, ulong bytes)
    {
        try { return bytes > long.MaxValue ? IoError(12) : (int)IoResult(InstanceIo.AccountMemoryFileBudget(token, (long)bytes)); }
        catch (Exception error) { return IoException(error); }
    }
    public static int blink_io_memory_file_budget_release(ulong token)
    {
        try { return (int)IoResult(InstanceIo.AccountMemoryFileBudget(token, 0, release: true)); }
        catch (Exception error) { return IoException(error); }
    }
}
