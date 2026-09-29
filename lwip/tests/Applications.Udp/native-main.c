#include "native-common.h"
extern void lwip_host_initialize(void (*)(void *),void *);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
extern void *lwip_host_interface_add(const char *,const char *,const char *,const char *,const unsigned char *);
extern void lwip_host_interface_up(void *);
extern void lwip_host_interface_stop(void *);
extern void lwip_host_interface_free(void *);
struct netif *fixture_interface;
ip_addr_t fixture_address;
char fixture_observations[256];
static atomic_int initialized;
static unsigned observed_pcbs;
#ifdef DOTCC_UDP_VIRTUAL_CLOCK
extern u32_t fixture_real_sys_now(void);
static atomic_uint fixture_clock_offset;
u32_t sys_now(void) { return fixture_real_sys_now()+atomic_load(&fixture_clock_offset); }
void fixture_advance_time(u32_t milliseconds) { atomic_fetch_add(&fixture_clock_offset,milliseconds); }
#else
void fixture_advance_time(u32_t milliseconds) { (void)milliseconds;R(0); }
#endif
int lwip_host_frame_send(void *iface,const unsigned char *bytes,unsigned length)
{ (void)iface;(void)bytes;(void)length;return 0; } /* Multicast announcements leave the isolated fixture. */
static void ready(void *arg) { (void)arg;atomic_store(&initialized,1); }
static void setup(void *arg)
{
  static const unsigned char mac[]={2,0,0,0,0,1};(void)arg;
  fixture_interface=lwip_host_interface_add("10.0.0.1","255.255.255.0","0.0.0.0","fd00::1",mac);
  R(fixture_interface);lwip_host_interface_up(fixture_interface);
  R(ipaddr_aton("10.0.0.1",&fixture_address));
}
static void empty(void *arg) { (void)arg; }
void fixture_wait_checked(atomic_int *done,void (*check)(void *))
{
  u32_t start=sys_now();
  while(!atomic_load(done)) {
    if(check)R(lwip_host_call(check,NULL)==0);
    R((u32_t)(sys_now()-start)<10000);lwip_host_poll();R(lwip_host_barrier()==0);
    struct timespec pause={0,1000000};nanosleep(&pause,NULL);
  }
  R(lwip_host_barrier()==0);R(lwip_host_call(empty,NULL)==0);
}
void fixture_wait(atomic_int *done) { fixture_wait_checked(done,NULL); }
void fixture_send(struct udp_pcb *pcb,u16_t port,const unsigned char *bytes,size_t length)
{
  R(length<=65535);struct pbuf *p=pbuf_alloc(PBUF_TRANSPORT,(u16_t)length,PBUF_RAM);R(p);
  R(pbuf_take(p,bytes,(u16_t)length)==ERR_OK);err_t result=udp_sendto(pcb,p,&fixture_address,port);pbuf_free(p);R(result==ERR_OK);
}
static void count_pcbs(void *arg)
{ (void)arg;observed_pcbs=0;for(struct udp_pcb *p=udp_pcbs;p;p=p->next)observed_pcbs++; }
unsigned fixture_udp_count(void) { R(lwip_host_call(count_pcbs,NULL)==0);return observed_pcbs; }
static void stop_interface(void *arg) { (void)arg;lwip_host_interface_stop(fixture_interface); }
static void free_interface(void *arg) { (void)arg;lwip_host_interface_free(fixture_interface);fixture_interface=NULL; }
int main(int argc,char **argv)
{
  R(argc==2);if(!strcmp(argv[1],"--assert-control"))R(0);
  lwip_host_initialize(ready,NULL);fixture_wait(&initialized);R(lwip_host_call(setup,NULL)==0);
  unsigned before=fixture_udp_count();
  if(!strncmp(argv[1],"tftp-",5))fixture_tftp(argv[1]);
  else if(!strcmp(argv[1],"sntp"))fixture_sntp();
  else if(!strcmp(argv[1],"netbios"))fixture_netbios();
  else if(!strcmp(argv[1],"mdns"))fixture_mdns();
  else if(!strcmp(argv[1],"owned-tftp"))fixture_owned_tftp();
  else R(0);
  unsigned after=fixture_udp_count();
  /* Original mDNS retains its owner-internal global PCB; it has no deinit API.
   * Service/interface callbacks must still be unregistered by the case. */
  R(after==before+(!strcmp(argv[1],"mdns")?1U:0U));
  R(lwip_host_barrier()==0);R(lwip_host_call(stop_interface,NULL)==0);
  R(lwip_host_barrier()==0);R(lwip_host_call(free_interface,NULL)==0);
  R(lwip_host_stop_worker()==0);
#if !NO_SYS
  native_join_workers();
#endif
  printf("{\"case\":\"%s\",\"status\":\"passed\",\"udp_before\":%u,\"udp_after\":%u,\"service_callbacks_released\":true%s}\n",argv[1],before,after,fixture_observations);
  return 0;
}
