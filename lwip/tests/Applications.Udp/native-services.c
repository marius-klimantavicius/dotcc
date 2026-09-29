#include "native-common.h"
#include "sntp-clock.h"
#include "lwip/apps/sntp.h"
#include "lwip/apps/netbiosns.h"
#include "lwip/apps/mdns.h"
#if SNTP_COMP_ROUNDTRIP
#error "This exact transmit-time fixture requires a separate uncompensated profile"
#endif

static struct udp_pcb *peer;
static atomic_int complete,probed;
static unsigned char answer[2048];static size_t answer_length;
static u16_t answer_port;
static u32_t clock_before;
static unsigned txt_calls;
static s8_t service_slot=-1;

static void receive_answer(void *arg,struct udp_pcb *pcb,struct pbuf *p,const ip_addr_t *address,u16_t port)
{
  (void)arg;(void)pcb;R(ip_addr_eq(address,&fixture_address)&&p->tot_len<=sizeof(answer));
  answer_length=p->tot_len;answer_port=port;R(pbuf_copy_partial(p,answer,p->tot_len,0)==p->tot_len);
  pbuf_free(p);atomic_store(&complete,1);
}
static void start_peer(u16_t port,udp_recv_fn receive)
{
  atomic_store(&complete,0);peer=udp_new_ip_type(IPADDR_TYPE_V4);R(peer);
  R(udp_bind(peer,IP_ANY_TYPE,port)==ERR_OK);udp_recv(peer,receive,NULL);
}
static void stop_peer(void)
{ udp_recv(peer,NULL,NULL);udp_remove(peer);peer=NULL; }

static void sntp_request(void *arg,struct udp_pcb *pcb,struct pbuf *p,const ip_addr_t *address,u16_t port)
{
  (void)arg;(void)pcb;unsigned char request[48],response[48]={0};
  R(ip_addr_eq(address,&fixture_address)&&p->tot_len==48&&pbuf_copy_partial(p,request,48,0)==48);pbuf_free(p);
  R((request[0]&7)==3);response[0]=0x24;response[1]=2;response[2]=6;response[3]=0xec;
  put32(response+8,1U<<16);memcpy(response+12,"TEST",4);put32(response+16,3999999122U);
  memcpy(response+24,request+40,8);put32(response+32,3999999123U);put32(response+36,0x40000000U);
  put32(response+40,3999999123U);put32(response+44,0x40000000U);fixture_send(peer,port,response,sizeof(response));
}
static void sntp_start(void *arg)
{
  (void)arg;start_peer(123,sntp_request);clock_before=dotcc_udp_sntp_notification_count();
  sntp_setoperatingmode(SNTP_OPMODE_POLL);sntp_setserver(0,&fixture_address);sntp_init();R(sntp_enabled());
}
static void sntp_check(void *arg)
{
  (void)arg;if(dotcc_udp_sntp_notification_count()!=clock_before) {
    R(dotcc_udp_sntp_notification_count()==clock_before+1);
    R(dotcc_udp_sntp_notification_seconds()==3999999123U&&dotcc_udp_sntp_notification_fraction()==0x40000000U);
    atomic_store(&complete,1);
  }
}
static void sntp_finish(void *arg)
{ (void)arg;sntp_stop();R(!sntp_enabled());R(dotcc_udp_sntp_notification_count()==clock_before+1);stop_peer(); }
void fixture_sntp(void)
{
  R(lwip_host_call(sntp_start,NULL)==0);fixture_wait_checked(&complete,sntp_check);R(lwip_host_call(sntp_finish,NULL)==0);
  snprintf(fixture_observations,sizeof(fixture_observations),",\"clock_notifications\":1,\"ntp_seconds\":3999999123,\"ntp_fraction\":1073741824");
}

static void netbios_start(void *arg)
{
  (void)arg;start_peer(31137,receive_answer);netbiosns_set_name("DOTCCUDP");netbiosns_init();
  unsigned char query[50]={0},name[16];memset(name,' ',sizeof(name));memcpy(name,"DOTCCUDP",8);name[15]=0;
  put16(query,0x4c57);put16(query+4,1);query[12]=32;
  for(unsigned i=0;i<16;i++){query[13+2*i]=(unsigned char)('A'+(name[i]>>4));query[14+2*i]=(unsigned char)('A'+(name[i]&15));}
  put16(query+46,0x20);put16(query+48,1);fixture_send(peer,137,query,sizeof(query));
}
static void netbios_finish(void *arg)
{
  (void)arg;R(answer_port==137&&answer_length==62&&get16(answer)==0x4c57);
  R((get16(answer+2)&0x800f)==0x8000&&get16(answer+6)==1);
  R(get16(answer+46)==0x20&&get16(answer+48)==1&&get16(answer+54)==6);
  R(!memcmp(answer+58,ip_2_ip4(&fixture_address),4));netbiosns_stop();stop_peer();
}
void fixture_netbios(void)
{
  R(lwip_host_call(netbios_start,NULL)==0);fixture_wait(&complete);R(lwip_host_call(netbios_finish,NULL)==0);
  snprintf(fixture_observations,sizeof(fixture_observations),",\"transaction\":19543,\"answers\":1,\"response_bytes\":%zu",answer_length);
}

static void name_result(struct netif *iface,u8_t result,s8_t slot)
{ (void)slot;R(iface==fixture_interface&&result==MDNS_PROBING_SUCCESSFUL);atomic_store(&probed,1); }
static void txt(struct mdns_service *service,void *arg)
{ (void)arg;R(mdns_resp_add_service_txtitem(service,"path=/fixture",13)==ERR_OK);txt_calls++; }
static void mdns_start(void *arg)
{
  (void)arg;start_peer(31535,receive_answer);atomic_store(&probed,0);
  mdns_resp_init();mdns_resp_register_name_result_cb(name_result);
  R(mdns_resp_add_netif(fixture_interface,"dotcc-udp-fixture")==ERR_OK);
  service_slot=mdns_resp_add_service(fixture_interface,"Fixture","_dotcc",DNSSD_PROTO_UDP,4242,txt,NULL);R(service_slot>=0);
}
static void mdns_query(void *arg)
{
  (void)arg;unsigned char query[64]={0};size_t length=12;put16(query,0x4d44);put16(query+4,1);
  const char *labels[]={"_dotcc","_udp","local"};
  for(unsigned i=0;i<3;i++){size_t n=strlen(labels[i]);query[length++]=(unsigned char)n;memcpy(query+length,labels[i],n);length+=n;}
  query[length++]=0;put16(query+length,12);length+=2;put16(query+length,1);length+=2;
  fixture_send(peer,5353,query,length);
}
static void domain(size_t *offset,char *out,size_t capacity)
{
  size_t cursor=*offset,end=0,written=0;unsigned hops=0;
  while(1) {
    R(cursor<answer_length&&hops++<128);unsigned n=answer[cursor++];
    if((n&0xc0)==0xc0){R(cursor<answer_length);size_t target=((n&63)<<8)|answer[cursor++];if(!end)end=cursor;cursor=target;continue;}
    R(n<=63&&cursor+n<=answer_length);
    if(!n){R(written<capacity);out[written]=0;*offset=end?end:cursor;return;}
    R(written+n+2<capacity);if(written)out[written++]='.';memcpy(out+written,answer+cursor,n);written+=n;cursor+=n;
  }
}
static void mdns_finish(void *arg)
{
  (void)arg;R(answer_port==5353&&answer_length>=12&&get16(answer)==0x4d44&&(get16(answer+2)&0x800f)==0x8000);
  size_t offset=12;char name[256],target[256];
  for(unsigned i=0;i<get16(answer+4);i++){domain(&offset,name,sizeof(name));R(offset+4<=answer_length);offset+=4;}
  unsigned records=get16(answer+6)+get16(answer+8)+get16(answer+10);int ptr=0,srv=0,text=0;
  for(unsigned i=0;i<records;i++) {
    domain(&offset,name,sizeof(name));R(offset+10<=answer_length);
    unsigned type=get16(answer+offset),length=get16(answer+offset+8);offset+=10;R(offset+length<=answer_length);size_t data=offset;
    if(type==12&&!strcmp(name,"_dotcc._udp.local")){domain(&data,target,sizeof(target));ptr|=!strcmp(target,"Fixture._dotcc._udp.local");}
    if(type==33&&!strcmp(name,"Fixture._dotcc._udp.local")){R(length>=7&&get16(answer+data+4)==4242);data+=6;domain(&data,target,sizeof(target));srv|=!strcmp(target,"dotcc-udp-fixture.local");}
    if(type==16&&!strcmp(name,"Fixture._dotcc._udp.local"))text|=length==14&&answer[data]==13&&!memcmp(answer+data+1,"path=/fixture",13);
    offset+=length;
  }
  R(ptr&&srv&&text&&txt_calls>0);R(mdns_resp_del_service(fixture_interface,(u8_t)service_slot)==ERR_OK);service_slot=-1;
  R(mdns_resp_remove_netif(fixture_interface)==ERR_OK);R(!mdns_resp_netif_active(fixture_interface));
  mdns_resp_register_name_result_cb(NULL);stop_peer();
}
void fixture_mdns(void)
{
  R(lwip_host_call(mdns_start,NULL)==0);fixture_wait(&probed);
  R(lwip_host_call(mdns_query,NULL)==0);fixture_wait(&complete);R(lwip_host_call(mdns_finish,NULL)==0);
  snprintf(fixture_observations,sizeof(fixture_observations),",\"service_port\":4242,\"txt_callbacks\":%u,\"service_removed\":true,\"interface_removed\":true",txt_calls);
}
