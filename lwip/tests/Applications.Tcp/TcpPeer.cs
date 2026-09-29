using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static Fixture;

// Coherent independent application peer using original translated raw TCP.
// Application framing below is fixture code only; product clients remain C.
internal sealed unsafe class TcpPeer:IDisposable
{
    readonly LwipStack stack;
    readonly object gate=new();
    readonly List<byte> bytes=new();
    readonly GCHandle root;
    nint listener,connection;
    Exception? failure;
    bool connected,finished,disposed;
    long received;
    internal Action<ReadOnlyMemory<byte>>? Observe;
    internal bool Discard;
    internal TcpPeer(LwipStack stack,ushort port)
    {
        this.stack=stack;root=GCHandle.Alloc(this);
        try{stack.Invoke(owner=>
        {
            var pcb=owner.tcp_new_ip_type(0);Require(pcb!=null,"peer TCP allocation");connection=(nint)pcb;
            Require(owner.tcp_bind(pcb,null,port)==0,"peer TCP bind");
            var listen=owner.tcp_listen_with_backlog(pcb,1);Require(listen!=null,"peer TCP listen");
            connection=0;listener=(nint)listen;owner.tcp_arg(listen,(void*)GCHandle.ToIntPtr(root));owner.tcp_accept(listen,&Accept);
        });}catch{Dispose();throw;}
    }
    static TcpPeer State(void* argument)=>(TcpPeer)GCHandle.FromIntPtr((nint)argument).Target!;
    static sbyte Accept(Lwip owner,void* arg,tcp_pcb* pcb,sbyte error)
    {
        var self=State(arg);
        try
        {
            Require(error==0&&self.connection==0,"one accepted peer connection");
            self.connection=(nint)pcb;owner.tcp_arg(pcb,arg);owner.tcp_recv(pcb,&Receive);owner.tcp_err(pcb,&Error);
            lock(self.gate)self.connected=true;return 0;
        }
        catch(Exception failure){self.Fail(failure);owner.tcp_abort(pcb);return -13;}
    }
    static void Error(Lwip owner,void* arg,sbyte error)
    {
        var self=State(arg);self.connection=0;
        self.Fail(new IOException("Independent TCP peer transport error "+error));
        lock(self.gate)self.finished=true;
    }
    static sbyte Receive(Lwip owner,void* arg,tcp_pcb* pcb,pbuf* packet,sbyte error)
    {
        var self=State(arg);
        try
        {
            Require(error==0,"peer receive status");
            if(packet==null)
            {
                owner.tcp_arg(pcb,null);owner.tcp_recv(pcb,null);owner.tcp_err(pcb,null);
                int result=owner.tcp_close(pcb);if(result!=0)owner.tcp_abort(pcb);
                self.connection=0;lock(self.gate)self.finished=true;Require(result==0,"normal peer close");return 0;
            }
            byte[] copy=new byte[packet->tot_len];fixed(byte* target=copy)Require(owner.pbuf_copy_partial(packet,target,packet->tot_len,0)==packet->tot_len,"peer copies received bytes");
            lock(self.gate)
            {
                self.received+=copy.Length;
                if(!self.Discard){Require(self.bytes.Count+copy.Length<=262144,"bounded TCP fixture capture");self.bytes.AddRange(copy);}
            }
            self.Observe?.Invoke(copy);owner.tcp_recved(pcb,packet->tot_len);return 0;
        }
        catch(Exception failure)
        {
            self.Fail(failure);
            if(self.connection!=0){owner.tcp_arg(pcb,null);owner.tcp_err(pcb,null);owner.tcp_abort(pcb);self.connection=0;}
            return -13;
        }
        finally{if(packet!=null)owner.pbuf_free(packet);}
    }
    void Fail(Exception error){lock(gate)failure??=error;}
    internal void Check(){lock(gate)if(failure!=null)throw new AggregateException("TCP application peer failed",failure);}
    internal bool Connected{get{lock(gate)return connected;}}
    internal bool Finished{get{lock(gate)return finished;}}
    internal long Received{get{lock(gate)return received;}}
    internal byte[] Snapshot(){lock(gate)return bytes.ToArray();}
    internal byte[] Take(int count)
    {lock(gate){Require(count<=bytes.Count,"captured peer byte count");var result=bytes.GetRange(0,count).ToArray();bytes.RemoveRange(0,count);return result;}}
    internal byte[] Until(Fixture fixture,byte[] terminator)
    {
        int end=-1;fixture.Wait(()=>{lock(gate){end=bytes.ToArray().AsSpan().IndexOf(terminator);return end>=0;}},Check);
        return Take(end+terminator.Length);
    }
    internal string Line(Fixture fixture)=>System.Text.Encoding.ASCII.GetString(Until(fixture,"\r\n"u8.ToArray()));
    internal void Send(Fixture fixture,ReadOnlySpan<byte> data)
    {
        byte[] copy=data.ToArray();int offset=0;
        while(offset<copy.Length)
        {
            bool written=false;int count=Math.Min(1024,copy.Length-offset),start=offset;
            fixture.Wait(()=>
            {
                stack.Invoke(owner=>
                {
                    Require(connection!=0,"peer connection available for send");
                    fixed(byte* p=copy)
                    {
                        int result=owner.tcp_write((tcp_pcb*)connection,p+start,(ushort)count,TCP_WRITE_FLAG_COPY);
                        Require(result==0||result==-1,"normal peer send result");written=result==0;
                        Require(owner.tcp_output((tcp_pcb*)connection)==0,"peer TCP output");
                    }
                });return written;
            },Check);
            offset+=count;
        }
    }
    internal void Send(Fixture fixture,string text)=>Send(fixture,System.Text.Encoding.ASCII.GetBytes(text));
    internal void FinishSend()=>stack.Invoke(owner=>{Require(connection!=0,"peer connection before FIN");Require(owner.tcp_shutdown((tcp_pcb*)connection,0,1)==0,"normal peer FIN");});
    public void Dispose()
    {
        if(disposed)return;
        stack.Invoke(owner=>
        {
            if(listener!=0){owner.tcp_arg((tcp_pcb*)listener,null);owner.tcp_accept((tcp_pcb*)listener,null);Require(owner.tcp_close((tcp_pcb*)listener)==0,"peer listener close");listener=0;}
            if(connection!=0){owner.tcp_arg((tcp_pcb*)connection,null);owner.tcp_recv((tcp_pcb*)connection,null);owner.tcp_err((tcp_pcb*)connection,null);owner.tcp_abort((tcp_pcb*)connection);connection=0;}
        });
        stack.Invoke(_=>{});root.Free();disposed=true;
    }
}
