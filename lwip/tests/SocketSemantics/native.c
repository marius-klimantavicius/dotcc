#include "lwip/sockets.h"
#include "lwip/tcpip.h"
#include "lwip/api.h"
#include "lwip/priv/sockets_priv.h"
#include <errno.h>
#include <pthread.h>
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d: %s errno=%d\n",__LINE__,#x,errno);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
static atomic_int ready;
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static socklen_t address(struct sockaddr_storage *s,int v6,int port){memset(s,0,sizeof(*s));if(v6){struct sockaddr_in6*a=(void*)s;a->sin6_len=sizeof(*a);a->sin6_family=AF_INET6;a->sin6_port=htons(port);a->sin6_addr.s6_addr[15]=1;return sizeof(*a);}struct sockaddr_in*a=(void*)s;a->sin_len=sizeof(*a);a->sin_family=AF_INET;a->sin_port=htons(port);a->sin_addr.s_addr=htonl(0x7f000001);return sizeof(*a);}
static void udp(int v6){
 struct sockaddr_storage addr; socklen_t length=address(&addr,v6,28100+v6);
 int fd=lwip_socket(v6?AF_INET6:AF_INET,SOCK_DGRAM,0);R(fd>=0);R(lwip_bind(fd,(void*)&addr,length)==0);
 int flag=1;R(lwip_ioctl(fd,FIONBIO,&flag)==0);R((lwip_fcntl(fd,F_GETFL,0)&O_NONBLOCK)!=0);
 char data[16]={0};R(lwip_recv(fd,data,sizeof(data),0)==-1&&errno==EWOULDBLOCK);
 R(lwip_fcntl(fd,F_SETFL,0)==0);R((lwip_fcntl(fd,F_GETFL,0)&O_NONBLOCK)==0);
 struct timeval timeout={0,20000};R(lwip_setsockopt(fd,SOL_SOCKET,SO_RCVTIMEO,&timeout,sizeof(timeout))==0);R(lwip_recv(fd,data,sizeof(data),0)==-1&&errno==EWOULDBLOCK);
 flag=1;R(lwip_setsockopt(fd,SOL_SOCKET,SO_BROADCAST,&flag,sizeof(flag))==0);flag=0;socklen_t size=sizeof(flag);R(lwip_getsockopt(fd,SOL_SOCKET,SO_BROADCAST,&flag,&size)==0&&flag!=0);
 fd_set reads;FD_ZERO(&reads);FD_SET(fd,&reads);timeout=(struct timeval){0,0};R(lwip_select(fd+1,&reads,NULL,NULL,&timeout)==0&&!FD_ISSET(fd,&reads));
 struct pollfd poll={fd,POLLIN|POLLOUT,0};R(lwip_poll(&poll,1,0)==1&&poll.revents==POLLOUT);
 memcpy(data,"readiness",9);R(lwip_sendto(fd,data,9,0,(void*)&addr,length)==9);poll.events=POLLIN;R(lwip_poll(&poll,1,2000)==1&&(poll.revents&POLLIN));
 int available=0;R(lwip_ioctl(fd,FIONREAD,&available)==0&&available==9);FD_ZERO(&reads);FD_SET(fd,&reads);R(lwip_select(fd+1,&reads,NULL,NULL,&timeout)==1&&FD_ISSET(fd,&reads));
 memset(data,0,sizeof(data));R(lwip_recv(fd,data,3,MSG_PEEK)==3&&memcmp(data,"rea",3)==0);R(lwip_recv(fd,data,sizeof(data),0)==9&&memcmp(data,"readiness",9)==0);
 R(lwip_close(fd)==0);poll.events=POLLIN;poll.revents=0;R(lwip_poll(&poll,1,0)==1&&poll.revents==POLLIN);poll.fd=9999;R(lwip_poll(&poll,1,0)==-1&&(poll.revents&POLLNVAL));R(lwip_recv(fd,data,1,0)==-1&&errno==EBADF);
 printf("{\"case\":\"socket-udp-ipv%d\",\"nonblocking_errno\":%d,\"closed_errno\":%d}\n",v6?6:4,EWOULDBLOCK,EBADF);
}
static atomic_int waiting;
static int blocked_result,blocked_errno;
static void*blocked(void*arg){int fd=*(int*)arg;lwip_socket_thread_init();atomic_store(&waiting,1);char byte;blocked_result=lwip_recv(fd,&byte,1,0);blocked_errno=errno;lwip_socket_thread_cleanup();return NULL;}
static void tcp(int v6){
 struct sockaddr_storage addr;socklen_t length=address(&addr,v6,28200+v6);int family=v6?AF_INET6:AF_INET;
 int listener=lwip_socket(family,SOCK_STREAM,0);R(listener>=0);R(lwip_bind(listener,(void*)&addr,length)==0);R(lwip_listen(listener,2)==0);
 int client=lwip_socket(family,SOCK_STREAM,0);R(client>=0);R(lwip_connect(client,(void*)&addr,length)==0);int peer=lwip_accept(listener,NULL,NULL);R(peer>=0);
 int flag=1;R(lwip_setsockopt(client,IPPROTO_TCP,TCP_NODELAY,&flag,sizeof(flag))==0);flag=0;socklen_t size=sizeof(flag);R(lwip_getsockopt(client,IPPROTO_TCP,TCP_NODELAY,&flag,&size)==0&&flag==1);
 struct sockaddr_storage name;socklen_t namesize=sizeof(name);R(lwip_getpeername(client,(void*)&name,&namesize)==0&&namesize==length);R(memcmp(&name,&addr,length)==0);
 char data[32];R(lwip_send(client,"partial",7,0)==7);R(lwip_shutdown(client,SHUT_WR)==0);R(lwip_recv(peer,data,2,MSG_WAITALL)==2&&memcmp(data,"pa",2)==0);R(lwip_recv(peer,data,5,MSG_WAITALL)==5&&memcmp(data,"rtial",5)==0);R(lwip_recv(peer,data,1,0)==0);R(lwip_send(peer,"ack",3,0)==3);R(lwip_recv(client,data,3,MSG_WAITALL)==3&&memcmp(data,"ack",3)==0);
 R(lwip_close(peer)==0);R(lwip_recv(client,data,1,0)==0);R(lwip_close(client)==0);R(lwip_close(listener)==0);
 printf("{\"case\":\"socket-tcp-ipv%d\",\"half_close\":true,\"partial_recv\":true}\n",v6?6:4);
}
static void close_waiter(void){struct sockaddr_storage addr;socklen_t length=address(&addr,0,28300);int fd=lwip_socket(AF_INET,SOCK_DGRAM,0);R(fd>=0);R(lwip_bind(fd,(void*)&addr,length)==0);pthread_t thread;R(pthread_create(&thread,NULL,blocked,&fd)==0);while(!atomic_load(&waiting)){struct timespec t={0,1000000};nanosleep(&t,NULL);}for(;;){sys_prot_t token=sys_arch_protect();int blocked=lwip_socket_dbg_get_socket(fd)->conn->mbox_threads_waiting;sys_arch_unprotect(token);if(blocked)break;struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_close(fd)==0);pthread_join(thread,NULL);R(blocked_result==-1);printf("{\"case\":\"socket-concurrent-close\",\"result\":%d,\"errno\":%d}\n",blocked_result,blocked_errno);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}lwip_socket_thread_init();udp(0);udp(1);tcp(0);tcp(1);close_waiter();lwip_socket_thread_cleanup();R(lwip_host_stop_worker()==0);native_join_workers();return 0;}
