using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

internal static unsafe class ProxyConnectCases
{
    sealed class State
    {
        internal readonly object Gate=new();
        internal nint Connection;
        internal bool Connected,Finished;
        internal readonly MemoryStream Body=new();
        internal Exception? Failure;
        internal void Check(){lock(Gate)if(Failure!=null)throw new AggregateException("CONNECT callback failed",Failure);}
    }
    static State Get(void* arg)=>(State)GCHandle.FromIntPtr((nint)arg).Target!;
    static sbyte Connected(Lwip owner,void* arg,altcp_pcb* pcb,sbyte error)
    {
        var state=Get(arg);try{lock(state.Gate){Require(error==0&&(nint)pcb==state.Connection,"original CONNECT connected callback");state.Connected=true;}return 0;}
        catch(Exception failure){lock(state.Gate)state.Failure=failure;return 0;}
    }
    static void Error(Lwip owner,void* arg,sbyte error)
    {var state=Get(arg);lock(state.Gate){state.Connection=0;state.Failure=new IOException("Original CONNECT transport error "+error);}}
    static sbyte Receive(Lwip owner,void* arg,altcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var state=Get(arg);try
        {
            Require(error==0,"CONNECT tunnel receive status");
            if(packet==null)
            {
                owner.altcp_arg(pcb,null);owner.altcp_recv(pcb,null);owner.altcp_err(pcb,null);
                int result=owner.altcp_close(pcb);if(result!=0)owner.altcp_abort(pcb);
                lock(state.Gate){state.Connection=0;state.Finished=true;}Require(result==0,"original CONNECT normal close");return 0;
            }
            lock(state.Gate){Require(state.Body.Length+packet->tot_len<=4096,"bounded CONNECT tunnel payload");for(var p=packet;p!=null;p=p->next)state.Body.Write(new ReadOnlySpan<byte>(p->payload,p->len));}
            owner.altcp_recved(pcb,packet->tot_len);return 0;
        }
        catch(Exception failure){lock(state.Gate)state.Failure=failure;return 0;}
        finally{if(packet!=null)owner.pbuf_free(packet);}
    }
    internal static void Run(Fixture fixture)
    {
        using var peer=new TcpPeer(fixture.Pair.B,18084);var state=new State();nint arg=fixture.Root(state),config=fixture.Allocate((nuint)sizeof(altcp_proxyconnect_config));
        fixture.BeforeStop(()=>fixture.Pair.A.Invoke(owner=>
        {
            if(state.Connection==0)return;var pcb=(altcp_pcb*)state.Connection;
            owner.altcp_arg(pcb,null);owner.altcp_recv(pcb,null);owner.altcp_err(pcb,null);owner.altcp_abort(pcb);state.Connection=0;
        }));
        fixture.Pair.A.Invoke(owner=>
        {
            var settings=(altcp_proxyconnect_config*)config;Fixture.Address(owner,"10.0.0.2",&settings->proxy_addr);settings->proxy_port=18084;
            var pcb=owner.altcp_proxyconnect_new_tcp(settings,0);Require(pcb!=null,"original CONNECT layer allocation");state.Connection=(nint)pcb;
            owner.altcp_arg(pcb,(void*)arg);owner.altcp_recv(pcb,&Receive);owner.altcp_err(pcb,&Error);
            ip_addr target;Fixture.Address(owner,"10.0.0.9",&target);Require(owner.altcp_connect(pcb,&target,8080,&Connected)==0,"original CONNECT submission through proxy");
        });
        string request=Encoding.ASCII.GetString(peer.Until(fixture,"\r\n\r\n"u8.ToArray()));Require(request.StartsWith("CONNECT 10.0.0.9:8080 HTTP/1.1\r\n",StringComparison.Ordinal),"original CONNECT target serialization");
        // The current parser searches each receive pbuf independently. Keep the
        // positive handshake together and send tunnel data only after success.
        // Split terminator and ordinary non-2xx handling remain required gaps.
        peer.Send(fixture,"HTTP/1.1 200 Connection Established\r\n\r\n");
        fixture.Wait(()=>{lock(state.Gate)return state.Connected;},state.Check);
        fixture.Pair.A.Invoke(owner=>
        {
            byte[] message="client tunnel payload"u8.ToArray();fixed(byte* data=message)Require(owner.altcp_write((altcp_pcb*)state.Connection,data,(ushort)message.Length,TCP_WRITE_FLAG_COPY)==0,"original CONNECT tunnel write");
            Require(owner.altcp_output((altcp_pcb*)state.Connection)==0,"original tunnel output");
        });
        fixture.Wait(()=>peer.Snapshot().Length>=21,peer.Check);Require(peer.Take(21).AsSpan().SequenceEqual("client tunnel payload"u8),"independent peer receives tunnel payload");
        peer.Send(fixture,"server tunnel payload");fixture.Wait(()=>{lock(state.Gate)return state.Body.Length==21;},state.Check);
        lock(state.Gate)Require(state.Body.ToArray().AsSpan().SequenceEqual("server tunnel payload"u8),"original tunnel receive forwards copied payload");
        peer.FinishSend();fixture.Wait(()=>{lock(state.Gate)return state.Finished;},state.Check);fixture.Wait(()=>peer.Finished,peer.Check);
    }
}
