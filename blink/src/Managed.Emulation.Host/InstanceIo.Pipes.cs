namespace Managed.Emulation.Host;

public sealed partial class InstanceIo
{
    public long PipeBytes => pipes.AllocatedBytes;
    public int PendingPipeOperations => pipes.PendingOperations;
    public HostResult<PipePair> Pipe(int flags = 0)
    {
        lock(sync)
        {
            if (disposed) return Fail<PipePair>(GuestError.BadDescriptor);
            if ((flags & ~(2048|524288)) != 0) return Fail<PipePair>(GuestError.Invalid);
            int read=-1,write=-1;
            for(int fd=0;fd<descriptorLimit;++fd)
                if(!descriptors.ContainsKey(fd)) { if(read<0) read=fd; else {write=fd;break;} }
            if(write<0) return Fail<PipePair>(GuestError.TooManyFiles);
            try {
                descriptors.EnsureCapacity(descriptors.Count+2);
                if((flags&524288)!=0) closeOnExec.EnsureCapacity(closeOnExec.Count+2);
                var pair=pipes.Create();
                if(!pair.Succeeded) return Fail<PipePair>(pair.Error);
                try {
                    var reader=new Description(Kind.Pipe,pair.Value.Read,flags&2048);
                    var writer=new Description(Kind.Pipe,pair.Value.Write,1|(flags&2048));
                    descriptors.Add(read,reader);descriptors.Add(write,writer);
                    if((flags&524288)!=0) {closeOnExec.Add(read);closeOnExec.Add(write);}
                } catch {
                    descriptors.Remove(read);descriptors.Remove(write);
                    closeOnExec.Remove(read);closeOnExec.Remove(write);
                    pipes.Close(pair.Value.Read);pipes.Close(pair.Value.Write);throw;
                }
                return HostResult<PipePair>.Success(new(read,write));
            } catch(OutOfMemoryException) {return Fail<PipePair>(GuestError.NoMemory);}
        }
    }
    private async Task<HostResult<bool>> WaitPipeReadable(int fd,TimeSpan timeout,CancellationToken cancellation)
    {
        long start=System.Diagnostics.Stopwatch.GetTimestamp();
        while(true)
        {
            var remaining=timeout==Timeout.InfiniteTimeSpan ? timeout
                : timeout-System.Diagnostics.Stopwatch.GetElapsedTime(start);
            int milliseconds=timeout==Timeout.InfiniteTimeSpan ? -1
                : (int)Math.Clamp(Math.Ceiling(remaining.TotalMilliseconds),0,int.MaxValue);
            var result=await PollAsync([new(fd,1)],milliseconds,cancellation).ConfigureAwait(false);
            if(!result.Succeeded) return Fail<bool>(result.Error);
            if((result.Value.Events[0]&32)!=0) return Fail<bool>(GuestError.BadDescriptor);
            if(result.Value.Count!=0) return HostResult<bool>.Success(true);
            if(timeout!=Timeout.InfiniteTimeSpan && System.Diagnostics.Stopwatch.GetElapsedTime(start)>=timeout)
                return HostResult<bool>.Success(false);
        }
    }
}
