using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void ArpQueueCase(VirtualPair pair)
    {
        var receiver=new Receiver();var handle=GCHandle.Alloc(receiver);nint sender=0,listener=0;byte[]? reply=null;
        try
        {
            pair.Drain();pair.A.Invoke(owner=>owner.etharp_cleanup_netif((netif*)owner.Globals.netif_default));
            pair.Filter=(toB,frame)=>{if(!toB&&frame.Length>=42&&frame[12]==8&&frame[13]==6&&frame[20]==0&&frame[21]==2){Volatile.Write(ref reply,frame);return null;}return frame;};
            pair.B.Invoke(owner=>{ip_addr address;fixed(byte*text="10.0.0.2\0"u8)Require(owner.ipaddr_aton(text,&address)==1,"ARP queue destination");var pcb=owner.udp_new();Require(pcb!=null,"ARP queue listener");listener=(nint)pcb;Require(owner.udp_bind(pcb,&address,25050)==0,"ARP queue bind");owner.udp_recv(pcb,&Receive,(void*)GCHandle.ToIntPtr(handle));});
            pair.A.Invoke(owner=>{var pcb=owner.udp_new();Require(pcb!=null,"ARP queue sender");sender=(nint)pcb;ip_addr address;fixed(byte*text="10.0.0.2\0"u8)Require(owner.ipaddr_aton(text,&address)==1,"ARP queue target");for(byte value=1;value<=3;value++){var p=owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT,1,pbuf_type.PBUF_RAM);Require(p!=null,"queued ARP pbuf");try{Require(owner.pbuf_take(p,&value,1)==0,"queued ARP byte");Require(owner.udp_sendto(pcb,p,&address,25050)==0,"ARP unresolved send");}finally{owner.pbuf_free(p);}}});
            pair.Drain();lock(receiver.Datagrams)Require(receiver.Datagrams.Count==0,"withheld neighbor response prevents delivery");Require(Volatile.Read(ref reply)!=null,"actual neighbor reply captured");pair.Filter=null;
            nint netifAddress=0;pair.A.Invoke(owner=>netifAddress=owner.Globals.netif_default);pair.A.Receive(netifAddress,reply!);pair.Drain();if(receiver.Error!=null)throw receiver.Error;
            lock(receiver.Datagrams)
            {
                if(ARP_QUEUEING!=0){Require(receiver.Datagrams.Count==3,"ARP queue retains all pending packets");for(int i=0;i<3;i++)Require(receiver.Datagrams[i].Length==1&&receiver.Datagrams[i][0]==i+1,"ARP queued FIFO");}
                else Require(receiver.Datagrams.Count==1&&receiver.Datagrams[0].Length==1&&receiver.Datagrams[0][0]==3,"ARP single pending slot replaces older packets");
            }
            Console.WriteLine($"PASS ARP_QUEUEING={ARP_QUEUEING} withheld reply pending packets=3 delivered={(ARP_QUEUEING!=0?3:1)} exact FIFO/drop policy");
        }
        finally{pair.Filter=null;pair.A.Invoke(owner=>{if(sender!=0)owner.udp_remove((udp_pcb*)sender);});pair.B.Invoke(owner=>{if(listener!=0)owner.udp_remove((udp_pcb*)listener);});handle.Free();receiver.Arrived.Dispose();}
    }
}
