#include "native-common.h"
static struct tcp_pcb *listener,*connection;
static unsigned char capture[262144];
static size_t captured,total_received;
static int discard;
static size_t capture_limit=sizeof(capture);
atomic_int peer_connected,peer_finished;
unsigned char peer_message[262144];
size_t peer_message_length;
static atomic_int message_ready;
static const char *delimiter;
static size_t wanted;
static int mqtt_frame;
static void peer_error(void *arg,err_t err){(void)arg;(void)err;connection=NULL;R(0);}
static err_t receive(void *arg,struct tcp_pcb *pcb,struct pbuf *p,err_t err)
{
  (void)arg;R(err==ERR_OK&&pcb==connection);
  if(p==NULL){tcp_arg(pcb,NULL);tcp_recv(pcb,NULL);tcp_err(pcb,NULL);R(tcp_close(pcb)==ERR_OK);connection=NULL;atomic_store(&peer_finished,1);return ERR_OK;}
  total_received+=p->tot_len;
  if(!discard){size_t count=p->tot_len;if(capture_limit==sizeof(capture))R(captured+count<=sizeof(capture));if(count>capture_limit-captured)count=capture_limit-captured;R(pbuf_copy_partial(p,capture+captured,(u16_t)count,0)==count);captured+=count;}
  tcp_recved(pcb,p->tot_len);pbuf_free(p);return ERR_OK;
}
static err_t accept_peer(void *arg,struct tcp_pcb *pcb,err_t err)
{(void)arg;R(err==ERR_OK&&connection==NULL);connection=pcb;tcp_recv(pcb,receive);tcp_err(pcb,peer_error);atomic_store(&peer_connected,1);return ERR_OK;}
void peer_start(u16_t port)
{
  R(listener==NULL&&connection==NULL);captured=total_received=0;discard=0;capture_limit=sizeof(capture);
  atomic_store(&peer_connected,0);atomic_store(&peer_finished,0);
  listener=tcp_new_ip_type(IPADDR_TYPE_V4);R(listener);R(tcp_bind(listener,IP_ANY_TYPE,port)==ERR_OK);
  struct tcp_pcb *pcb=tcp_listen_with_backlog(listener,1);R(pcb);listener=pcb;tcp_accept(listener,accept_peer);
}
void peer_destroy(void)
{
  if(listener){tcp_accept(listener,NULL);R(tcp_close(listener)==ERR_OK);listener=NULL;}
  if(connection){tcp_arg(connection,NULL);tcp_recv(connection,NULL);tcp_err(connection,NULL);tcp_abort(connection);connection=NULL;}
}
void peer_send(const void *bytes,size_t length)
{R(connection&&length<=4096);R(tcp_write(connection,bytes,(u16_t)length,TCP_WRITE_FLAG_COPY)==ERR_OK);R(tcp_output(connection)==ERR_OK);}
void peer_finish_send(void){R(connection);R(tcp_shutdown(connection,0,1)==ERR_OK);}
void peer_set_discard(int value){discard=value;if(discard)captured=0;}
void peer_set_capture_limit(size_t value){R(value<=sizeof(capture)&&captured<=value);capture_limit=value;}
size_t peer_received(void){return total_received;}
static void read_message(void *arg)
{
  (void)arg;size_t count=0;
  if(mqtt_frame)
  {
    if(captured<2)return;size_t index=1,multiplier=1,length=0;
    for(;;){if(index>=captured)return;unsigned byte=capture[index++];length+=(byte&127)*multiplier;if(!(byte&128)){count=index+length;break;}multiplier*=128;R(index<=4);}
  }
  else if(delimiter)
  {
    size_t length=strlen(delimiter);for(size_t i=0;i+length<=captured;i++)if(!memcmp(capture+i,delimiter,length)){count=i+length;break;}
    if(!count)return;
  }
  else count=wanted;
  if(captured<count)return;R(count<=sizeof(peer_message));memcpy(peer_message,capture,count);memmove(capture,capture+count,captured-count);captured-=count;
  peer_message_length=count;atomic_store(&message_ready,1);
}
static void await_message(void){atomic_store(&message_ready,0);fixture_wait_checked(&message_ready,read_message);}
void peer_wait_mqtt(void){mqtt_frame=1;delimiter=NULL;await_message();mqtt_frame=0;}
void peer_wait_until(const char *text){delimiter=text;mqtt_frame=0;await_message();delimiter=NULL;}
void peer_wait_bytes(size_t length){wanted=length;delimiter=NULL;mqtt_frame=0;await_message();}
struct outgoing{const void *bytes;size_t length;};
static void send_core(void *arg){struct outgoing *request=arg;peer_send(request->bytes,request->length);}
void peer_send_sync(const void *bytes,size_t length){struct outgoing request={bytes,length};R(lwip_host_call(send_core,&request)==0);}
void peer_text_sync(const char *text){peer_send_sync(text,strlen(text));}
static void finish_core(void *arg){(void)arg;peer_finish_send();}
void peer_finish_sync(void){R(lwip_host_call(finish_core,NULL)==0);}
static void destroy_core(void *arg){(void)arg;peer_destroy();}
void peer_destroy_sync(void){R(lwip_host_call(destroy_core,NULL)==0);}
