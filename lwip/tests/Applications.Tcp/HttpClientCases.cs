using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

internal static unsafe class HttpClientCases
{
    sealed class State
    {
        internal readonly object Gate=new();
        internal readonly MemoryStream Body=new();
        internal bool RejectHeaders;
        internal int Headers,Results,Result;
        internal uint Length,Status;
        internal string HeaderText="";
        internal Exception? Failure;
        internal void Check(){lock(Gate)if(Failure!=null)throw new AggregateException("HTTP client callback failed",Failure);}
    }
    static State Get(void* arg)=>(State)GCHandle.FromIntPtr((nint)arg).Target!;
    static sbyte Headers(Lwip owner,_httpc_state* connection,void* arg,pbuf* packet,ushort length,uint contentLength)
    {
        var state=Get(arg);try
        {
            byte[] copy=new byte[length];fixed(byte* target=copy)Require(owner.pbuf_copy_partial(packet,target,length,0)==length,"copy borrowed HTTP headers");
            lock(state.Gate){state.Headers++;state.HeaderText=Encoding.ASCII.GetString(copy);Require(contentLength==5,"ordinary HTTP content length");return(sbyte)(state.RejectHeaders?-13:0);}
        }
        catch(Exception error){lock(state.Gate)state.Failure=error;return -13;}
    }
    static sbyte Body(Lwip owner,void* arg,altcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var state=Get(arg);try
        {
            Require(error==0&&packet!=null,"original HTTP body callback");
            lock(state.Gate){Require(state.Body.Length+packet->tot_len<=4096,"bounded HTTP body");for(var part=packet;part!=null;part=part->next)state.Body.Write(new ReadOnlySpan<byte>(part->payload,part->len));}
            owner.altcp_recved(pcb,packet->tot_len);
        }
        catch(Exception failure){lock(state.Gate)state.Failure=failure;}
        finally{if(packet!=null)owner.pbuf_free(packet);}
        return 0;
    }
    static void Result(Lwip owner,void* arg,ehttpc_result result,uint length,uint status,sbyte error)
    {
        var state=Get(arg);try{lock(state.Gate){state.Results++;state.Result=(int)result;state.Length=length;state.Status=status;Require(state.Results==1,"one HTTP terminal result");}}
        catch(Exception failure){lock(state.Gate)state.Failure=failure;}
    }
    static void Scenario(Fixture fixture,ushort port,int status,bool reject=false,bool earlyClose=false)
    {
        using var peer=new TcpPeer(fixture.Pair.B,port);
        var state=new State{RejectHeaders=reject};nint arg=fixture.Root(state),settings=fixture.Allocate((nuint)sizeof(_httpc_connection)),uri=fixture.Text("/fixture.txt");
        fixture.Pair.A.Invoke(owner=>
        {
            var config=(_httpc_connection*)settings;config->result_fn=&Result;config->headers_done_fn=&Headers;
            ip_addr address;Fixture.Address(owner,"10.0.0.2",&address);_httpc_state* connection;
            Require(owner.httpc_get_file(&address,port,(byte*)uri,config,&Body,(void*)arg,&connection)==0&&connection!=null,"original numeric HTTP client submission");
        });
        string request=Encoding.ASCII.GetString(peer.Until(fixture,"\r\n\r\n"u8.ToArray()));
        Require(request.StartsWith("GET /fixture.txt HTTP/1.1\r\n",StringComparison.Ordinal)&&request.Contains("\r\nHost: 10.0.0.2\r\n",StringComparison.Ordinal),"original HTTP request and host");
        if(!earlyClose)
        {
            string headers=$"HTTP/1.1 {status} {(status==200?"OK":"Not Found")}\r\nContent-Length: 5\r\nConnection: close\r\n\r\n";
            // Normal stream segmentation: deliver a partial header first and the
            // remainder later, preserving the complete coherent response.
            peer.Send(fixture,headers[..19]);fixture.Pair.Drain();peer.Send(fixture,headers[19..]);
            if(!reject)peer.Send(fixture,"hello");
        }
        if(!reject)peer.FinishSend();
        fixture.Wait(()=>{lock(state.Gate)return state.Results!=0;},state.Check);
        fixture.Wait(()=>peer.Finished,peer.Check);
        lock(state.Gate)
        {
            Require(state.Results==1&&state.Result==(earlyClose?4:reject?8:0),"original HTTP completion/ordinary close/header-policy abort");
            if(!earlyClose)Require(state.Headers==1&&state.Status==status,"original HTTP status reported independently");
            if(!earlyClose&&!reject)Require(state.Length==5&&state.Body.ToArray().AsSpan().SequenceEqual("hello"u8),"copied HTTP body and result length");
        }
        // Original 404 with a complete body reports HTTPC_RESULT_OK plus status
        // 404. Consumers must inspect status; this is not an HTTP-200-only claim.
    }
    internal static void Run(Fixture fixture)
    {Scenario(fixture,18080,200);Scenario(fixture,18081,404);Scenario(fixture,18082,200,reject:true);Scenario(fixture,18083,200,earlyClose:true);}
}
