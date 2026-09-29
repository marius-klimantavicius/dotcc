#include "native-common.h"
#include "lwip/apps/mqtt.h"
static mqtt_client_t *client;
static atomic_int connections,requests,incoming_done;
static int statuses[8],results[8];
static unsigned char incoming[4096];
static size_t incoming_length;
static void connection_cb(mqtt_client_t *value,void *arg,mqtt_connection_status_t status)
{R(value==client&&arg==&client);int index=atomic_load(&connections);R(index<8);statuses[index]=status;atomic_store(&connections,index+1);}
static void request_cb(void *arg,err_t result)
{R(arg==&client);int index=atomic_load(&requests);R(index<8);results[index]=result;atomic_store(&requests,index+1);}
static void publication(void *arg,const char *topic,u32_t length)
{R(arg==&client&&!strcmp(topic,"dotcc/in")&&length==19);incoming_length=0;}
static void data(void *arg,const u8_t *bytes,u16_t length,u8_t flags)
{R(arg==&client&&incoming_length+length<=sizeof(incoming));memcpy(incoming+incoming_length,bytes,length);incoming_length+=length;if(flags&MQTT_DATA_FLAG_LAST)atomic_store(&incoming_done,1);}
static void open_client(void *arg)
{
  u16_t port=*(u16_t *)arg;peer_start(port);
  if(!client){client=mqtt_client_new();R(client);mqtt_set_inpub_callback(client,publication,data,&client);}
  struct mqtt_connect_client_info_t info;memset(&info,0,sizeof(info));info.client_id="dotcc-client";
  R(mqtt_client_connect(client,&fixture_address,port,connection_cb,&client,&info)==ERR_OK);
}
static unsigned char *body(void)
{size_t index=1;while(peer_message[index++]&128){R(index<5);}return peer_message+index;}
static int want_connections,want_requests;
static atomic_int count_ready;
static void check_counts(void *arg)
{(void)arg;if(atomic_load(&connections)==want_connections&&atomic_load(&requests)==want_requests)atomic_store(&count_ready,1);}
static void wait_counts(int connection_count,int request_count)
{want_connections=connection_count;want_requests=request_count;atomic_store(&count_ready,0);fixture_wait_checked(&count_ready,check_counts);}
static void connect_peer(u16_t port,u8_t code)
{
  int before=atomic_load(&connections);R(lwip_host_call(open_client,&port)==0);peer_wait_mqtt();
  static const unsigned char expected[]={0,4,'M','Q','T','T',4,2,0,0,0,12,'d','o','t','c','c','-','c','l','i','e','n','t'};
  R(peer_message[0]==0x10&&peer_message_length==26&&!memcmp(body(),expected,sizeof(expected)));
  unsigned char reply[]={0x20,2,0,code};peer_send_sync(reply,sizeof(reply));wait_counts(before+1,atomic_load(&requests));R(statuses[before]==code);
}
static void subscribe(void *arg){(void)arg;R(mqtt_sub_unsub(client,"dotcc/in",1,request_cb,&client,1)==ERR_OK);}
static void unsubscribe(void *arg){(void)arg;R(mqtt_sub_unsub(client,"dotcc/in",0,request_cb,&client,0)==ERR_OK);}
static void publish(void *arg){(void)arg;R(mqtt_publish(client,"dotcc/out","copied notification",19,1,0,request_cb,&client)==ERR_OK);}
static void disconnect(void *arg){(void)arg;mqtt_disconnect(client);R(!mqtt_client_is_connected(client));}
static void destroy(void *arg){(void)arg;mqtt_disconnect(client);mqtt_set_inpub_callback(client,NULL,NULL,NULL);mqtt_client_free(client);client=NULL;}
void fixture_mqtt(void)
{
  connect_peer(1883,0);R(lwip_host_call(subscribe,NULL)==0);peer_wait_mqtt();unsigned char *p=body();
  R(peer_message[0]==0x82&&get16(p+2)==8&&!memcmp(p+4,"dotcc/in",8)&&p[12]==1);
  unsigned char suback[]={0x90,3,p[0],p[1],1};peer_send_sync(suback,sizeof(suback));wait_counts(1,1);R(results[0]==ERR_OK);
  static const unsigned char message[]={0x32,31,0,8,'d','o','t','c','c','/','i','n',0,57,'c','o','p','i','e','d',' ','n','o','t','i','f','i','c','a','t','i','o','n'};
  peer_send_sync(message,sizeof(message));peer_wait_mqtt();R(peer_message_length==4&&!memcmp(peer_message,"\x40\x02\x00\x39",4));fixture_wait(&incoming_done);R(incoming_length==19&&!memcmp(incoming,"copied notification",19));
  R(lwip_host_call(publish,NULL)==0);peer_wait_mqtt();p=body();R(peer_message[0]==0x32&&get16(p)==9&&!memcmp(p+2,"dotcc/out",9)&&!memcmp(p+13,"copied notification",19));
  unsigned char puback[]={0x40,2,p[11],p[12]};peer_send_sync(puback,sizeof(puback));wait_counts(1,2);R(results[1]==ERR_OK);
  R(lwip_host_call(unsubscribe,NULL)==0);peer_wait_mqtt();p=body();R(peer_message[0]==0xa2);unsigned char unsuback[]={0xb0,2,p[0],p[1]};peer_send_sync(unsuback,sizeof(unsuback));wait_counts(1,3);R(results[2]==ERR_OK);
  peer_finish_sync();wait_counts(2,3);R(statuses[1]==MQTT_CONNECT_DISCONNECTED);fixture_wait(&peer_finished);peer_destroy_sync();
  connect_peer(1884,3);fixture_wait(&peer_finished);peer_destroy_sync();
  connect_peer(1885,0);R(lwip_host_call(subscribe,NULL)==0);peer_wait_mqtt();R(peer_message[0]==0x82);
  R(lwip_host_call(disconnect,NULL)==0);fixture_wait(&peer_finished);R(lwip_host_barrier()==0);R(atomic_load(&connections)==4&&atomic_load(&requests)==3);
  peer_destroy_sync();R(lwip_host_call(destroy,NULL)==0);
  snprintf(fixture_observations,sizeof(fixture_observations),",\"request_callbacks\":3,\"connection_callbacks\":4,\"copied_payload_bytes\":19,\"explicit_disconnect_callbacks_suppressed\":true");
}
