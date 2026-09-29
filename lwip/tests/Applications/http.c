#include "lwip/init.h"
#include "lwip/tcp.h"
#include "lwip/priv/tcp_priv.h"
#include "lwip/apps/httpd.h"
#include "lwip/apps/fs.h"
#include "lwip/altcp.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define REQUIRE(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_call(void(*)(void*),void*);extern void lwip_host_poll(void);extern int lwip_host_stop_worker(void);extern void native_join_workers(void);
static atomic_int ready,connected,finished;
static struct tcp_pcb*client;
static unsigned char response[32768];static size_t response_length;
static unsigned opened,closed,ssi_count,cgi_count,post_count;
static unsigned char post[100];static size_t post_length;
const unsigned char*lwip_host_http_open(const char*name,int*length){const char*data=!strcmp(name,"/index.txt")?"alpha":!strcmp(name,"/tag.shtml")?"hello <!--#name-->!":!strcmp(name,"/done.txt")?"stored":NULL;if(!data)return NULL;*length=(int)strlen(data);unsigned char*out=malloc((size_t)*length);REQUIRE(out);memcpy(out,data,(size_t)*length);opened++;return out;}
void lwip_host_http_close(const unsigned char*data){closed++;free((void*)data);}
int lwip_host_post_begin(void*connection,const char*uri,int length){(void)connection;REQUIRE(!strcmp(uri,"/submit")&&length==11);post_length=0;return 1;}
int lwip_host_post_data(void*connection,const unsigned char*data,unsigned length){(void)connection;REQUIRE(post_length+length<=sizeof(post));memcpy(post+post_length,data,length);post_length+=length;return 1;}
void lwip_host_post_finish(void*connection,char*uri,unsigned capacity){(void)connection;REQUIRE(post_length==11&&!memcmp(post,"hello world",11)&&capacity>=10);strcpy(uri,"/done.txt");post_count++;}
static u16_t ssi(int index,char*out,int capacity,u16_t part,u16_t*next){REQUIRE(index==0&&part==0&&capacity>=5);memcpy(out,"world",5);*next=0xffff;ssi_count++;return 5;}
static const char*cgi(int index,int count,char**names,char**values){REQUIRE(index==0&&count==1&&!strcmp(names[0],"x")&&!strcmp(values[0],"one%20two"));cgi_count++;return"/index.txt";}
static const tCGI handlers[]={{"/echo.cgi",cgi}};static const char*tags[]={"name"};
static void initialize(void*arg){(void)arg;http_set_cgi_handlers(handlers,1);http_set_ssi_handler(ssi,tags,1);httpd_init();atomic_store(&ready,1);}
static void wait_flag(atomic_int*flag){u32_t end=sys_now()+10000;while(!atomic_load(flag)){REQUIRE((int)(end-sys_now())>0);lwip_host_poll();struct timespec t={0,1000000};nanosleep(&t,NULL);}}
static err_t receive(void*arg,struct tcp_pcb*pcb,struct pbuf*p,err_t error){(void)arg;REQUIRE(error==0);if(!p){tcp_recv(pcb,NULL);tcp_err(pcb,NULL);REQUIRE(tcp_close(pcb)==0);client=NULL;atomic_store(&finished,1);return 0;}REQUIRE(response_length+p->tot_len<sizeof(response));response_length+=pbuf_copy_partial(p,response+response_length,p->tot_len,0);tcp_recved(pcb,p->tot_len);pbuf_free(p);return 0;}
static void error(void*arg,err_t err){(void)arg;(void)err;REQUIRE(0);}
static err_t connect_done(void*arg,struct tcp_pcb*pcb,err_t err){(void)arg;(void)pcb;REQUIRE(err==0);atomic_store(&connected,1);return 0;}
static void connect_client(void*arg){int v6=*(int*)arg;ip_addr_t address;REQUIRE(ipaddr_aton(v6?"::1":"127.0.0.1",&address));client=tcp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);REQUIRE(client);tcp_recv(client,receive);tcp_err(client,error);REQUIRE(tcp_connect(client,&address,80,connect_done)==0);}
static void write_request(void*arg){const char*request=arg;REQUIRE(tcp_write(client,request,(u16_t)strlen(request),TCP_WRITE_FLAG_COPY)==0&&tcp_output(client)==0);}
static const unsigned char*request(const char*text,int v6,int status,size_t*length){atomic_store(&connected,0);atomic_store(&finished,0);response_length=0;REQUIRE(lwip_host_call(connect_client,&v6)==0);wait_flag(&connected);char fragment[8];memcpy(fragment,text,7);fragment[7]=0;REQUIRE(lwip_host_call(write_request,fragment)==0);lwip_host_poll();REQUIRE(lwip_host_call(write_request,(void*)(text+7))==0);wait_flag(&finished);response[response_length]=0;char*body=strstr((char*)response,"\r\n\r\n");REQUIRE(body&&atoi((char*)response+9)==status);body+=4;*length=response_length-(size_t)(body-(char*)response);return(unsigned char*)body;}
static void get(const char*path,const unsigned char*expected,size_t size,int v6){char text[200];snprintf(text,sizeof(text),"GET %s HTTP/1.0\r\nHost: fixture\r\nConnection: close\r\n\r\n",path);size_t length;const unsigned char*body=request(text,v6,200,&length);REQUIRE(length==size&&!memcmp(body,expected,size));}
static void close_server(void*arg){(void)arg;struct tcp_pcb_listen*listener=tcp_listen_pcbs.listen_pcbs;while(listener&&listener->local_port!=80)listener=listener->next;REQUIRE(listener&&listener->callback_arg);REQUIRE(altcp_close(listener->callback_arg)==0);REQUIRE(opened==closed);}
int main(int argc,char**argv){if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(initialize,NULL);wait_flag(&ready);
 get("/index.txt",(const unsigned char*)"alpha",5,0);get("/tag.shtml",(const unsigned char*)"hello <!--#name-->world!",24,1);get("/echo.cgi?x=one%20two",(const unsigned char*)"alpha",5,0);
 size_t length;request("GET /absent HTTP/1.0\r\nHost: fixture\r\nConnection: close\r\n\r\n",0,404,&length);const unsigned char*body=request("POST /submit HTTP/1.0\r\nHost: fixture\r\nContent-Length: 11\r\nConnection: close\r\n\r\nhello world",0,200,&length);REQUIRE(length==6&&!memcmp(body,"stored",6));
 if(argc>1&&!strcmp(argv[1],"--generated")){get("/empty.txt",(const unsigned char*)"",0,0);get("/odd.txt",(const unsigned char*)"abcde",5,0);unsigned char data[4097];for(unsigned i=0;i<sizeof(data);i++)data[i]=(unsigned char)(i%251);get("/multiple.txt",data,sizeof(data),0);}
 REQUIRE(ssi_count==1&&cgi_count==1&&post_count==1);REQUIRE(lwip_host_call(close_server,NULL)==0);REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 printf("{\"http_transactions\":5,\"ssi\":1,\"cgi_raw_parameters\":1,\"post\":1,\"custom_files_released\":true,\"generated_files\":%d,\"fs_file_size\":%zu}\n",argc>1&&!strcmp(argv[1],"--generated")?3:0,sizeof(struct fs_file));return 0;}
