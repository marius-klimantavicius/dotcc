using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe class Program
{
    static void R(bool value,string text){if(!value)throw new InvalidOperationException(text);}
    // Test owner used to qualify the C lifecycle boundary before public facade promotion.
    sealed class Circuit : IDisposable
    {
        internal readonly LwipStack Stack=new();internal readonly nint[] Ports=new nint[3];internal nint Bridge;
        readonly Pcap capture;uint clock=1000;byte[] expected=Array.Empty<byte>();int count,mask;long heap,timeout,pbuf;
        readonly Resource resource;
        sealed class Resource(Circuit parent):IDisposable{public void Dispose()=>parent.StopBridge();}
        internal Circuit(Pcap capture)
        {
            this.capture=capture;LwipSysArch.SetClock(Stack.Core,()=>Volatile.Read(ref clock));Stack.Start();
            for(int i=0;i<3;i++){int port=i;Ports[i]=Stack.AddInterface("0.0.0.0","",new byte[]{2,0,0,0,0,(byte)(i+1)},bytes=>Output(port,bytes));}
            Stack.Invoke(core=>{heap=core.Globals.lwip_stats.mem.used;timeout=core.Globals.memp_SYS_TIMEOUT.stats->used;pbuf=core.Globals.memp_PBUF_POOL.stats->used;});
            resource=Stack.StartService(_=>
            {
                try{Stack.Invoke(core=>{byte*mac=stackalloc byte[]{2,0,0,0,0x42,1};Bridge=(nint)core.lwip_owned_bridge_new(mac,3,8,2);R(Bridge!=0,"bridge allocation");R(core.lwip_owned_bridge_free((void*)Bridge)==-8,"bridge cannot free before stop");foreach(nint port in Ports)R(core.lwip_owned_bridge_add((void*)Bridge,(netif*)port)==0,"attach physical bridge port");R(core.lwip_owned_bridge_add((void*)Bridge,(netif*)Ports[0])==-8,"duplicate bridge port rejected");});return new Resource(this);}
                catch{if(Bridge!=0)StopBridge();throw;}
            });
        }
        void Output(int port,byte[]bytes){capture.Write(bytes);if(bytes.Length!=64||bytes[12]!=0x88||bytes[13]!=0xb5)return;R(bytes.AsSpan().SequenceEqual(expected),"unchanged bridge Ethernet bytes");Interlocked.Increment(ref count);Interlocked.Or(ref mask,1<<port);}
        internal void Barrier(){using(Stack.Core.__DotCcEnter())R(Stack.Core.lwip_owned_bridge_barrier()==0,"actual TCP/IP FIFO barrier");LwipFrames.ThrowErrors(Stack.Core);}
        internal void Send(int port,int source,int destination,int wanted)
        {
            expected=new byte[64];expected[0]=2;expected[5]=(byte)destination;expected[6]=2;expected[11]=(byte)source;expected[12]=0x88;expected[13]=0xb5;for(int i=14;i<64;i++)expected[i]=(byte)(i*7);count=mask=0;capture.Write(expected);Stack.Receive(Ports[port],expected);Barrier();R(mask==wanted&&count==System.Numerics.BitOperations.PopCount((uint)wanted),"bridge egress mask/count");Stack.Invoke(core=>R(core.Globals.memp_PBUF_POOL.stats->used==pbuf,"bridge releases input pbuf before callback returns"));
        }
        internal void StaticCases()
        {
            Stack.Invoke(core=>{netif*n=core.lwip_owned_bridge_netif((void*)Bridge);eth_addr a=default,b=default,c=default;byte*pa=(byte*)&a,pb=(byte*)&b,pc=(byte*)&c;pa[0]=pb[0]=pc[0]=2;pa[5]=40;pb[5]=41;pc[5]=42;R(core.bridgeif_fdb_add(n,&a,0)==0,"static drop");R(core.bridgeif_fdb_add(n,&b,4)==0,"static egress");R(core.bridgeif_fdb_add(n,&c,1)==-1,"static FDB capacity");R(core.bridgeif_fdb_remove(n,&c)==-6,"unknown static FDB remove");});Send(0,10,40,0);Send(0,10,41,4);
            Stack.Invoke(core=>{eth_addr a=default;byte*p=(byte*)&a;p[0]=2;p[5]=40;R(core.bridgeif_fdb_remove(core.lwip_owned_bridge_netif((void*)Bridge),&a)==0,"remove static drop");});Send(0,10,40,6);
        }
        internal void Age(int ticks)=>Stack.Invoke(core=>{for(int i=0;i<ticks;i++){clock+=1000;core.sys_check_timeouts();for(netif*n=(netif*)core.Globals.netif_list;n!=null;n=n->next)core.netif_poll(n);}});
        internal void StopBridge()
        {
            if(Bridge==0)return;Barrier();
            Stack.Invoke(core=>{netif*n=core.lwip_owned_bridge_netif((void*)Bridge);if(n!=null){var packet=core.pbuf_alloc(pbuf_layer.PBUF_RAW,40,pbuf_type.PBUF_RAM);R(packet!=null,"pending bridge loopback pbuf");var ip6=new Span<byte>(packet->payload,40);ip6.Clear();ip6[0]=0x60;ip6[6]=59;ip6[7]=1;ip6[8]=0xfe;ip6[9]=0x80;ip6[23]=2;ip6[24]=0xff;ip6[25]=2;ip6[39]=1;R(core.netif_loop_output(n,packet)==0,"queue bridge loopback");core.pbuf_free(packet);R(n->loop_first!=null,"loopback is pending at stop");R(core.lwip_owned_bridge_stop((void*)Bridge)==0,"bridge stop");R(n->loop_first==null,"stop drains original bridge loopback");}R(core.lwip_owned_bridge_stop((void*)Bridge)==0,"idempotent bridge stop");});
            Barrier();Stack.Invoke(core=>{R(core.lwip_owned_bridge_free((void*)Bridge)==0,"bridge context release after FIFO barrier");Bridge=0;R(core.Globals.lwip_stats.mem.used==heap,"bridge private heap naturally recovered");R(core.Globals.memp_SYS_TIMEOUT.stats->used==timeout,"bridge recurring FDB timer canceled");R(core.Globals.memp_PBUF_POOL.stats->used==pbuf,"bridge pbuf baseline");foreach(nint port in Ports)R((((netif*)port)->flags&2)!=0,"physical ETHARP flag restored");});
        }
        public void Dispose()=>Stack.Dispose();
    }
    static void Main(string[]args)
    {
        using var capture=new Pcap(args[0]);using var a=new Circuit(capture);using var b=new Circuit(capture);
        b.Stack.Invoke(core=>{R(core.lwip_owned_bridge_netif((void*)a.Bridge)==null,"foreign owner bridge lookup rejected");R(core.lwip_owned_bridge_stop((void*)a.Bridge)==-16,"foreign owner bridge stop rejected");});
        a.Send(0,10,20,6);a.Send(1,20,10,1);a.Send(2,10,20,2);a.Send(1,20,10,4);a.StaticCases();a.Stack.SetLinkState(a.Ports[2],false);a.Send(0,10,99,2);a.Stack.SetLinkState(a.Ports[2],true);a.Age(301);a.Send(0,10,20,6);
        b.Send(0,10,20,6);b.Send(1,20,10,1);GC.Collect();GC.WaitForPendingFinalizers();a.Dispose();b.Send(1,20,10,1);b.Age(301);b.Send(1,20,10,5);b.Dispose();
        Console.WriteLine("PASS bridge-fixtures: original flooding/learning/migration/static/aging/link controls; two owners overlap MACs; A disposal leaves B learned traffic; FIFO/loopback drain and natural allocation recovery");
    }
}
