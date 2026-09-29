#include "native-common.h"
#include "lwip/priv/tcp_priv.h"
extern void lwip_host_initialize(void (*)(void *),void *);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
extern void *lwip_host_interface_add(const char *,const char *,const char *,const char *,const unsigned char *);
extern void lwip_host_interface_up(void *);
extern void lwip_host_interface_stop(void *);
extern void lwip_host_interface_free(void *);
extern u32_t fixture_real_sys_now(void);
static atomic_uint clock_offset;
u32_t sys_now(void){return fixture_real_sys_now()+atomic_load(&clock_offset);}
void fixture_advance_time(u32_t milliseconds){atomic_fetch_add(&clock_offset,milliseconds);}
ip_addr_t fixture_address;
char fixture_observations[512];
static struct netif *interface;
static atomic_int initialized;
static unsigned active,listeners,timewait;
int lwip_host_frame_send(void *iface,const unsigned char *bytes,unsigned length)
{(void)iface;(void)bytes;(void)length;return 0;}
static void ready(void *arg){(void)arg;atomic_store(&initialized,1);}
static void setup(void *arg)
{
  static const unsigned char mac[]={2,0,0,0,0,1};(void)arg;
  interface=lwip_host_interface_add("10.0.0.1","255.255.255.0","0.0.0.0","fd00::1",mac);R(interface);
  lwip_host_interface_up(interface);R(ipaddr_aton("10.0.0.1",&fixture_address));
}
static void empty(void *arg){(void)arg;}
void fixture_wait_checked(atomic_int *done,void (*check)(void *))
{
  /* Deadline uses real monotonic host time even in the lwiperf clock scenario. */
  u32_t start=fixture_real_sys_now();
  while(!atomic_load(done))
  {
    if(check)R(lwip_host_call(check,NULL)==0);
    R((u32_t)(fixture_real_sys_now()-start)<15000);lwip_host_poll();R(lwip_host_barrier()==0);
    struct timespec pause={0,1000000};nanosleep(&pause,NULL);
  }
  R(lwip_host_barrier()==0);R(lwip_host_call(empty,NULL)==0);
}
void fixture_wait(atomic_int *done){fixture_wait_checked(done,NULL);}
static void counts(void *arg)
{
  (void)arg;active=listeners=timewait=0;
  for(struct tcp_pcb *p=tcp_active_pcbs;p;p=p->next)active++;
  for(struct tcp_pcb_listen *p=tcp_listen_pcbs.listen_pcbs;p;p=p->next)listeners++;
  for(struct tcp_pcb *p=tcp_tw_pcbs;p;p=p->next)timewait++;
}
static void stop_interface(void *arg){(void)arg;lwip_host_interface_stop(interface);}
static void free_interface(void *arg){(void)arg;lwip_host_interface_free(interface);interface=NULL;}
int main(int argc,char **argv)
{
  R(argc==2);if(!strcmp(argv[1],"--assert-control"))R(0);
  lwip_host_initialize(ready,NULL);fixture_wait(&initialized);R(lwip_host_call(setup,NULL)==0);
  if(!strcmp(argv[1],"mqtt"))fixture_mqtt();
  else if(!strcmp(argv[1],"smtp"))fixture_smtp();
  else if(!strcmp(argv[1],"lwiperf"))fixture_lwiperf();
  else if(!strcmp(argv[1],"http-client"))fixture_http();
  else if(!strcmp(argv[1],"proxy-connect"))fixture_proxy();
  else R(0);
  R(lwip_host_barrier()==0);R(lwip_host_call(counts,NULL)==0);
  R(listeners==(!strcmp(argv[1],"lwiperf")?1U:0U));
  R(lwip_host_call(stop_interface,NULL)==0);R(lwip_host_barrier()==0);R(lwip_host_call(free_interface,NULL)==0);
  R(lwip_host_stop_worker()==0);
#if !NO_SYS
  native_join_workers();
#endif
  printf("{\"case\":\"%s\",\"status\":\"passed\",\"tcp_active\":%u,\"tcp_listeners\":%u,\"tcp_timewait\":%u,\"scope\":\"finite original algorithms; owning service cancellation not qualified\"%s}\n",argv[1],active,listeners,timewait,fixture_observations);
  return 0;
}
