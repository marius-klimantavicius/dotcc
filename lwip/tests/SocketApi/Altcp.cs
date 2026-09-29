using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
internal static unsafe partial class Program
{
    private sealed class AltcpTransfer : IDisposable
    {
        internal nint Client,Server,Listener;
        internal int Received,Echoed,Connected;
        internal Exception? Error;
        internal readonly AutoResetEvent Progress=new(false);
        internal void Fail(Exception error){Interlocked.CompareExchange(ref Error,error,null);Progress.Set();}
        public void Dispose()=>Progress.Dispose();
    }
    private static AltcpTransfer AltcpState(void* argument)=>(AltcpTransfer)GCHandle.FromIntPtr((nint)argument).Target!;
    private static sbyte AltcpAccepted(Lwip owner,void* argument,altcp_pcb* connection,sbyte error)
    {
        var state=AltcpState(argument);
        try{Require(error==0,"altcp accept");state.Server=(nint)connection;owner.altcp_arg(connection,argument);owner.altcp_recv(connection,&AltcpServerReceive);return 0;}
        catch(Exception failure){state.Fail(failure);state.Server=0;owner.altcp_abort(connection);return -13;}
    }
    private static sbyte AltcpConnected(Lwip owner,void* argument,altcp_pcb* connection,sbyte error)
    {
        var state=AltcpState(argument);
        try{Require(error==0,"altcp connected");owner.altcp_recv(connection,&AltcpClientReceive);Volatile.Write(ref state.Connected,1);state.Progress.Set();return 0;}
        catch(Exception failure){state.Fail(failure);state.Client=0;owner.altcp_abort(connection);return -13;}
    }
    private static sbyte AltcpServerReceive(Lwip owner,void* argument,altcp_pcb* connection,pbuf* packet,sbyte error)
    {
        var state=AltcpState(argument);
        if(packet==null){owner.altcp_arg(connection,null);owner.altcp_recv(connection,null);sbyte status=owner.altcp_close(connection);if(status==0)state.Server=0;return status;}
        try
        {
            Require(error==0,"altcp server status");for(var p=packet;p!=null;p=p->next)
            {for(int i=0;i<p->len;i++)Require(((byte*)p->payload)[i]==Pattern(state.Received++),"altcp server bytes");Require(owner.altcp_write(connection,p->payload,p->len,TCP_WRITE_FLAG_COPY)==0,"altcp echo queue");}
            owner.altcp_recved(connection,packet->tot_len);Require(owner.altcp_output(connection)==0,"altcp echo output");return 0;
        }
        catch(Exception failure){state.Fail(failure);state.Server=0;owner.altcp_abort(connection);return -13;}
        finally{owner.pbuf_free(packet);}
    }
    private static sbyte AltcpClientReceive(Lwip owner,void* argument,altcp_pcb* connection,pbuf* packet,sbyte error)
    {
        var state=AltcpState(argument);if(packet==null)return 0;
        try
        {
            Require(error==0,"altcp client status");int echoed=state.Echoed;
            for(var p=packet;p!=null;p=p->next)for(int i=0;i<p->len;i++)Require(((byte*)p->payload)[i]==Pattern(echoed++),"altcp echo bytes");
            owner.altcp_recved(connection,packet->tot_len);Volatile.Write(ref state.Echoed,echoed);state.Progress.Set();return 0;
        }
        catch(Exception failure){state.Fail(failure);state.Client=0;owner.altcp_abort(connection);return -13;}
        finally{owner.pbuf_free(packet);}
    }
    private static void AltcpWait(AltcpTransfer state,Func<bool> completed)
    {
        var time=Stopwatch.StartNew();while(!completed())
        {if(state.Error!=null)throw state.Error;if(time.Elapsed>TimeSpan.FromSeconds(10))throw new TimeoutException("altcp bounded progress");state.Progress.WaitOne(100);}
        if(state.Error!=null)throw state.Error;
    }
    private static void AltcpCase(VirtualPair pair,bool ipv6)
    {
        using var state=new AltcpTransfer();var handle=GCHandle.Alloc(state);nint argument=GCHandle.ToIntPtr(handle);
        byte type=(byte)(ipv6?lwip_ip_addr_type.IPADDR_TYPE_V6:lwip_ip_addr_type.IPADDR_TYPE_V4);ushort port=(ushort)(ipv6?24202:24201);
        byte[] text=Encoding.UTF8.GetBytes((ipv6?"fd00::2":"10.0.0.2")+"\0");
        try
        {
            pair.B.Invoke(owner=>
            {
                ip_addr address;fixed(byte* bytes=text)Require(owner.ipaddr_aton(bytes,&address)==1,"altcp server address");
                var listener=owner.altcp_tcp_new_ip_type(type);Require(listener!=null,"plain altcp allocation");state.Listener=(nint)listener;
                Require(owner.altcp_bind(listener,&address,port)==0,"altcp bind");
                var duplicate=owner.altcp_tcp_new_ip_type(type);Require(duplicate!=null,"altcp duplicate allocation");Require(owner.altcp_bind(duplicate,&address,port)==-8,"altcp duplicate bind ERR_USE");owner.altcp_abort(duplicate);
                sbyte error;listener=owner.altcp_listen_with_backlog_and_err(listener,2,&error);Require(listener!=null&&error==0,"altcp listen");state.Listener=(nint)listener;
                owner.altcp_arg(listener,(void*)argument);owner.altcp_accept(listener,&AltcpAccepted);
            });
            pair.A.Invoke(owner=>
            {
                ip_addr address;fixed(byte* bytes=text)Require(owner.ipaddr_aton(bytes,&address)==1,"altcp client address");
                var client=owner.altcp_tcp_new_ip_type(type);Require(client!=null,"altcp client");state.Client=(nint)client;owner.altcp_arg(client,(void*)argument);Require(owner.altcp_connect(client,&address,port,&AltcpConnected)==0,"altcp connect");
            });
            AltcpWait(state,()=>Volatile.Read(ref state.Connected)!=0);
            byte[] payload=new byte[4096];for(int sent=0;sent<262144;sent+=payload.Length)
            {
                for(int i=0;i<payload.Length;i++)payload[i]=Pattern(sent+i);
                pair.A.Invoke(owner=>{fixed(byte* bytes=payload){Require(owner.altcp_write((altcp_pcb*)state.Client,bytes,(ushort)payload.Length,TCP_WRITE_FLAG_COPY)==0,"altcp write");Require(owner.altcp_output((altcp_pcb*)state.Client)==0,"altcp output");}});
                int expected=sent+payload.Length;AltcpWait(state,()=>Volatile.Read(ref state.Echoed)==expected);
            }
            Require(state.Received==262144,"altcp full peer receive");
            pair.A.Invoke(owner=>{var client=(altcp_pcb*)state.Client;owner.altcp_arg(client,null);owner.altcp_recv(client,null);Require(owner.altcp_close(client)==0,"altcp close");state.Client=0;});
            Console.WriteLine($"PASS plain-altcp IPv{(ipv6?6:4)} echo=262144 duplicate-bind explicit-owner callbacks");
        }
        finally
        {
            pair.A.Invoke(owner=>{if(state.Client!=0){owner.altcp_arg((altcp_pcb*)state.Client,null);owner.altcp_abort((altcp_pcb*)state.Client);state.Client=0;}});
            pair.B.Invoke(owner=>{if(state.Server!=0){owner.altcp_arg((altcp_pcb*)state.Server,null);owner.altcp_abort((altcp_pcb*)state.Server);state.Server=0;}if(state.Listener!=0){Require(owner.altcp_close((altcp_pcb*)state.Listener)==0,"altcp listener close");state.Listener=0;}});
            handle.Free();
        }
    }
}
