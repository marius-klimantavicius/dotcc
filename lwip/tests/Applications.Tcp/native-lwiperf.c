#include "native-common.h"
#include "lwip/apps/lwiperf.h"
#include "lwip/priv/tcp_priv.h"
static atomic_int client_done,server_done;
static int client_kind,server_kind;
static u32_t client_bytes,client_ms,server_bytes;
static void *listener;
static void report(void *arg,enum lwiperf_report_type type,const ip_addr_t *local,u16_t lp,const ip_addr_t *remote,u16_t rp,u32_t bytes,u32_t ms,u32_t bandwidth)
{
  (void)bandwidth;R(local&&remote&&lp&&rp);
  if(arg==&client_done){client_kind=type;client_bytes=bytes;client_ms=ms;atomic_store(&client_done,1);}
  else{R(arg==&server_done);server_kind=type;server_bytes=bytes;atomic_store(&server_done,1);}
}
static void start_sink(void *arg)
{(void)arg;peer_start(5001);peer_set_capture_limit(48);R(lwiperf_start_tcp_client(&fixture_address,5001,LWIPERF_CLIENT,report,&client_done));}
static void tick(void *arg){(void)arg;sys_check_timeouts();}
static void discard(void *arg){(void)arg;R(peer_received()>=48);peer_set_discard(1);}
static void start_pair(void *arg)
{
  (void)arg;atomic_store(&client_done,0);listener=lwiperf_start_tcp_server(&fixture_address,5002,report,&server_done);R(listener);
  R(lwiperf_start_tcp_client(&fixture_address,5002,LWIPERF_CLIENT,report,&client_done));
}
void fixture_lwiperf(void)
{
  R(lwip_host_call(start_sink,NULL)==0);peer_wait_bytes(48);R(!memcmp(peer_message,peer_message+24,24)&&get32(peer_message)==0&&get32(peer_message+20)==(u32_t)-1000);
  R(lwip_host_call(discard,NULL)==0);fixture_advance_time(10001);R(lwip_host_call(tick,NULL)==0);fixture_wait(&client_done);fixture_wait(&peer_finished);
  R(client_kind==LWIPERF_TCP_DONE_CLIENT&&client_bytes>=48&&client_ms>=10000);peer_destroy_sync();
  /* The same global native stack can complete the second original client/server
   * flow while draining loopback. Let its unchanged 10-second duration elapse. */
  R(lwip_host_call(start_pair,NULL)==0);fixture_wait(&client_done);fixture_wait(&server_done);
  R(client_kind==LWIPERF_TCP_DONE_CLIENT&&client_bytes>=48&&server_kind==LWIPERF_TCP_DONE_SERVER&&server_bytes>=24);
  /* Retain original listener and its static callback context until process/core
   * teardown. Public lwiperf_abort is deliberately not exercised or qualified. */
  snprintf(fixture_observations,sizeof(fixture_observations),",\"original_client_completion\":true,\"original_server_completion\":true,\"listener_retained_until_owner_stop\":true");
}
