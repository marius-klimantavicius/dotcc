/* Positive, independently encoded TCP peer and ordinary original API options.
 * Complete valid data segments are prepared once and delivered out of order.
 * No packet byte is mutated after preparation. */
#include "lwip/init.h"
#include "lwip/tcp.h"
#include "lwip/tcpip.h"
#include "lwip/timeouts.h"
#include "lwip/udp.h"
#include "lwip/sockets.h"
#include "lwip/api.h"
#include "lwip/stats.h"
#include "lwip/priv/sockets_priv.h"
#include <stdatomic.h>
#include <stddef.h>
#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <errno.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s errno=%d\n",__FILE__,__LINE__,#x,errno);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_call(void(*)(void*),void*);
extern int lwip_host_barrier(void);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
static atomic_int ready,connected;
static atomic_uint frame_count;
static atomic_uint fixture_clock=1000;
u32_t sys_now(void){return atomic_load(&fixture_clock);}
static struct netif iface;
static struct tcp_pcb *accepted;
static unsigned char frames[64][2048],received[32];
static unsigned sizes[64],received_count,removed,reused,timed,canceled;
static int client_fd,connect_result;
static unsigned memory_before,pool_before[6];
static const int pool_kinds[6]={MEMP_PBUF_POOL,MEMP_TCP_PCB,MEMP_UDP_PCB,MEMP_NETCONN,MEMP_NETBUF,MEMP_TCP_PCB_LISTEN};
static FILE *wire;
static void put16(unsigned char*p,unsigned n){p[0]=(unsigned char)(n>>8);p[1]=(unsigned char)n;}
static void put32(unsigned char*p,u32_t n){p[0]=(unsigned char)(n>>24);p[1]=(unsigned char)(n>>16);p[2]=(unsigned char)(n>>8);p[3]=(unsigned char)n;}
static unsigned word(const unsigned char*p){return ((unsigned)p[0]<<8)|p[1];}
static u32_t dword(const unsigned char*p){return ((u32_t)word(p)<<16)|word(p+2);}
static unsigned sum(const unsigned char*p,unsigned n,unsigned total){for(unsigned i=0;i<n;i+=2)total+=(p[i]<<8)|(i+1<n?p[i+1]:0);while(total>>16)total=(total&65535)+(total>>16);return total;}
static void record(const char*direction,const unsigned char*p,unsigned n){fprintf(wire,"%s ",direction);for(unsigned i=0;i<n;i++)fprintf(wire,"%02x",p[i]);fputc('\n',wire);fflush(wire);}
static err_t output(struct netif*n,struct pbuf*p,const ip4_addr_t*target){(void)n;(void)target;R(frame_count<64&&p->tot_len<=2048);sizes[frame_count]=p->tot_len;R(pbuf_copy_partial(p,frames[frame_count],p->tot_len,0)==p->tot_len);record("stack",frames[frame_count],p->tot_len);frame_count++;return ERR_OK;}
static err_t initialize_netif(struct netif*n){n->mtu=1500;n->flags=NETIF_FLAG_BROADCAST;n->output=output;return ERR_OK;}
static void on_remove(struct netif*n){R(n==&iface);removed++;}
static unsigned packet(unsigned char*out,unsigned protocol,u16_t destination,u32_t seq,u32_t ack,unsigned flags,u32_t timestamp,u32_t echo,const unsigned char*data,unsigned length,int syn)
{
  unsigned options=syn?24:12,tcp_length=20+options+length,total=20+tcp_length;
  memset(out,0,total);out[0]=0x45;put16(out+2,total);out[8]=64;out[9]=(unsigned char)protocol;out[12]=out[16]=10;out[13]=out[17]=44;out[15]=2;out[19]=1;put16(out+10,~sum(out,20,0));
  unsigned char*t=out+20;put16(t,50001);put16(t+2,destination);put32(t+4,seq);put32(t+8,ack);t[12]=(unsigned char)(((20+options)/4)<<4);t[13]=(unsigned char)flags;put16(t+14,32768);
  unsigned at=20;if(syn){t[at++]=2;t[at++]=4;put16(t+at,1440);at+=2;t[at++]=4;t[at++]=2;t[at++]=1;t[at++]=3;t[at++]=3;t[at++]=2;t[at++]=1;t[at++]=1;}
  t[at++]=1;t[at++]=1;t[at++]=8;t[at++]=10;put32(t+at,timestamp);at+=4;put32(t+at,echo);at+=4;R(at==20+options);
  if(length)memcpy(t+at,data,length);put16(t+16,~sum(t,tcp_length,sum(out+12,8,6+tcp_length)));return total;
}
static void inject(const unsigned char*bytes,unsigned length){record("peer",bytes,length);struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)length,PBUF_RAM);R(p&&pbuf_take(p,bytes,(u16_t)length)==ERR_OK);R(ip_input(p,&iface)==ERR_OK);}
static const unsigned char*option(const unsigned char*p,unsigned length,unsigned wanted)
{R(length>=40&&p[9]==6);unsigned end=20+(p[32]>>4)*4;R(end<=length);for(unsigned at=40;at<end;){unsigned kind=p[at];if(!kind)break;if(kind==1){at++;continue;}R(at+1<end&&p[at+1]>=2&&at+p[at+1]<=end);if(kind==wanted)return p+at;at+=p[at+1];}return NULL;}
static err_t receive(void*arg,struct tcp_pcb*pcb,struct pbuf*p,err_t error){(void)arg;R(error==ERR_OK&&p&&received_count+p->tot_len<=sizeof(received));R(pbuf_copy_partial(p,received+received_count,p->tot_len,0)==p->tot_len);received_count+=p->tot_len;tcp_recved(pcb,p->tot_len);pbuf_free(p);return ERR_OK;}
static err_t accept_peer(void*arg,struct tcp_pcb*pcb,err_t error){(void)arg;R(error==ERR_OK);accepted=pcb;tcp_recv(pcb,receive);tcp_backlog_delayed(pcb);return ERR_OK;}
static void setup(void*arg)
{(void)arg;memory_before=lwip_stats.mem.used;for(unsigned i=0;i<6;i++)pool_before[i]=lwip_stats.memp[pool_kinds[i]]->used;ip4_addr_t ip,mask,zero={0};R(ip4addr_aton("10.44.0.1",&ip)&&ip4addr_aton("255.255.255.0",&mask));R(netif_add(&iface,&ip,&mask,&zero,NULL,initialize_netif,ip_input));netif_set_up(&iface);netif_set_link_up(&iface);netif_set_default(&iface);netif_set_remove_callback(&iface,on_remove);}
static void tcp_options(void*arg)
{
  (void)arg;ip_addr_t local;R(ipaddr_aton("10.44.0.1",&local));struct tcp_pcb*listener=tcp_new();R(listener&&tcp_bind(listener,&local,50000)==ERR_OK);listener=tcp_listen_with_backlog(listener,1);R(listener);tcp_accept(listener,accept_peer);
  unsigned char syn[128],ack[128],first[128],later[128];const u32_t base=0x10203040U;
  unsigned n=packet(syn,6,50000,base,0,2,100,0,NULL,0,1);frame_count=0;inject(syn,n);R(frame_count==1&&(frames[0][33]&0x12)==0x12);
  const unsigned char*ws=option(frames[0],sizes[0],3),*ts=option(frames[0],sizes[0],8),*sack=option(frames[0],sizes[0],4);R(ws&&ws[1]==3&&ws[2]==2&&ts&&ts[1]==10&&dword(ts+6)==100&&sack&&sack[1]==2);
  u32_t native_seq=dword(frames[0]+24),native_time=dword(ts+2);n=packet(ack,6,50000,base+1,native_seq+1,16,101,native_time,NULL,0,0);inject(ack,n);
  R(accepted&&(accepted->flags&TF_TIMESTAMP)&&(accepted->flags&TF_WND_SCALE)&&(accepted->flags&TF_SACK));R(accepted->snd_scale==2&&accepted->rcv_scale==2&&accepted->rcv_wnd==TCP_WND);R(((struct tcp_pcb_listen*)listener)->accepts_pending==1);tcp_backlog_accepted(accepted);R(((struct tcp_pcb_listen*)listener)->accepts_pending==0);
  /* Encode both legal segments before delivery; later arrives first unchanged. */
  unsigned a=packet(first,6,50000,base+1,native_seq+1,24,102,native_time,(const unsigned char*)"abcd",4,0);
  unsigned b=packet(later,6,50000,base+5,native_seq+1,24,102,native_time,(const unsigned char*)"efgh",4,0);
  frame_count=0;inject(later,b);R(received_count==0&&accepted->ooseq);R(frame_count>0);sack=option(frames[frame_count-1],sizes[frame_count-1],5);R(sack&&sack[1]==10&&dword(sack+2)==base+5&&dword(sack+6)==base+9);
  inject(first,a);R(received_count==8&&!memcmp(received,"abcdefgh",8)&&accepted->ooseq==NULL);tcp_abort(accepted);accepted=NULL;R(tcp_close(listener)==ERR_OK);
}
static void reuse_receive(void*arg,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*source,u16_t port){(void)arg;(void)pcb;R(IP_IS_V4(source)&&port==50001&&p->tot_len==4);unsigned char b[4];R(pbuf_copy_partial(p,b,4,0)==4&&!memcmp(b,"copy",4));pbuf_free(p);reused++;}
static void udp_reuse(void*arg)
{
  (void)arg;struct udp_pcb*a=udp_new(),*b=udp_new();R(a&&b);ip_set_option(a,SOF_REUSEADDR);ip_set_option(b,SOF_REUSEADDR);R(udp_bind(a,IP_ADDR_ANY,51000)==ERR_OK&&udp_bind(b,IP_ADDR_ANY,51000)==ERR_OK);udp_recv(a,reuse_receive,NULL);udp_recv(b,reuse_receive,NULL);
  unsigned char packet[32]={0};packet[0]=0x45;put16(packet+2,32);packet[8]=64;packet[9]=17;packet[12]=packet[16]=10;packet[13]=packet[17]=44;packet[15]=2;packet[19]=255;put16(packet+10,~sum(packet,20,0));put16(packet+20,50001);put16(packet+22,51000);put16(packet+24,12);memcpy(packet+28,"copy",4);put16(packet+26,~sum(packet+20,12,sum(packet+12,8,17+12)));inject(packet,sizeof(packet));R(reused==2);udp_remove(a);udp_remove(b);
}
static socklen_t loop_address(struct sockaddr_storage*s,int v6,u16_t port){memset(s,0,sizeof(*s));if(v6){struct sockaddr_in6*a=(void*)s;a->sin6_len=sizeof(*a);a->sin6_family=AF_INET6;a->sin6_port=htons(port);a->sin6_addr.s6_addr[15]=1;return sizeof(*a);}struct sockaddr_in*a=(void*)s;a->sin_len=sizeof(*a);a->sin_family=AF_INET;a->sin_port=htons(port);a->sin_addr.s_addr=htonl(0x7f000001);return sizeof(*a);}
static void udp_info(void)
{
  struct sockaddr_storage address;socklen_t length=loop_address(&address,0,52000);int fd=lwip_socket(AF_INET,SOCK_DGRAM,0);R(fd>=0&&lwip_bind(fd,(void*)&address,length)==0);struct timeval bound={5,0};R(lwip_setsockopt(fd,SOL_SOCKET,SO_RCVTIMEO,&bound,sizeof(bound))==0);int yes=1;R(lwip_setsockopt(fd,IPPROTO_IP,IP_PKTINFO,&yes,sizeof(yes))==0);
  R(lwip_sendto(fd,"first",5,0,(void*)&address,length)==5&&lwip_sendto(fd,"second-longer",13,0,(void*)&address,length)==13);R(lwip_host_barrier()==0);
  for(unsigned i=0;i<2;i++){int available=0;R(lwip_ioctl(fd,FIONREAD,&available)==0&&available==(i?13:5));char bytes[32];union{struct cmsghdr align;unsigned char bytes[128];}control;memset(&control,0,sizeof(control));struct iovec vector={bytes,sizeof(bytes)};struct msghdr message={0};message.msg_iov=&vector;message.msg_iovlen=1;message.msg_control=control.bytes;message.msg_controllen=sizeof(control);R(lwip_recvmsg(fd,&message,0)==available);R(!memcmp(bytes,i?"second-longer":"first",available));struct cmsghdr*c=CMSG_FIRSTHDR(&message);R(c&&c->cmsg_level==IPPROTO_IP&&c->cmsg_type==IP_PKTINFO&&c->cmsg_len==CMSG_LEN(sizeof(struct in_pktinfo)));struct in_pktinfo*info=(void*)CMSG_DATA(c);R(info->ipi_ifindex>0&&info->ipi_addr.s_addr==htonl(0x7f000001));}
  R(lwip_close(fd)==0);
  struct netconn*connection=netconn_new(NETCONN_UDP_IPV6);R(connection);netconn_set_recvtimeout(connection,5000);connection->flags|=NETCONN_FLAG_PKTINFO;ip_addr_t loop;R(ipaddr_aton("::1",&loop)&&netconn_bind(connection,&loop,52001)==ERR_OK);struct netbuf*send=netbuf_new();R(send);void*p=netbuf_alloc(send,4);R(p);memcpy(p,"info",4);R(netconn_sendto(connection,send,&loop,52001)==ERR_OK);netbuf_delete(send);struct netbuf*result=NULL;R(netconn_recv(connection,&result)==ERR_OK&&result&&(result->flags&NETBUF_FLAG_DESTADDR));R(ip_addr_eq(&result->toaddr,&loop)&&result->toport_chksum==htons(52001));netbuf_delete(result);R(netconn_delete(connection)==ERR_OK);
}
static void*connect_peer(void*arg){(void)arg;lwip_socket_thread_init();struct sockaddr_in address={0};address.sin_len=sizeof(address);address.sin_family=AF_INET;address.sin_port=htons(50001);address.sin_addr.s_addr=htonl(0x0a2c0002);connect_result=lwip_connect(client_fd,(void*)&address,sizeof(address));atomic_store(&connected,1);lwip_socket_thread_cleanup();return NULL;}
static void clear_frames(void*arg){(void)arg;frame_count=0;}
static void handshake(void*arg)
{(void)arg;R(frame_count>0);const unsigned char*syn=frames[0],*ts=option(syn,sizes[0],8);R(ts&&(syn[33]&2));unsigned char response[128];unsigned n=packet(response,6,(u16_t)word(syn+20),0x20406080,dword(syn+24)+1,18,500,dword(ts+2),NULL,0,1);inject(response,n);}
static void require_unacked(void*arg){(void)arg;struct lwip_sock*socket=lwip_socket_dbg_get_socket(client_fd);R(socket&&socket->conn->pcb.tcp->unacked);}
static void require_reset(void*arg){(void)arg;int found=0;for(unsigned i=0;i<frame_count;i++)if(frames[i][9]==6&&(frames[i][33]&4))found=1;R(found);}
static void linger_case(void)
{
  client_fd=lwip_socket(AF_INET,SOCK_STREAM,0);R(client_fd>=0);struct linger linger={1,0},got={0};socklen_t size=sizeof(got);R(lwip_setsockopt(client_fd,SOL_SOCKET,SO_LINGER,&linger,sizeof(linger))==0&&lwip_getsockopt(client_fd,SOL_SOCKET,SO_LINGER,&got,&size)==0&&got.l_onoff&&got.l_linger==0);R(lwip_host_call(clear_frames,NULL)==0);
  pthread_t thread;R(pthread_create(&thread,NULL,connect_peer,NULL)==0);struct timespec pause={0,1000000};unsigned attempts=0;while(1){R(lwip_host_barrier()==0);if(frame_count)break;R(attempts++<5000);nanosleep(&pause,NULL);}R(lwip_host_call(handshake,NULL)==0);pthread_join(thread,NULL);R(atomic_load(&connected)&&connect_result==0);
  R(lwip_send(client_fd,"unacknowledged",14,0)==14);R(lwip_host_call(require_unacked,NULL)==0);R(lwip_close(client_fd)==0);R(lwip_host_call(require_reset,NULL)==0);
}
static void timeout(void*arg){LWIP_ASSERT_CORE_LOCKED();if(arg)timed++;else canceled++;}
static void advance_timers(void*arg){(void)arg;atomic_store(&fixture_clock,1100);sys_check_timeouts();}
static void finish(void*arg){(void)arg;netif_set_down(&iface);netif_remove(&iface);R(removed==1);R(lwip_stats.mem.used==memory_before);for(unsigned i=0;i<6;i++)R(lwip_stats.memp[pool_kinds[i]]->used==pool_before[i]);}
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
int main(int argc,char**argv)
{
  if(argc>1&&!strcmp(argv[1],"--assert-control"))R(0);R(argc==2);wire=fopen(argv[1],"w");R(wire);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){}lwip_socket_thread_init();
  R(lwip_host_call(setup,NULL)==0&&lwip_host_call(tcp_options,NULL)==0&&lwip_host_call(udp_reuse,NULL)==0);udp_info();linger_case();
  R(tcpip_timeout(0,timeout,(void*)1)==ERR_OK&&tcpip_timeout(20,timeout,NULL)==ERR_OK&&tcpip_untimeout(timeout,NULL)==ERR_OK);R(lwip_host_barrier()==0);R(lwip_host_call(advance_timers,NULL)==0);R(lwip_host_barrier()==0);R(timed==1&&canceled==0);R(lwip_host_call(finish,NULL)==0);lwip_socket_thread_cleanup();R(lwip_host_stop_worker()==0);native_join_workers();fclose(wire);
  printf("{\"status\":\"passed\",\"options\":10,\"native_tcp_window\":%u,\"native_rcv_scale\":%u,\"pktinfo_size\":%zu,\"cmsg_space\":%zu,\"cmsg_data_offset\":%zu,\"cmsg_length\":%zu,\"cmsghdr_size\":%zu,\"netbuf_size\":%zu,\"netbuf_toaddr_offset\":%zu,\"sack_range_bytes\":4,\"reused_receivers\":2,\"timeout_callbacks\":1,\"canceled_callbacks\":0,\"remove_callbacks\":1,\"natural_baseline\":true,\"baseline_resources\":[\"heap\",\"pbuf_pool\",\"tcp_pcb\",\"udp_pcb\",\"netconn\",\"netbuf\",\"tcp_pcb_listen\"]}\n",TCP_WND,TCP_RCV_SCALE,sizeof(struct in_pktinfo),(size_t)CMSG_SPACE(sizeof(struct in_pktinfo)),(size_t)ALIGN_D(sizeof(struct cmsghdr)),(size_t)CMSG_LEN(sizeof(struct in_pktinfo)),sizeof(struct cmsghdr),sizeof(struct netbuf),offsetof(struct netbuf,toaddr));return 0;
}
