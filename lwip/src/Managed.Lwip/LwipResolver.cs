using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using static Managed.Networking.Lwip;
namespace Managed.Networking;

/// <summary>Resolves through the owning lwIP DNS stack. Cancellation completes the managed task while retaining its original callback context until the real reply or core shutdown.</summary>
public sealed unsafe class LwipResolver:IDisposable,ILwipAfterCoreStop
{
    readonly LwipStack stack;readonly object gate=new();readonly HashSet<Query> pending=new();bool stopping;
    sealed class Query(LwipResolver parent,IDisposable lease)
    {
        internal readonly LwipResolver Parent=parent;internal readonly IDisposable Lease=lease;
        internal readonly TaskCompletionSource<IPAddress> Completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal GCHandle Root;internal CancellationTokenRegistration Cancellation;
        internal bool Released;
    }
    private LwipResolver(LwipStack stack)=>this.stack=stack;
    public static LwipResolver Start(LwipStack stack){ArgumentNullException.ThrowIfNull(stack);return stack.StartService(s=>new LwipResolver(s));}
    public int RetainedQueries {get{lock(gate)return pending.Count;}}
    static string Normalize(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if(name.Length==0||name.IndexOf('\0')>=0||name.Any(char.IsWhiteSpace))throw new ArgumentException("DNS names cannot be empty or contain NUL/whitespace.",nameof(name));
        string ascii=new IdnMapping{UseStd3AsciiRules=true}.GetAscii(name);
        string labels=ascii.EndsWith('.')?ascii[..^1]:ascii;
        if(labels.Length==0||labels.Length>253||labels.Split('.').Any(x=>x.Length==0||x.Length>63))throw new ArgumentException("DNS label/name length is invalid.",nameof(name));
        return ascii;
    }
    public Task<IPAddress> ResolveAsync(string name,bool ipv6=false,CancellationToken cancellation=default)
    {
        ArgumentNullException.ThrowIfNull(name);cancellation.ThrowIfCancellationRequested();
        lock(gate)ObjectDisposedException.ThrowIf(stopping,this);
        if(name.IndexOf('\0')>=0||name.Any(char.IsWhiteSpace))throw new ArgumentException("Invalid DNS name.",nameof(name));
        if(name.Contains(':')&&name.Contains('%')&&(!uint.TryParse(name[(name.LastIndexOf('%')+1)..],out var zone)||zone>byte.MaxValue))throw new ArgumentException("IPv6 zones must be numeric owner-relative lwIP interface indices (0..255).",nameof(name));
        if(IPAddress.TryParse(name,out var literal))
        {
            if((literal.GetAddressBytes().Length==16)!=ipv6)throw new ArgumentException("Literal address family does not match the requested DNS family.",nameof(name));
            return Task.FromResult(ipv6?new IPAddress(literal.GetAddressBytes(),literal.ScopeId):new IPAddress(literal.GetAddressBytes()));
        }
        string ascii=Normalize(name);Task<IPAddress>? task=null;
        stack.Invoke(core=>
        {
            Query query;lock(gate)
            {
                ObjectDisposedException.ThrowIf(stopping,this);var lease=core.__DotCcRetain();query=new Query(this,lease);
                try{query.Root=GCHandle.Alloc(query);pending.Add(query);}
                catch{if(query.Root.IsAllocated)query.Root.Free();lease.Dispose();throw;}
            }
            task=query.Completion.Task;
            query.Cancellation=cancellation.Register(()=>query.Completion.TrySetCanceled(cancellation));
            if(cancellation.IsCancellationRequested){Release(query);return;}
            bool callStarted=false;
            try
            {
                byte[]text=Encoding.ASCII.GetBytes(ascii+"\0");ip_addr immediate;int result;
                callStarted=true;
                fixed(byte*p=text)result=core.dns_gethostbyname_addrtype(p,&immediate,&Found,(void*)GCHandle.ToIntPtr(query.Root),(byte)(ipv6?LWIP_DNS_ADDRTYPE_IPV6:LWIP_DNS_ADDRTYPE_IPV4));
                if(result==0){query.Completion.TrySetResult(Copy(&immediate));Release(query);}
                else if(result!=-5){query.Completion.TrySetException(new IOException($"lwIP DNS admission failed ({result})."));Release(query);}
                // ERR_INPROGRESS retains the callback root even if cancellation
                // has already completed the caller's Task.
            }
            catch(Exception error){query.Completion.TrySetException(error);if(!callStarted)Release(query);}
        });
        return task!;
    }
    static IPAddress Copy(ip_addr*address)
    {
        if(address->type==6)return new IPAddress(new ReadOnlySpan<byte>(&address->u_addr.ip6.addr[0],16),address->u_addr.ip6.zone);
        return new IPAddress(new ReadOnlySpan<byte>(&address->u_addr.ip4.addr,4));
    }
    static void Found(Lwip owner,byte*name,ip_addr*address,void*argument)
    {
        var query=(Query)GCHandle.FromIntPtr((nint)argument).Target!;
        try
        {
            if(!ReferenceEquals(owner,query.Parent.stack.Core))throw new InvalidOperationException("Foreign resolver callback owner.");
            if(address==null)query.Completion.TrySetException(new IOException("lwIP DNS lookup failed or expired."));
            else query.Completion.TrySetResult(Copy(address));
        }
        catch(Exception error){query.Completion.TrySetException(error);}
        finally{query.Parent.Release(query);}
    }
    void Release(Query query)
    {
        lock(gate){if(query.Released)return;query.Released=true;pending.Remove(query);}
        query.Cancellation.Dispose();if(query.Root.IsAllocated)query.Root.Free();query.Lease.Dispose();
    }
    public void Dispose()
    {
        Query[] snapshot;lock(gate){stopping=true;snapshot=pending.ToArray();}
        foreach(var query in snapshot)query.Completion.TrySetCanceled();
        // Upstream has no public per-query cancellation. Roots remain reachable
        // until Found returns or the owning stack's post-join phase runs.
    }
    void ILwipAfterCoreStop.ReleaseAfterCoreStop()
    {
        Query[] snapshot;lock(gate){if(!stopping)throw new InvalidOperationException("Resolver must stop before core release.");snapshot=pending.ToArray();}
        foreach(var query in snapshot)Release(query);
    }
}
