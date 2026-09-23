using System.Runtime.InteropServices;
using static Managed.Smb.LibSmb2;

namespace Managed.Smb;

public sealed partial class SmbConnection
{
    public sealed class SmbFile : IDisposable, IAsyncDisposable
    {
        private readonly SmbConnection _owner;
        private nint _handle;
        private readonly object _disposeGate = new();
        private Task? _disposeTask;
        internal SmbFile(SmbConnection owner, nint handle) { _owner = owner; _handle = handle; }

        public int Read(byte[] buffer, ulong offset = 0) => _owner.Block(() => ReadAsync(buffer, offset));
        public int Write(byte[] buffer, ulong offset = 0) => _owner.Block(() => WriteAsync(buffer, offset));
        public Task<int> ReadAsync(byte[] buffer, ulong offset = 0, CancellationToken cancellationToken = default)
            => TransferAsync(buffer, offset, false, cancellationToken);
        public Task<int> WriteAsync(byte[] buffer, ulong offset = 0, CancellationToken cancellationToken = default)
            => TransferAsync(buffer, offset, true, cancellationToken);
        private Task<int> TransferAsync(byte[] buffer, ulong offset, bool writing, CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            return _owner.RunOperationAsync(writing ? "write" : "read", op => StartTransfer(op, buffer, offset, writing),
                op => FinishTransfer(op, buffer, writing), token, validate: RequireHandle);
        }
        private unsafe int StartTransfer(Operation op, byte[] buffer, ulong offset, bool writing)
        {
            RequireHandle();
            // Upstream credit calculation subtracts one from count.
            if (buffer.Length == 0)
            {
                op.Finished = true;
                op.Completion.TrySetResult();
                return 0;
            }
            op.Output = op.Allocate(buffer.Length);
            if (writing) Marshal.Copy(buffer, 0, op.Output, buffer.Length);
            return writing
                ? smb2_pwrite_async(_owner.Context, (smb2fh*)_handle, (byte*)op.Output, (uint)buffer.Length, offset, &Complete, op.Token)
                : smb2_pread_async(_owner.Context, (smb2fh*)_handle, (byte*)op.Output, (uint)buffer.Length, offset, &Complete, op.Token);
        }
        private static int FinishTransfer(Operation op, byte[] buffer, bool writing)
        {
            if (!writing && op.Status > 0) Marshal.Copy(op.Output, buffer, 0, op.Status);
            return op.Status;
        }
        public void Flush() => _owner.Block(() => FlushAsync());
        public Task FlushAsync(CancellationToken cancellationToken = default)
            => _owner.SimpleAsync("flush", StartFlush, cancellationToken, validate: RequireHandle);
        private unsafe int StartFlush(Operation op)
        {
            RequireHandle();
            return smb2_fsync_async(_owner.Context, (smb2fh*)_handle, &Complete, op.Token);
        }
        public Metadata Stat() => _owner.Block(() => StatAsync());
        public Task<Metadata> StatAsync(CancellationToken cancellationToken = default)
            => _owner.RunOperationAsync("fstat", StartStat, ReadMetadata, cancellationToken, validate: RequireHandle);
        private unsafe int StartStat(Operation op)
        {
            RequireHandle();
            op.Output = op.Allocate(sizeof(smb2_stat_64));
            return smb2_fstat_async(_owner.Context, (smb2fh*)_handle, (smb2_stat_64*)op.Output, &Complete, op.Token);
        }
        public void Truncate(ulong length) => _owner.Block(() => TruncateAsync(length));
        public Task TruncateAsync(ulong length, CancellationToken cancellationToken = default)
            => _owner.SimpleAsync("truncate", op => StartTruncate(op, length), cancellationToken, validate: RequireHandle);
        private unsafe int StartTruncate(Operation op, ulong length)
        {
            RequireHandle();
            return smb2_ftruncate_async(_owner.Context, (smb2fh*)_handle, length, &Complete, op.Token);
        }
        private void RequireHandle() => ObjectDisposedException.ThrowIf(_handle == 0, this);
        internal unsafe void ReleaseAfterDestroy()
        {
            if (_handle != 0) FreeHandle((smb2fh*)_handle);
            _handle = 0;
        }
        public void Dispose() => _owner.Block(() => DisposeAsync().AsTask());
        public ValueTask DisposeAsync() => new(DisposeAsyncCore(disposing: false));
        internal Task DisposeAsyncCore(bool disposing)
        {
            lock (_disposeGate) return _disposeTask ??= CloseAsync(disposing);
        }
        private async Task CloseAsync(bool disposing)
        {
            if (await _owner.Turn(() => _handle == 0).ConfigureAwait(false)) return;
            try
            {
                // Resource cleanup remains admissible after connection disposal has begun.
                await _owner.SimpleAsync("close", StartClose, default, disposing: true).ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
                if (!await _owner.Turn(() => _handle == 0).ConfigureAwait(false)) throw;
            }
        }
        private unsafe int StartClose(Operation op)
        {
            if (_handle == 0)
            {
                op.Finished = true;
                op.Completion.TrySetResult();
                return 0;
            }
            var handle = (smb2fh*)_handle;
            _handle = 0;
            _owner._files.Remove(this);
            int started = smb2_close_async(_owner.Context, handle, &Complete, op.Token);
            // The upstream callback frees the file handle. Failed submission without
            // a callback leaves it locally owned; context destruction cannot find it.
            if (started < 0 && !op.Finished) FreeHandle(handle);
            return started;
        }
    }
}
