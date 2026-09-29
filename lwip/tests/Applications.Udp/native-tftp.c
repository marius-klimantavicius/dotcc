#include "native-common.h"
#include "lwip/apps/tftp_server.h"
#include "lwip/apps/tftp_client.h"
#include "lwip/timeouts.h"

/* One original native application at a time. The other endpoint is this
 * coherent UDP message peer, not a second instance of native global TFTP. */
enum direction { SERVER_READ,SERVER_WRITE,CLIENT_GET,CLIENT_PUT };
static enum direction direction;
static struct udp_pcb *peer;
static unsigned char expected[1025],stored[4096],peer_bytes[4096];
static size_t wanted,position,stored_length,peer_length,sent_length;
static unsigned opens,closes;
static int handle_token,handle_live,writing,initialized;
static atomic_int peer_done,storage_done;
static u16_t peer_block;
static int borrowed_admission,owned_fixture,pending_request,withhold_ack,storage_failure;
static unsigned expected_close_context;

static void *open_file(const char *name,const char *mode,u8_t write)
{
  R(!strcmp(name,"fixture.bin")&&!strcmp(mode,"octet")&&!handle_live);
  handle_live=1;writing=write!=0;position=stored_length=0;opens++;return &handle_token;
}
static void close_file(void *handle)
{
  R(handle==&handle_token&&handle_live);
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
  if(owned_fixture)R(lwip_owned_tftp_close_context()==expected_close_context);
#endif
  handle_live=0;closes++;atomic_store(&storage_done,1);
}
static int read_file(void *handle,void *buffer,int capacity)
{
  R(handle==&handle_token&&handle_live&&!writing&&capacity>=0);
  size_t size=wanted-position;if(size>(size_t)capacity)size=(size_t)capacity;
  memcpy(buffer,expected+position,size);position+=size;return (int)size;
}
static int write_file(void *handle,struct pbuf *p)
{
  R(handle==&handle_token&&handle_live&&writing&&stored_length+p->tot_len<=sizeof(stored));
  if(storage_failure)return -1;
  R(pbuf_copy_partial(p,stored+stored_length,p->tot_len,0)==p->tot_len);stored_length+=p->tot_len;return 0;
}
static void error_file(void *handle,int error,const char *message,int length)
{ R(handle==&handle_token);fprintf(stderr,"TFTP ordinary peer error %d: %.*s\n",error,length,message);R(0); }
static const struct tftp_context context={open_file,close_file,read_file,write_file,error_file};
static void send_block(u16_t destination,u16_t opcode,u16_t block,const unsigned char *data,size_t size)
{
  unsigned char bytes[516];R(size<=512);put16(bytes,opcode);put16(bytes+2,block);
  if(size)memcpy(bytes+4,data,size);fixture_send(peer,destination,bytes,size+4);
}
static void send_request(u16_t opcode)
{
  unsigned char bytes[32];static const char request[]="fixture.bin\0octet";
  put16(bytes,opcode);memcpy(bytes+2,request,sizeof(request));fixture_send(peer,69,bytes,sizeof(request)+2);
}
static void send_next_data(u16_t port)
{
  size_t remaining=wanted-peer_length;sent_length=remaining>512?512:remaining;
  send_block(port,3,peer_block,expected+peer_length,sent_length);peer_length+=sent_length;
}
static void receive_peer(void *arg,struct udp_pcb *pcb,struct pbuf *p,const ip_addr_t *address,u16_t port)
{
  (void)arg;(void)pcb;R(ip_addr_eq(address,&fixture_address));
  unsigned char bytes[600];R(p->tot_len<=sizeof(bytes));size_t length=p->tot_len;
  R(pbuf_copy_partial(p,bytes,p->tot_len,0)==p->tot_len);pbuf_free(p);
  R(length>=2);unsigned opcode=get16(bytes);
  /* Output-only diagnosis: preserve the peer's transmitted bytes and flow. */
  fprintf(stderr,"TFTP peer received opcode=%u length=%zu",opcode,length);
  if(length>=4&&(opcode==3||opcode==4||opcode==5))fprintf(stderr," %s=%u",opcode==5?"error":"block",get16(bytes+2));
  if(opcode==5&&length>=4)fprintf(stderr," message=%.*s",(int)(length-4),(const char*)bytes+4);
  fputc('\n',stderr);
  if(storage_failure&&opcode==5){R(length>=4&&get16(bytes+2)==2);atomic_store(&peer_done,1);return;}
  if(direction==SERVER_READ||direction==CLIENT_PUT) {
    if(direction==CLIENT_PUT&&opcode==2) {
      static const char request[]="fixture.bin\0octet";
      R(length==sizeof(request)+2&&!memcmp(bytes+2,request,sizeof(request)));
      send_block(port,4,0,NULL,0);return;
    }
    R(opcode==3&&length>=4&&length<=516&&get16(bytes+2)==peer_block);
    if(withhold_ack){atomic_store(&peer_done,1);return;}
    R(peer_length+length-4<=sizeof(peer_bytes));memcpy(peer_bytes+peer_length,bytes+4,length-4);peer_length+=length-4;
    send_block(port,4,peer_block,NULL,0);peer_block++;
    if(length<516) { R(peer_length==wanted&&!memcmp(peer_bytes,expected,wanted));atomic_store(&peer_done,1); }
  } else {
    if(direction==CLIENT_GET&&opcode==1) {
      static const char request[]="fixture.bin\0octet";
      R(length==sizeof(request)+2&&!memcmp(bytes+2,request,sizeof(request)));
      if(pending_request){atomic_store(&peer_done,1);return;}
      send_next_data(port);return;
    }
    R(opcode==4&&length==4);
    if(direction==SERVER_WRITE&&get16(bytes+2)==0) { R(peer_length==0&&peer_block==1);send_next_data(port);return; }
    R(get16(bytes+2)==peer_block);
    if(sent_length<512)atomic_store(&peer_done,1);
    else { peer_block++;send_next_data(port); }
  }
}
static void start(void *arg)
{
  (void)arg;handle_live=0;position=stored_length=peer_length=sent_length=0;peer_block=1;
  atomic_store(&peer_done,0);atomic_store(&storage_done,0);
  for(size_t i=0;i<wanted;i++)expected[i]=(unsigned char)(i%251);
  peer=udp_new_ip_type(IPADDR_TYPE_V4);R(peer);R(udp_bind(peer,IP_ANY_TYPE,31069)==ERR_OK);udp_recv(peer,receive_peer,NULL);
  err_t result;
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
  if(owned_fixture)result=lwip_owned_tftp_start(direction<=SERVER_WRITE?LWIP_TFTP_MODE_SERVER:LWIP_TFTP_MODE_CLIENT,&context);
  else
#endif
  result=direction<=SERVER_WRITE?tftp_init_server(&context):tftp_init_client(&context);
  R(result==ERR_OK);initialized=1;
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
  if(borrowed_admission)R(lwip_owned_tftp_start(LWIP_TFTP_MODE_CLIENT,&context)==ERR_USE);
#endif
  if(direction==SERVER_READ)send_request(1);
  else if(direction==SERVER_WRITE)send_request(2);
  else {
    void *handle=open_file("fixture.bin","octet",direction==CLIENT_GET);
    result=direction==CLIENT_GET?tftp_get(handle,&fixture_address,31069,"fixture.bin",TFTP_MODE_OCTET):tftp_put(handle,&fixture_address,31069,"fixture.bin",TFTP_MODE_OCTET);
    if(result!=ERR_OK) { /* Original APIs retain handle before attempting send. */
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
      if(owned_fixture){expected_close_context=256;lwip_owned_tftp_stop();}else
#endif
      tftp_cleanup();initialized=0;R(!handle_live);R(result==ERR_OK);
    }
  }
}
static void finish(void *arg)
{
  (void)arg;R(!handle_live);
  if(!pending_request&&!storage_failure&&(direction==SERVER_WRITE||direction==CLIENT_GET))R(stored_length==wanted&&!memcmp(stored,expected,wanted));
  if(initialized){
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
    if(owned_fixture)lwip_owned_tftp_stop();else
#endif
    tftp_cleanup();initialized=0;
  }
  udp_recv(peer,NULL,NULL);udp_remove(peer);peer=NULL;R(opens==closes);
}
void fixture_tftp(const char *name)
{
  opens=closes=0;
  if(!strcmp(name,"tftp-server")) {
    static const size_t sizes[]={0,512,1025};
    for(unsigned i=0;i<3;i++)for(unsigned write=0;write<2;write++) {
      wanted=sizes[i];direction=write?SERVER_WRITE:SERVER_READ;
      expected_close_context=write?3:4;
      R(lwip_host_call(start,NULL)==0);fixture_wait(&peer_done);fixture_wait(&storage_done);R(lwip_host_call(finish,NULL)==0);
    }
    R(opens==6&&closes==6);
  } else {
    R(!strcmp(name,"tftp-get")||!strcmp(name,"tftp-put"));wanted=1025;
    direction=!strcmp(name,"tftp-get")?CLIENT_GET:CLIENT_PUT;
    expected_close_context=direction==CLIENT_GET?3:4;
    R(lwip_host_call(start,NULL)==0);fixture_wait(&peer_done);fixture_wait(&storage_done);R(lwip_host_call(finish,NULL)==0);
    R(opens==1&&closes==1);
  }
  snprintf(fixture_observations,sizeof(fixture_observations),",\"storage_opens\":%u,\"storage_closes\":%u,\"live_storage_handles\":%d",opens,closes,handle_live);
}

#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
static void stop_pending(void *arg)
{ (void)arg;R(handle_live&&closes==0);expected_close_context=256;lwip_owned_tftp_stop();initialized=0;R(!handle_live&&closes==1); }
static void tick_original(void *arg) { (void)arg;sys_check_timeouts(); }
#endif
void fixture_owned_tftp(void)
{
#ifdef DOTCC_UDP_PRODUCT_BOUNDARIES
  borrowed_admission=1;owned_fixture=0;fixture_tftp("tftp-get");borrowed_admission=0;
  owned_fixture=1;
  fixture_tftp("tftp-server");fixture_tftp("tftp-get");fixture_tftp("tftp-put");
  /* Original client has no request timer. Qualify explicit cancellation only. */
  opens=closes=0;pending_request=1;direction=CLIENT_GET;wanted=1025;
  R(lwip_host_call(start,NULL)==0);fixture_wait(&peer_done);R(!atomic_load(&storage_done));
  R(lwip_host_call(stop_pending,NULL)==0);R(lwip_host_call(finish,NULL)==0);pending_request=0;
  /* The valid DATA payload encounters a normal application storage failure. */
  opens=closes=0;storage_failure=1;direction=SERVER_WRITE;wanted=9;expected_close_context=3;
  R(lwip_host_call(start,NULL)==0);fixture_wait(&peer_done);fixture_wait(&storage_done);R(lwip_host_call(finish,NULL)==0);storage_failure=0;
  /* No ACK is sent. Advance the host clock, then execute original timeouts;
   * original TFTP retry/expiry functions and scheduling remain unchanged. */
  opens=closes=0;withhold_ack=1;direction=SERVER_READ;wanted=3;expected_close_context=0;
  R(lwip_host_call(start,NULL)==0);fixture_wait(&peer_done);
  for(unsigned i=0;i<80&&!atomic_load(&storage_done);i++){fixture_advance_time(1100);R(lwip_host_call(tick_original,NULL)==0);R(lwip_host_barrier()==0);lwip_host_poll();}
  R(atomic_load(&storage_done));R(lwip_host_call(finish,NULL)==0);withhold_ack=0;
  snprintf(fixture_observations,sizeof(fixture_observations),",\"borrowed_start_preserved\":true,\"original_success_close_contexts\":true,\"explicit_cancel\":true,\"storage_error\":true,\"original_server_timeout\":true");
#else
  R(0); /* This case requires the explicit matching product adapter. */
#endif
}
