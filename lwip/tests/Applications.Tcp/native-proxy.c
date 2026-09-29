#include "native-common.h"
#include "lwip/altcp.h"
#include "lwip/apps/altcp_proxyconnect.h"
static struct altcp_proxyconnect_config settings;
static struct altcp_pcb *connection;
static atomic_int connected,received,finished;
static err_t connected_cb(void *arg,struct altcp_pcb *pcb,err_t error)
{R(arg==&connected&&pcb==connection&&error==ERR_OK);atomic_store(&connected,1);return ERR_OK;}
static void error_cb(void *arg,err_t error){(void)arg;(void)error;connection=NULL;R(0);}
static err_t receive_cb(void *arg,struct altcp_pcb *pcb,struct pbuf *p,err_t err)
{
  static unsigned char response[21];static size_t length;
  R(arg==&connected&&pcb==connection&&err==ERR_OK);
  if(!p){altcp_arg(pcb,NULL);altcp_recv(pcb,NULL);altcp_err(pcb,NULL);R(altcp_close(pcb)==ERR_OK);connection=NULL;atomic_store(&finished,1);return ERR_OK;}
  R(length+p->tot_len<=sizeof(response));R(pbuf_copy_partial(p,response+length,p->tot_len,0)==p->tot_len);length+=p->tot_len;altcp_recved(pcb,p->tot_len);pbuf_free(p);
  if(length==sizeof(response)){R(!memcmp(response,"server tunnel payload",21));atomic_store(&received,1);}return ERR_OK;
}
static void begin(void *arg)
{
  (void)arg;peer_start(18084);memset(&settings,0,sizeof(settings));ip_addr_copy(settings.proxy_addr,fixture_address);settings.proxy_port=18084;
  connection=altcp_proxyconnect_new_tcp(&settings,IPADDR_TYPE_V4);R(connection);altcp_arg(connection,&connected);altcp_recv(connection,receive_cb);altcp_err(connection,error_cb);
  ip_addr_t target;R(ipaddr_aton("10.0.0.9",&target));R(altcp_connect(connection,&target,8080,connected_cb)==ERR_OK);
}
static void send_tunnel(void *arg)
{(void)arg;R(connection&&altcp_write(connection,"client tunnel payload",21,TCP_WRITE_FLAG_COPY)==ERR_OK);R(altcp_output(connection)==ERR_OK);}
void fixture_proxy(void)
{
  R(lwip_host_call(begin,NULL)==0);peer_wait_until("\r\n\r\n");static const char prefix[]="CONNECT 10.0.0.9:8080 HTTP/1.1\r\n";R(peer_message_length>=sizeof(prefix)-1&&!memcmp(peer_message,prefix,sizeof(prefix)-1));
  /* Positive coherent handshake only. Status interpretation is an upstream
   * parsing TODO; normal segmentation/coalescing are separate tunnel limits.
   * None is patched or claimed as qualified by this positive case. */
  peer_text_sync("HTTP/1.1 200 Connection Established\r\n\r\n");fixture_wait(&connected);
  R(lwip_host_call(send_tunnel,NULL)==0);peer_wait_bytes(21);R(!memcmp(peer_message,"client tunnel payload",21));
  peer_text_sync("server tunnel payload");fixture_wait(&received);peer_finish_sync();fixture_wait(&finished);fixture_wait(&peer_finished);peer_destroy_sync();
  snprintf(fixture_observations,sizeof(fixture_observations),",\"cleartext_connect_200\":true,\"bidirectional_copied_tunnel\":true,\"original_normal_close\":true");
}
