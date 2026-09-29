using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Managed.Networking.Lwip;

namespace Managed.Networking;

/// <summary>Owns one original TFTP client or server, bounded copied file storage and callback handles. Only one transfer is active per stack, as upstream requires.</summary>
public sealed unsafe class LwipTftpService:IDisposable
{
    sealed class Transfer
    {
        internal readonly string Name;
        internal readonly bool Receiving,Client;
        internal readonly byte[] Source;
        internal readonly MemoryStream Destination=new();
        internal readonly TaskCompletionSource<byte[]> Completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal GCHandle Root;
        internal int Position;
        internal bool FinalBlock;
        internal Exception? Error;
        internal Transfer(string name,bool receiving,bool client,byte[] source)
        {Name=name;Receiving=receiving;Client=client;Source=source;}
    }
    static readonly ConditionalWeakTable<Lwip,LwipTftpService> Owners=new();
    readonly LwipStack stack;
    readonly bool server;
    readonly int maximumFileBytes,maximumStoredBytes,maximumFiles;
    readonly Dictionary<string,byte[]> files=new(StringComparer.Ordinal);
    readonly Dictionary<nint,Transfer> handles=new();
    nint context;
    int storedBytes;
    bool claimed,registered,initialized,closing;
    volatile bool disposed;
    Exception? callbackError;
    long completedTransfers,failedTransfers,canceledTransfers;
    LwipTftpService(LwipStack stack,bool server,int maximumFileBytes,int maximumStoredBytes,int maximumFiles)
    {this.stack=stack;this.server=server;this.maximumFileBytes=maximumFileBytes;this.maximumStoredBytes=maximumStoredBytes;this.maximumFiles=maximumFiles;}

    /// <summary>Limits stored payload bytes and file count separately. One active transfer and copied read snapshots require additional bounded transient storage.</summary>
    public static LwipTftpService StartServer(LwipStack stack,int maximumFileBytes=1024*1024,int maximumStoredBytes=4*1024*1024,int maximumFiles=1024)
        =>Start(stack,true,maximumFileBytes,maximumStoredBytes,maximumFiles);
    public static LwipTftpService StartClient(LwipStack stack,int maximumTransferBytes=1024*1024)
        =>Start(stack,false,maximumTransferBytes,maximumTransferBytes,0);
    static LwipTftpService Start(LwipStack stack,bool server,int maximumFileBytes,int maximumStoredBytes,int maximumFiles)
    {
        ArgumentNullException.ThrowIfNull(stack);
        if(maximumFileBytes<0||maximumStoredBytes<maximumFileBytes)throw new ArgumentOutOfRangeException(nameof(maximumFileBytes));
        if(server&&(maximumFiles<1||maximumFiles>1_000_000))throw new ArgumentOutOfRangeException(nameof(maximumFiles));
        return stack.StartInitializedService(s=>new LwipTftpService(s,server,maximumFileBytes,maximumStoredBytes,maximumFiles),service=>service.Initialize());
    }
    void Initialize()=>stack.RunServiceBoundary(()=>stack.Invoke(core=>
    {
        LwipUdpApplications.ClaimTftp(core,this);claimed=true;
        Owners.Add(core,this);registered=true;
        context=(nint)NativeMemory.AllocZeroed((nuint)sizeof(tftp_context));
        if(context==0)throw new OutOfMemoryException();
        var callbacks=(tftp_context*)context;
        callbacks->open=&Open;callbacks->close=&Close;callbacks->read=&Read;callbacks->write=&Write;callbacks->error=&Error;
        StartOriginal(core);
    }));
    void StartOriginal(Lwip core)
    {
        int result=core.lwip_owned_tftp_start((byte)(server?1:2),(tftp_context*)context);
        if(result!=0)throw new IOException($"Original TFTP initialization failed ({result}).");
        initialized=true;
    }
    static string Filename(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if(name.Length==0||name.Length>TFTP_MAX_FILENAME_LEN)throw new ArgumentException("Filename exceeds the selected original TFTP limit.",nameof(name));
        foreach(char value in name)if(value<0x20||value>0x7e)throw new ArgumentException("TFTP filenames must contain printable ASCII without NUL.",nameof(name));
        return name;
    }
    void Boundary(Action<Lwip> action)=>stack.RunServiceBoundary(()=>
    {
        ObjectDisposedException.ThrowIf(closing||disposed,this);
        stack.Invoke(action);
    });
    /// <summary>Replace a server's memory-backed file with a copied payload. Active reads retain their original snapshot.</summary>
    public void SetFile(string name,ReadOnlySpan<byte> bytes)
    {
        name=Filename(name);if(bytes.Length>maximumFileBytes)throw new ArgumentOutOfRangeException(nameof(bytes));byte[] copy=bytes.ToArray();
        Boundary(_=>
        {
            if(!server)throw new InvalidOperationException("Only the server exposes stored files.");
            if(!files.ContainsKey(name)&&files.Count>=maximumFiles)throw new IOException("TFTP file-count limit reached.");
            int previous=files.TryGetValue(name,out var old)?old.Length:0;
            if((long)storedBytes-previous+copy.Length>maximumStoredBytes)throw new IOException("TFTP storage limit reached.");
            files[name]=copy;storedBytes=storedBytes-previous+copy.Length;
        });
    }
    public byte[] GetFile(string name)
    {
        name=Filename(name);byte[]? result=null;
        Boundary(_=>{if(!server)throw new InvalidOperationException("Only the server exposes stored files.");result=files.TryGetValue(name,out var bytes)?(byte[])bytes.Clone():throw new FileNotFoundException("No such TFTP file.",name);});
        return result!;
    }
    public bool RemoveFile(string name)
    {
        name=Filename(name);bool removed=false;
        Boundary(_=>{if(!server)throw new InvalidOperationException("Only the server exposes stored files.");if(files.Remove(name,out var bytes)){storedBytes-=bytes.Length;removed=true;}});
        return removed;
    }
    public int ActiveTransfers {get{int count=0;Boundary(_=>count=handles.Count);return count;}}
    public long CompletedTransfers {get{long count=0;Boundary(_=>count=completedTransfers);return count;}}
    public long FailedTransfers {get{long count=0;Boundary(_=>count=failedTransfers);return count;}}
    public long CanceledTransfers {get{long count=0;Boundary(_=>count=canceledTransfers);return count;}}
    public Task<byte[]> DownloadAsync(LwipEndpoint server,string filename)=>Begin(server,filename,true,Array.Empty<byte>());
    public Task UploadAsync(LwipEndpoint server,string filename,ReadOnlyMemory<byte> bytes)
    {
        if(bytes.Length>maximumFileBytes)throw new ArgumentOutOfRangeException(nameof(bytes));
        return Begin(server,filename,false,bytes.ToArray());
    }
    Task<byte[]> Begin(LwipEndpoint destination,string filename,bool receive,byte[] source)
    {
        ArgumentNullException.ThrowIfNull(destination);filename=Filename(filename);
        Task<byte[]>? task=null;
        Boundary(core=>
        {
            if(server)throw new InvalidOperationException("This service is an original TFTP server.");
            if(handles.Count!=0)throw new InvalidOperationException("The original TFTP client supports one active transfer.");
            var transfer=new Transfer(filename,receive,true,source);nint handle=Adopt(transfer);task=transfer.Completion.Task;
            try
            {
                ip_addr address;destination.WriteIp(&address);
                fixed(byte* name=Encoding.ASCII.GetBytes(filename+"\0"))
                {
                    int result=receive?core.tftp_get((void*)handle,&address,destination.Port,name,tftp_transfer_mode.TFTP_MODE_OCTET)
                        :core.tftp_put((void*)handle,&address,destination.Port,name,tftp_transfer_mode.TFTP_MODE_OCTET);
                    if(result!=0)throw new IOException($"Original TFTP request submission failed ({result}).");
                }
            }
            catch(Exception failure)
            {
                transfer.Error=failure;
                // Validated entries retain handle before sending. Original stop
                // releases it even when request submission fails synchronously.
                core.lwip_owned_tftp_stop();initialized=false;
                if(handles.ContainsKey(handle))ReleaseUnsubmitted(transfer,handle,failure);
                StartOriginal(core);
            }
        });
        return task!;
    }
    nint Adopt(Transfer transfer)
    {
        handles.EnsureCapacity(handles.Count+1);
        transfer.Root=GCHandle.Alloc(transfer);nint handle=GCHandle.ToIntPtr(transfer.Root);
        try{handles.Add(handle,transfer);return handle;}
        catch{transfer.Root.Free();transfer.Destination.Dispose();throw;}
    }
    void ReleaseUnsubmitted(Transfer transfer,nint handle,Exception failure)
    {
        handles.Remove(handle);if(transfer.Root.IsAllocated)transfer.Root.Free();transfer.Destination.Dispose();transfer.Completion.TrySetException(failure);
    }
    static bool Find(Lwip owner,void* handle,out LwipTftpService service,out Transfer transfer)
    {
        transfer=null!;
        if(!Owners.TryGetValue(owner,out service!))return false;
        if(service.handles.TryGetValue((nint)handle,out transfer!))return true;
        service.callbackError??=new InvalidOperationException("Original TFTP callback references an unregistered storage handle.");return false;
    }
    static void* Open(Lwip owner,byte* name,byte* mode,byte write)
    {
        if(!Owners.TryGetValue(owner,out var service))return null;
        try
        {
            if(service.closing||!service.server||service.handles.Count!=0)return null;
            if(Marshal.PtrToStringUTF8((nint)mode)!="octet")return null;
            string filename=Filename(Marshal.PtrToStringUTF8((nint)name)!);
            if(write!=0&&!service.files.ContainsKey(filename)&&service.files.Count>=service.maximumFiles)return null;
            byte[] source=Array.Empty<byte>();if(write==0&&!service.files.TryGetValue(filename,out source!))return null;
            return(void*)service.Adopt(new Transfer(filename,write!=0,false,source));
        }
        catch(Exception failure){service.callbackError=failure;return null;}
    }
    static int Read(Lwip owner,void* handle,void* buffer,int capacity)
    {
        if(!Find(owner,handle,out var service,out var transfer))return -1;
        try
        {
            if(transfer.Receiving||capacity<0)throw new InvalidOperationException("TFTP read callback mode/capacity is invalid.");
            int count=Math.Min(capacity,transfer.Source.Length-transfer.Position);
            transfer.Source.AsSpan(transfer.Position,count).CopyTo(new Span<byte>(buffer,count));transfer.Position+=count;
            transfer.FinalBlock=count<512;return count;
        }
        catch(Exception failure){transfer.Error=failure;service.callbackError=failure;return -1;}
    }
    static int Write(Lwip owner,void* handle,pbuf* packet)
    {
        if(!Find(owner,handle,out var service,out var transfer))return -1;
        try
        {
            if(!transfer.Receiving)throw new InvalidOperationException("TFTP write callback for a sending handle.");
            long length=transfer.Destination.Length+packet->tot_len;
            if(length>service.maximumFileBytes)throw new IOException("TFTP transfer limit reached.");
            if(service.server)
            {
                if(!service.files.ContainsKey(transfer.Name)&&service.files.Count>=service.maximumFiles)throw new IOException("TFTP file-count limit reached.");
                int previous=service.files.TryGetValue(transfer.Name,out var old)?old.Length:0;
                if(service.storedBytes-previous+length>service.maximumStoredBytes)throw new IOException("TFTP storage limit reached.");
            }
            for(var part=packet;part!=null;part=part->next)transfer.Destination.Write(new ReadOnlySpan<byte>(part->payload,part->len));
            transfer.FinalBlock=packet->tot_len<512;return 0;
        }
        catch(Exception failure){transfer.Error=failure;service.callbackError=failure;return -1;}
        // Original receive owns/frees packet, including callback failure paths.
    }
    static void Error(Lwip owner,void* handle,int error,byte* message,int length)
    {
        if(!Find(owner,handle,out var service,out var transfer))return;
        try
        {
            // Upstream forwards sbuf[1] without ntohs. Normalize that copied
            // network-order word for consumer diagnostics; do not change parsing.
            ushort code=(ushort)error;if(BitConverter.IsLittleEndian)code=BinaryPrimitives.ReverseEndianness(code);
            transfer.Error=new IOException($"Original TFTP error {code}: {Encoding.ASCII.GetString(new ReadOnlySpan<byte>(message,length))}");
        }
        catch(Exception failure){transfer.Error=failure;service.callbackError=failure;}
    }
    static void Close(Lwip owner,void* handle)
    {
        if(!Find(owner,handle,out var service,out var transfer))return;
        Exception? failure=transfer.Error;bool canceled=false;byte[] result=Array.Empty<byte>();
        try
        {
            uint reason=owner.lwip_owned_tftp_close_context();
            canceled=service.closing||reason==256;
            if(failure==null&&!canceled)
            {
                bool complete=transfer.FinalBlock&&reason==(transfer.Receiving?3U:4U);
                if(!complete)failure=new TimeoutException("Original TFTP closed before a complete final DATA/ACK exchange.");
                else if(transfer.Receiving)
                {
                    result=transfer.Destination.ToArray();
                    if(service.server)
                    {
                        if(!service.files.ContainsKey(transfer.Name)&&service.files.Count>=service.maximumFiles)throw new IOException("TFTP file-count limit reached.");
                        int previous=service.files.TryGetValue(transfer.Name,out var old)?old.Length:0;
                        service.files[transfer.Name]=result;service.storedBytes=service.storedBytes-previous+result.Length;
                    }
                }
            }
        }
        catch(Exception error){failure=error;service.callbackError=error;}
        finally
        {
            service.handles.Remove((nint)handle);
            try{transfer.Destination.Dispose();if(transfer.Root.IsAllocated)transfer.Root.Free();}
            catch(Exception error){failure??=error;service.callbackError=error;}
            if(failure!=null)service.failedTransfers++;
            else if(canceled)service.canceledTransfers++;
            else service.completedTransfers++;
            if(transfer.Client)
            {
                if(failure!=null)transfer.Completion.TrySetException(failure);
                else if(canceled)transfer.Completion.TrySetCanceled();
                else transfer.Completion.TrySetResult(result);
            }
        }
    }
    /// <summary>Cancel the current transfer locally, discard partial writes, and restart the same original client/server. Does not wait for its peer.</summary>
    public void CancelPending()=>Boundary(core=>
    {
        core.lwip_owned_tftp_stop();initialized=false;
        if(handles.Count!=0)throw new InvalidOperationException("Original TFTP stop did not close its storage handle.");
        StartOriginal(core);
    });
    public void ThrowCallbackErrors()=>Boundary(_=>
    {if(callbackError is { } error){callbackError=null;throw new AggregateException("TFTP storage callback failed.",error);}});
    public void Dispose()
    {
        if(disposed)return;
        try{stack.RunServiceBoundary(()=>
        {
            if(disposed)return;closing=true;
            stack.Invoke(core=>{if(initialized){core.lwip_owned_tftp_stop();initialized=false;}});
            stack.ServiceBarrier();
            stack.Invoke(core=>
            {
                if(handles.Count!=0)throw new InvalidOperationException("TFTP storage handles remain live; callback roots retained for disposal retry.");
                if(registered){Owners.Remove(core);registered=false;}
                if(claimed){LwipUdpApplications.ReleaseTftp(core,this);claimed=false;}
                NativeMemory.Free((void*)context);context=0;files.Clear();storedBytes=0;
            });
            disposed=true;stack.CompleteServiceDisposal(this);
        });}catch(ObjectDisposedException)when(disposed){}
    }
}
