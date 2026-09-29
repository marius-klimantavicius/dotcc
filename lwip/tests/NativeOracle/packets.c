/* Matching native packet primitive witness using an in-memory Ethernet netif. */
#include "lwip/init.h"
#include "lwip/udp.h"
#include "lwip/netif.h"
#include "lwip/etharp.h"
#include "lwip/ethip6.h"
#include "netif/ethernet.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define CHECK(x) do{if(!(x)){fprintf(stderr,"PACKET FAIL line%d %s\n",__LINE__,#x);exit(93);}}while(0)
static FILE *trace;
static unsigned trace_sequence;
static void trace_frame(const unsigned char *bytes,unsigned length)
{if(trace){uint32_t h[4]={0,trace_sequence++,length,length};fwrite(h,sizeof(h),1,trace);fwrite(bytes,length,1,trace);}}
static unsigned char frames[32][1600];static unsigned lengths[32],frame_count;
static unsigned char payload[4500];static unsigned expected_length,received,mutation;
static err_t capture(struct netif*n,struct pbuf*p){(void)n;CHECK(frame_count<32&&p->tot_len<=1600);lengths[frame_count]=p->tot_len;CHECK(pbuf_copy_partial(p,frames[frame_count],p->tot_len,0)==p->tot_len);trace_frame(frames[frame_count],p->tot_len);frame_count++;return ERR_OK;}
static err_t initialize(struct netif*n){n->name[0]='t';n->name[1]='p';n->mtu=1500;n->hwaddr_len=6;n->hwaddr[0]=2;n->hwaddr[5]=1;n->output=etharp_output;n->output_ip6=ethip6_output;n->linkoutput=capture;n->flags=NETIF_FLAG_BROADCAST|NETIF_FLAG_ETHARP|NETIF_FLAG_ETHERNET|NETIF_FLAG_IGMP|NETIF_FLAG_MLD6;return ERR_OK;}
static void receive_packet(void*arg,struct udp_pcb*u,struct pbuf*p,const ip_addr_t*addr,u16_t port){(void)arg;(void)u;(void)addr;(void)port;CHECK(p->tot_len==expected_length);unsigned char bytes[4500];CHECK(pbuf_copy_partial(p,bytes,p->tot_len,0)==p->tot_len);for(unsigned i=0;i<expected_length;i++)CHECK(bytes[i]==(unsigned char)(payload[i]^((mutation&&i==32)?0x80:0)));received++;pbuf_free(p);}
static void input_frames(struct netif*n){unsigned count=frame_count;for(unsigned i=0;i<count;i++){trace_frame(frames[i],lengths[i]);struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)lengths[i],PBUF_POOL);CHECK(p);CHECK(pbuf_take(p,frames[i],(u16_t)lengths[i])==ERR_OK);CHECK(ethernet_input(p,n)==ERR_OK);}frame_count=0;}
static void send_packet(struct udp_pcb*sender,ip_addr_t*target,u16_t port){struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,(u16_t)expected_length,PBUF_RAM);CHECK(p);CHECK(pbuf_take(p,payload,(u16_t)expected_length)==ERR_OK);CHECK(udp_sendto(sender,p,target,port)==ERR_OK);pbuf_free(p);}
void native_packet_cases(void*argument)
{
  (void)argument;const char *trace_path=getenv("LWIP_PACKET_PCAP");if(trace_path){trace=fopen(trace_path,"wb");CHECK(trace);uint32_t header[6]={0xa1b2c3d4,0x00040002,0,0,65535,1};CHECK(fwrite(header,sizeof(header),1,trace)==1);}struct netif interface;memset(&interface,0,sizeof(interface));ip4_addr_t ip,mask,gw;IP4_ADDR(&ip,10,1,0,1);IP4_ADDR(&mask,255,255,255,0);ip4_addr_set_zero(&gw);
  CHECK(netif_add(&interface,&ip,&mask,&gw,NULL,initialize,ethernet_input));netif_set_default(&interface);
  ip6_addr_t ipv6;CHECK(ip6addr_aton("fd01::1",&ipv6));netif_ip6_addr_set(&interface,0,&ipv6);netif_ip6_addr_set_state(&interface,0,IP6_ADDR_PREFERRED);netif_set_up(&interface);netif_set_link_up(&interface);
  for(int v6=0;v6<2;v6++)for(int lite=0;lite<2;lite++)
  {
    frame_count=received=mutation=0;expected_length=lite?64:4500;for(unsigned i=0;i<expected_length;i++)payload[i]=(unsigned char)(i*31+(i>>8));
    ip_addr_t target;CHECK(ipaddr_aton(v6?"ff02::1":"224.0.0.1",&target));if(v6)ip6_addr_assign_zone(ip_2_ip6(&target),IP6_MULTICAST,&interface);
    struct udp_pcb*sender=udp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);struct udp_pcb*listener=udp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);CHECK(sender&&listener);
    if(lite){sender->flags|=UDP_FLAGS_UDPLITE;listener->flags|=UDP_FLAGS_UDPLITE;sender->chksum_len_tx=listener->chksum_len_rx=16;}
    ip_addr_t any;ip_addr_set_any(v6,&any);CHECK(udp_bind(listener,&any,25000)==ERR_OK);udp_recv(listener,receive_packet,NULL);send_packet(sender,&target,25000);
    if(!lite)CHECK(frame_count>=3);input_frames(&interface);CHECK(received==1);
    if(lite)
    {
      unsigned offset=v6?54:34;send_packet(sender,&target,25000);CHECK(frame_count==1);CHECK(frames[0][offset+4]==0&&frames[0][offset+5]==16);frames[0][offset+8+32]^=0x80;mutation=1;input_frames(&interface);CHECK(received==2);
      send_packet(sender,&target,25000);CHECK(frame_count==1);frames[0][offset+8+2]^=0x80;input_frames(&interface);CHECK(received==2);
      mutation=0;send_packet(sender,&target,25000);input_frames(&interface);CHECK(received==3);
    }
    udp_remove(sender);udp_remove(listener);printf("{\"case\":\"native-%s-ipv%d\",\"payload\":%u,\"received\":%u}\n",lite?"udplite":"fragment",v6?6:4,expected_length,received);
  }
  netif_set_link_down(&interface);netif_set_down(&interface);netif_remove(&interface);if(trace){CHECK(fclose(trace)==0);trace=NULL;}
}
