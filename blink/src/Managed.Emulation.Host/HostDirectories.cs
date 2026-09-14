using System.Runtime.InteropServices;
using System.Text;

namespace Managed.Emulation.Host;

/// <summary>Bounded private directory streams. Dispose after all C borrowers
/// stop; token addresses remain reserved until disposal to prevent reuse.</summary>
public sealed unsafe class HostDirectories : IDisposable
{
    public const int MaximumStreams=32, MaximumRegistrations=128, MaximumEntries=4096, MaximumNameBytes=262144;
    public const int RecordBytes=272;
    private sealed class StreamState(InstanceIo.DirectoryLease lease,nuint record,int bytes)
    {
        internal readonly InstanceIo.DirectoryLease Lease=lease;
        internal nuint Record=record;
        internal readonly int NameBytes=bytes;
        internal int Position;
        internal bool Closed;
    }
    private readonly object sync=new();
    private readonly InstanceIo io;
    private readonly Dictionary<nuint,StreamState> streams=new();
    private int active,entries,nameBytes;
    private bool disposed;
    public HostDirectories(InstanceIo owner) { ArgumentNullException.ThrowIfNull(owner);io=owner; }
    public int ActiveStreams { get { lock(sync)return active; } }
    public int SnapshotNameBytes { get { lock(sync)return nameBytes; } }
    public HostResult<nuint> Open(string path)
    {
        lock(sync)
        {
            if(disposed)return Fail<nuint>(GuestError.BadDescriptor);
            var fd=io.OpenFile(path,FileAccessMode.Read,allowDirectory:true,requireDirectory:true,closeOnExecFlag:true);
            if(!fd.Succeeded)return Fail<nuint>(fd.Error);
            try { var result=Open(fd.Value);if(!result.Succeeded)io.Close(fd.Value);return result; }
            catch { io.Close(fd.Value);throw; }
        }
    }
    public HostResult<nuint> Open(int fd)
    {
        lock(sync)
        {
            if(disposed)return Fail<nuint>(GuestError.BadDescriptor);
            if(active==MaximumStreams || streams.Count==MaximumRegistrations)return Fail<nuint>(GuestError.TooManyFiles);
            // Allocate all C storage before ownership transfer. Any subsequent
            // failure drops the internal lease without closing the caller's fd.
            void* token=NativeMemory.AllocZeroed(8);
            void* record=null;InstanceIo.DirectoryLease? lease=null;
            try
            {
                if(token==null)throw new OutOfMemoryException();
                record=NativeMemory.AllocZeroed(RecordBytes);
                if(record==null)throw new OutOfMemoryException();
                var acquired=io.AcquireDirectory(fd,MaximumEntries-entries,MaximumNameBytes-nameBytes);
                if(!acquired.Succeeded)return Fail<nuint>(acquired.Error);
                lease=acquired.Value;
                int bytes=0;foreach(var entry in lease.Entries)bytes+=Encoding.UTF8.GetByteCount(entry.Name)+1;
                var state=new StreamState(lease,(nuint)record,bytes);
                streams.Add((nuint)token,state);
                *(ulong*)token=(ulong)streams.Count;
                ++active;entries+=lease.Entries.Count;nameBytes+=bytes;
                nuint result=(nuint)token;token=null;record=null;lease=null;
                return HostResult<nuint>.Success(result);
            }
            finally
            {
                if(lease!=null)io.ReleaseDirectory(lease,false);
                NativeMemory.Free(record);NativeMemory.Free(token);
            }
        }
    }
    // Only the measured campaign d_name-first layout is written here. Native
    // system dirent layout is separate and never passed to this implementation.
    public HostResult<nuint> Read(nuint token)
    {
        lock(sync)
        {
            var found=Find(token);if(!found.Succeeded)return Fail<nuint>(found.Error);
            var state=found.Value;
            if(state.Position==state.Lease.Entries.Count)return HostResult<nuint>.Success(0);
            var entry=state.Lease.Entries[state.Position];
            byte* record=(byte*)state.Record;
            NativeMemory.Clear(record,RecordBytes);
            int length=Encoding.UTF8.GetBytes(entry.Name,new Span<byte>(record,255));
            record[length]=0;*(ulong*)(record+256)=entry.Inode;record[264]=entry.Type;
            ++state.Position;return HostResult<nuint>.Success(state.Record);
        }
    }
    public HostResult<long> Tell(nuint token)
    { lock(sync){var found=Find(token);return found.Succeeded?HostResult<long>.Success(found.Value.Position):Fail<long>(found.Error);} }
    public HostResult<int> Seek(nuint token,long position)
    {
        lock(sync)
        {
            var found=Find(token);if(!found.Succeeded)return Fail<int>(found.Error);
            if(position<0 || position>found.Value.Lease.Entries.Count)return Fail<int>(GuestError.Invalid);
            found.Value.Position=(int)position;return HostResult<int>.Success(0);
        }
    }
    public HostResult<int> Descriptor(nuint token)
    { lock(sync){var found=Find(token);return found.Succeeded?io.DirectoryDescriptor(found.Value.Lease):Fail<int>(found.Error);} }
    public HostResult<int> Close(nuint token)
    {
        lock(sync)
        {
            if(disposed || !streams.TryGetValue(token,out var state) || state.Closed)return Fail<int>(GuestError.BadDescriptor);
            state.Closed=true;--active;entries-=state.Lease.Entries.Count;nameBytes-=state.NameBytes;
            var result=io.ReleaseDirectory(state.Lease);
            NativeMemory.Free((void*)state.Record);state.Record=0;
            return result;
        }
    }
    private HostResult<StreamState> Find(nuint token)
    {
        if(disposed || !streams.TryGetValue(token,out var state) || state.Closed)return Fail<StreamState>(GuestError.BadDescriptor);
        var fd=io.DirectoryDescriptor(state.Lease);
        return fd.Succeeded?HostResult<StreamState>.Success(state):Fail<StreamState>(fd.Error);
    }
    public void Dispose()
    {
        lock(sync)
        {
            if(disposed)return;
            foreach(var item in streams){if(!item.Value.Closed)Close(item.Key);NativeMemory.Free((void*)item.Key);}
            streams.Clear();disposed=true;
        }
    }
    private static HostResult<T> Fail<T>(GuestError error)=>HostResult<T>.Failure(error);
}
