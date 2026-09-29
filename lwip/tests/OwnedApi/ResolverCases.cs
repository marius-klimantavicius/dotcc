using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    sealed class ResolverPeer
    {
        internal int Queries;internal Exception? Error;internal bool Hold;
        internal string LastName="";internal byte[]? Held;internal ip_addr Address;internal ushort Port;
    }
    static byte[] Response(ReadOnlySpan<byte> query)
    {
        R(query.Length>=17&&BinaryPrimitives.ReadUInt16BigEndian(query[4..])==1,"one DNS question");int cursor=12;
        while(cursor<query.Length&&query[cursor]!=0){int length=query[cursor++];R(length<=63&&cursor+length<query.Length,"uncompressed query label");cursor+=length;}
        R(cursor+5==query.Length,"DNS query ends after type/class");cursor++;ushort type=BinaryPrimitives.ReadUInt16BigEndian(query[cursor..]);R(type==1||type==28,"A or AAAA query");R(BinaryPrimitives.ReadUInt16BigEndian(query[(cursor+2)..])==1,"IN question");
        bool missing=query[12]==7&&query.Slice(13,7).SequenceEqual("missing"u8);int size=type==1?4:16;
        byte[] response=new byte[query.Length+(missing?0:12+size)];query.CopyTo(response);response[2]=0x81;response[3]=(byte)(missing?0x83:0x80);response[6]=0;response[7]=(byte)(missing?0:1);
        if(!missing){int tail=query.Length;response[tail++]=0xc0;response[tail++]=0x0c;BinaryPrimitives.WriteUInt16BigEndian(response.AsSpan(tail),type);tail+=2;response[tail++]=0;response[tail++]=1;BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(tail),60);tail+=4;BinaryPrimitives.WriteUInt16BigEndian(response.AsSpan(tail),(ushort)size);tail+=2;if(type==1)new byte[]{192,0,2,123}.CopyTo(response,tail);else{response[tail]=0x20;response[tail+1]=1;response[tail+2]=0x0d;response[tail+3]=0xb8;response[tail+15]=0x7b;}}
        return response;
    }
    static void ResolverQuery(Lwip owner,void*argument,udp_pcb*pcb,pbuf*packet,ip_addr*address,ushort port)
    {
        var peer=(ResolverPeer)GCHandle.FromIntPtr((nint)argument).Target!;pbuf*output=null;
        try
        {
            byte[]bytes=new byte[packet->tot_len];fixed(byte*p=bytes)R(owner.pbuf_copy_partial(packet,p,packet->tot_len,0)==packet->tot_len,"resolver wire copy");
            var labels=new List<string>();for(int at=12;bytes[at]!=0;){int size=bytes[at++];labels.Add(Encoding.ASCII.GetString(bytes,at,size));at+=size;}
            peer.LastName=string.Join('.',labels);Interlocked.Increment(ref peer.Queries);byte[]response=Response(bytes);
            if(peer.Hold){peer.Held=response;peer.Address=*address;peer.Port=port;return;}
            output=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)response.Length,pbuf_type.PBUF_RAM);R(output!=null,"resolver response allocate");fixed(byte*p=response)R(owner.pbuf_take(output,p,(ushort)response.Length)==0,"resolver response copy");R(owner.udp_sendto(pcb,output,address,port)==0,"resolver response send");
        }
        catch(Exception error){peer.Error=error;}finally{if(output!=null)owner.pbuf_free(output);owner.pbuf_free(packet);}
    }
    static void ResolverCases(VirtualPair pair)
    {
        var peer=new ResolverPeer();var root=GCHandle.Alloc(peer);nint server=0;using var resolver=LwipResolver.Start(pair.A);
        try
        {
            pair.B.Invoke(core=>{ip_addr address;fixed(byte*p="10.0.0.2\0"u8)R(core.ipaddr_aton(p,&address)==1,"resolver peer address");var pcb=core.udp_new();R(pcb!=null,"resolver peer PCB");server=(nint)pcb;R(core.udp_bind(pcb,&address,53)==0,"resolver peer bind");core.udp_recv(pcb,&ResolverQuery,(void*)GCHandle.ToIntPtr(root));});
            pair.A.Invoke(core=>{ip_addr address;fixed(byte*p="10.0.0.2\0"u8)R(core.ipaddr_aton(p,&address)==1,"resolver server address");core.dns_setserver(0,&address);});
            foreach(string invalid in new[]{"", "bad\0name.test", "white space.test", "empty..label", new string('a',64)+".test"}){bool rejected=false;try{resolver.ResolveAsync(invalid);}catch(ArgumentException){rejected=true;}R(rejected&&resolver.RetainedQueries==0,"invalid DNS names rejected before callback allocation");}
            R(Await(resolver.ResolveAsync("fe80::1%3",true)).ScopeId==3&&resolver.RetainedQueries==0,"literal IPv6 numeric zone copied without DNS context");
            bool zoneRejected=false;try{resolver.ResolveAsync("fe80::1%256",true);}catch(ArgumentException){zoneRejected=true;}R(zoneRejected,"resolver literal zone uses same lwIP byte index range as socket/netconn DTOs");
            R(Await(resolver.ResolveAsync("bücher.example.test")).Equals(IPAddress.Parse("192.0.2.123")),"IDNA A result copied");pair.Drain();R(peer.LastName=="xn--bcher-kva.example.test","IDNA wire encoding");int queries=peer.Queries;R(Await(resolver.ResolveAsync("bücher.example.test")).Equals(IPAddress.Parse("192.0.2.123")),"cached resolver result");pair.Drain();R(peer.Queries==queries,"cached DNS has no new query");R(Await(resolver.ResolveAsync("v6.owned.test",true)).Equals(IPAddress.Parse("2001:db8::7b")),"AAAA result copied");
            bool missing=false;try{Await(resolver.ResolveAsync("missing.owned.test"));}catch(IOException){missing=true;}R(missing,"NXDOMAIN becomes managed failure");
            peer.Hold=true;using var cancel=new CancellationTokenSource();var delayed=resolver.ResolveAsync("delayed.owned.test",cancellation:cancel.Token);pair.Drain();R(peer.Held!=null&&resolver.RetainedQueries==1,"actual delayed original DNS context rooted");cancel.Cancel();bool canceled=false;try{Await(delayed);}catch(OperationCanceledException){canceled=true;}R(canceled&&resolver.RetainedQueries==1,"canceled Task retains still-callable C context");GC.Collect();GC.WaitForPendingFinalizers();
            pair.B.Invoke(core=>{byte[]bytes=peer.Held!;var p=core.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,(ushort)bytes.Length,pbuf_type.PBUF_RAM);R(p!=null,"late reply pbuf");try{fixed(byte*b=bytes)R(core.pbuf_take(p,b,(ushort)bytes.Length)==0,"late reply copy");ip_addr address=peer.Address;R(core.udp_sendto((udp_pcb*)server,p,&address,peer.Port)==0,"late reply delivery");}finally{core.pbuf_free(p);}});pair.Drain();R(resolver.RetainedQueries==0&&delayed.IsCanceled,"late callback safely releases canceled context exactly once");if(peer.Error!=null)throw peer.Error;
            Console.WriteLine("PASS owning-resolver IDNA/NUL/length A/AAAA/cache/NXDOMAIN copied results and delayed callback cancellation retention");
        }
        finally{pair.B.Invoke(core=>{if(server!=0)core.udp_remove((udp_pcb*)server);});root.Free();}
    }
    sealed class RetryStop:IDisposable
    {
        bool first=true;
        public void Dispose(){if(first){first=false;throw new InvalidOperationException("retained service retry");}}
    }
    static void ResolverShutdownCases()
    {
        using var a=new LwipStack();using var b=new LwipStack();a.Start();b.Start();
        foreach(var stack in new[]{a,b})
        {
            stack.AddInterface("10.9.0.1","",new byte[]{2,0,0,0,9,1},_=>{});
            stack.Invoke(core=>{ip_addr server;fixed(byte*p="10.9.0.2\0"u8)R(core.ipaddr_aton(p,&server)==1,"pending resolver server");core.dns_setserver(0,&server);});
        }
        var resolverA=LwipResolver.Start(a);var resolverB=LwipResolver.Start(b);
        Task<IPAddress> pendingA=resolverA.ResolveAsync("pending.shutdown.test"),pendingB=resolverB.ResolveAsync("pending.shutdown.test");
        R(resolverA.RetainedQueries==1&&resolverB.RetainedQueries==1,"both owners retain genuine pending original DNS callbacks");
        a.StartService(_=>new RetryStop());bool retry=false;
        try{a.Dispose();}catch(InvalidOperationException error)when(error.Message=="retained service retry"){retry=true;}
        R(retry&&resolverA.RetainedQueries==1&&!pendingA.IsCompleted,"failed service disposal preserves callback root and owner lease");
        GC.Collect();GC.WaitForPendingFinalizers();a.Dispose();
        R(resolverA.RetainedQueries==0&&pendingA.IsCanceled,"successful core join releases canceled pending callback exactly once");
        R(resolverB.RetainedQueries==1&&!pendingB.IsCompleted,"A disposal preserves B pending query root");
        R(Await(resolverB.ResolveAsync("192.0.2.1")).Equals(IPAddress.Parse("192.0.2.1")),"B resolver service admits work after A release");
        b.Dispose();R(resolverB.RetainedQueries==0&&pendingB.IsCanceled,"B independently releases callback only at its own core stop");
    }
}
