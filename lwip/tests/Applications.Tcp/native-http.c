#include "native-common.h"
#include "lwip/apps/http_client.h"
static httpc_connection_t settings;
static atomic_int done;
static unsigned results,headers;
static httpc_result_t result_code;
static u32_t response_status,response_length;
static unsigned char body[4096];
static size_t body_length;
static int reject_headers;
struct scenario{u16_t port;int reject;};
static err_t header_cb(httpc_state_t *state,void *arg,struct pbuf *p,u16_t length,u32_t content_length)
{R(state&&arg==&done&&p&&length>4&&content_length==5);headers++;return reject_headers?ERR_ABRT:ERR_OK;}
static err_t body_cb(void *arg,struct altcp_pcb *pcb,struct pbuf *p,err_t err)
{R(arg==&done&&p&&err==ERR_OK&&body_length+p->tot_len<=sizeof(body));R(pbuf_copy_partial(p,body+body_length,p->tot_len,0)==p->tot_len);body_length+=p->tot_len;altcp_recved(pcb,p->tot_len);pbuf_free(p);return ERR_OK;}
static void result_cb(void *arg,httpc_result_t result,u32_t length,u32_t status,err_t error)
{(void)error;R(arg==&done);results++;R(results==1);result_code=result;response_length=length;response_status=status;atomic_store(&done,1);}
static void begin(void *arg)
{
  struct scenario *test=arg;peer_start(test->port);results=headers=0;body_length=0;reject_headers=test->reject;atomic_store(&done,0);
  memset(&settings,0,sizeof(settings));settings.result_fn=result_cb;settings.headers_done_fn=header_cb;httpc_state_t *connection=NULL;
  R(httpc_get_file(&fixture_address,test->port,"/fixture.txt",&settings,body_cb,&done,&connection)==ERR_OK&&connection);
}
void fixture_http(void)
{
  for(unsigned i=0;i<4;i++)
  {
    struct scenario test={(u16_t)(18080+i),i==2};R(lwip_host_call(begin,&test)==0);peer_wait_until("\r\n\r\n");
    R(peer_message_length<sizeof(peer_message));peer_message[peer_message_length]=0;R(!strncmp((char*)peer_message,"GET /fixture.txt HTTP/1.1\r\n",27)&&strstr((char*)peer_message,"\r\nHost: 10.0.0.1\r\n"));
    if(i!=3)
    {
      char text[160];int code=i==1?404:200;snprintf(text,sizeof(text),"HTTP/1.1 %d %s\r\nContent-Length: 5\r\nConnection: close\r\n\r\n",code,code==200?"OK":"Not Found");
      peer_send_sync(text,19);lwip_host_poll();R(lwip_host_barrier()==0);peer_text_sync(text+19);if(i!=2)peer_text_sync("hello");
    }
    if(i!=2)peer_finish_sync();fixture_wait(&done);fixture_wait(&peer_finished);
    R(results==1&&result_code==(i==3?HTTPC_RESULT_ERR_CLOSED:i==2?HTTPC_RESULT_LOCAL_ABORT:HTTPC_RESULT_OK));
    if(i!=3)R(headers==1&&response_status==(i==1?404U:200U));
    if(i<2)R(response_length==5&&body_length==5&&!memcmp(body,"hello",5));
    peer_destroy_sync();
  }
  snprintf(fixture_observations,sizeof(fixture_observations),",\"segmented_headers\":true,\"complete_404_status_preserved\":true,\"header_policy_abort\":true,\"ordinary_early_fin\":true");
}
