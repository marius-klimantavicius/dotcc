using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Networking;
using static Managed.Networking.Lwip;
unsafe partial class Program
{
    static void AutoIp(VirtualPair pair)
    {
        nint client=0;var state=(autoip*)NativeMemory.AllocZeroed((nuint)sizeof(autoip));R(state!=null,"AutoIP state allocate");bool attached=false;int probes=0,announcements=0;
        pair.Observe=(toB,frame)=>
        {
            if(!toB||frame.Length<42||frame[12]!=8||frame[13]!=6||frame[21]!=1)return;
            uint sender=BinaryPrimitives.ReadUInt32BigEndian(frame.AsSpan(28)),target=BinaryPrimitives.ReadUInt32BigEndian(frame.AsSpan(38));
            if((target>>16)==0xa9fe&&sender==0)Interlocked.Increment(ref probes);
            if((sender>>16)==0xa9fe&&sender==target)Interlocked.Increment(ref announcements);
        };
        try
        {
            pair.A.Invoke(owner=>{client=owner.Globals.netif_default;var n=(netif*)client;owner.netif_set_addr(n,null,null,null);owner.autoip_set_struct(n,state);attached=true;R(owner.autoip_start(n)==0,"AutoIP start");});
            bool Bound(){bool result=false;pair.A.Invoke(owner=>result=owner.autoip_supplied_address((netif*)client)!=0);return result;}
            void Until(){for(int i=0;i<400;i++){pair.Drain();if(Bound())return;pair.A.Invoke(owner=>owner.acd_tmr());}throw new TimeoutException("AutoIP ACD did not bind within bounded protocol ticks");}
            uint Address(){uint result=0;pair.A.Invoke(owner=>{var n=(netif*)client;result=owner.lwip_htonl(n->ip_addr.u_addr.ip4.addr);R(result>=0xa9fe0100&&result<=0xa9fefeff,"AutoIP selected permitted link-local range");R(n->netmask.u_addr.ip4.addr==owner.lwip_htonl(0xffff0000)&&n->gw.u_addr.ip4.addr==0,"AutoIP link-local mask/no gateway");});return result;}
            Until();uint first=Address();R(probes>=3&&announcements>=1,"AutoIP actual ARP probes and announcement");
            // First conflict defends; a second inside DEFEND_INTERVAL withdraws and chooses a new address.
            byte[] conflict=ConflictArp();BinaryPrimitives.WriteUInt32BigEndian(conflict.AsSpan(28),first);BinaryPrimitives.WriteUInt32BigEndian(conflict.AsSpan(38),first);
            int before=announcements;pair.A.Receive(client,conflict);pair.Drain();R(Bound()&&announcements>before,"AutoIP defends first conflict on wire");pair.A.Receive(client,conflict);pair.Drain();R(!Bound(),"AutoIP second conflict withdraws lease");Until();uint second=Address();R(first!=second,"AutoIP conflict chooses another address");
            pair.A.SetLinkState(client,false);pair.A.SetLinkState(client,true);R(!Bound(),"DHCP cooperation holds AutoIP stopped after link cycle");pair.A.Invoke(owner=>R(owner.autoip_start((netif*)client)==0,"explicit AutoIP restart after cooperative link cycle"));Until();R(Address()==second,"AutoIP link recovery preserves chosen address");
            Console.WriteLine($"PASS autoip-fixtures: ARP probes={probes} announcements={announcements}, first conflict defense, repeated conflict address change, link recovery");
        }
        finally
        {
            if(attached)pair.A.Invoke(owner=>{var n=(netif*)client;owner.autoip_stop(n);owner.acd_stop(&state->acd);owner.acd_remove(n,&state->acd);owner.autoip_remove_struct(n);});pair.Drain();pair.Observe=null;NativeMemory.Free(state);
        }
    }
}
