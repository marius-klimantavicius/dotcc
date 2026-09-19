#define _POSIX_C_SOURCE 200809L
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <sys/socket.h>
#include <sys/uio.h>
#include <netinet/in.h>
#ifdef BLINK_MANAGED_MESSAGES
#include "host-io.h"
#include "host-messages.h"
void ForceGc(void);
#else
static void ForceGc(void) {}
#endif
static void Address(struct sockaddr_in *address,unsigned short port) {
  unsigned char *p=(unsigned char *)address;memset(address,0,sizeof(*address));
  p[0]=2;p[2]=(unsigned char)(port>>8);p[3]=(unsigned char)port;p[4]=127;p[7]=1;
}
int Prepare(unsigned short port) {
  int fd=socket(AF_INET,SOCK_STREAM,0);if(fd<0)return -1;
  struct sockaddr_in address;Address(&address,port);
  if(bind(fd,(struct sockaddr *)&address,sizeof(address)) || listen(fd,4)){close(fd);return -1;}
  return fd;
}
int Errors(void) {
  struct sockaddr address;socklen_t length=sizeof(address);
  errno=0;if(getpeername(-1,&address,&length)!=-1 || errno!=EBADF)return 1;
  int fd=socket(AF_INET,SOCK_STREAM,0);if(fd<0)return 2;
  errno=0;if(getpeername(fd,&address,&length)!=-1 || errno!=ENOTCONN)return 3;
  close(fd);return 0;
}
static int Peer(int fd,const struct sockaddr_in *expected) {
  struct sockaddr_in actual;memset(&actual,0xa5,sizeof(actual));
  socklen_t size=sizeof(actual);if(getpeername(fd,(struct sockaddr *)&actual,&size) || size!=16)return 1;
  if(memcmp(&actual,expected,8))return 2;
  unsigned char small[8];memset(small,0x5a,sizeof(small));size=3;
  if(getpeername(fd,(struct sockaddr *)small,&size) || size!=16 || memcmp(small,expected,3) || small[3]!=0x5a)return 3;
  size=0;if(getpeername(fd,0,&size) || size!=16)return 4;
  return 0;
}
#ifdef BLINK_MANAGED_MESSAGES
int Policies(int fd) {
  char b=0x5a;struct iovec vector={&b,1};struct msghdr m;memset(&m,0,sizeof(m));m.msg_iov=&vector;m.msg_iovlen=1;
  m.msg_control=(void *)1;m.msg_controllen=1;errno=0;
  if(sendmsg(fd,&m,0)!=-1 || errno!=EOPNOTSUPP)return 1;
  if(recvmsg(fd,&m,0)!=-1 || errno!=EOPNOTSUPP || m.msg_controllen!=1 || b!=0x5a)return 2;
  m.msg_control=0;m.msg_controllen=0;
  if(sendmsg(fd,&m,1)!=-1 || errno!=EOPNOTSUPP)return 3;
  m.msg_name=(void *)1;m.msg_namelen=1;
  if(sendmsg(fd,&m,0)!=-1 || errno!=EISCONN)return 4;
  m.msg_name=0;m.msg_namelen=0;m.msg_iovlen=1025;m.msg_iov=(struct iovec *)1;
  if(sendmsg(fd,&m,0)!=-1 || errno!=EMSGSIZE)return 5;
  m.msg_iovlen=1;m.msg_iov=0;if(sendmsg(fd,&m,0)!=-1 || errno!=EFAULT)return 6;
  m.msg_iov=&vector;vector.iov_base=0;if(sendmsg(fd,&m,0)!=-1 || errno!=EFAULT)return 7;
  vector.iov_base=&b;vector.iov_len=(size_t)-1;
  if(sendmsg(fd,&m,0)!=-1 || errno!=EINVAL)return 8;
  vector.iov_len=8;vector.iov_base=(void *)(uintptr_t)-4;
  if(sendmsg(fd,&m,0)!=-1 || errno!=EFAULT)return 9;
  if(sendmsg(1,&m,0)!=-1 || errno!=ENOTSOCK)return 10;
  return 0;
}
#endif
int Exchange(unsigned short port) {
  int listener=Prepare(port);if(listener<0)return 1;
  struct sockaddr_in local,clientlocal;socklen_t len=sizeof(local);
  if(getsockname(listener,(struct sockaddr *)&local,&len))return 2;
  int client=socket(AF_INET,SOCK_STREAM,0);if(client<0 || connect(client,(struct sockaddr *)&local,len))return 3;
  len=sizeof(clientlocal);if(getsockname(client,(struct sockaddr *)&clientlocal,&len))return 4;
  int server=accept(listener,0,0);if(server<0)return 5;
  if(Peer(client,&local) || Peer(server,&clientlocal))return 6;
  int duplicate=dup(client);if(duplicate<0 || close(client) || Peer(duplicate,&local))return 7;client=duplicate;
#ifdef BLINK_MANAGED_MESSAGES
  if(Policies(client))return 8;
#endif
  struct msghdr m;memset(&m,0,sizeof(m));
  if(sendmsg(client,&m,0)!=0)return 9;
  char text[]="abcdefghij";struct iovec sendvectors[3]={{text,3},{0,0},{text+3,7}};
  m.msg_iov=sendvectors;m.msg_iovlen=3;m.msg_flags=123;
  if(sendmsg(client,&m,0)!=10)return 10;
  struct msghdr empty;memset(&empty,0,sizeof(empty));
  if(recvmsg(server,&empty,0)!=0)return 11;
  char request[12];memset(request,0x5a,sizeof(request));
  unsigned char name[16];memset(name,0xa5,sizeof(name));
  int received=0;
  while(received<10){
    struct iovec v[3]={{request+1+received,1},{0,0},{request+2+received,(size_t)(9-received)}};
    memset(&m,0,sizeof(m));m.msg_name=name;m.msg_namelen=16;m.msg_iov=v;m.msg_iovlen=3;m.msg_flags=123;
    ssize_t n=recvmsg(server,&m,0);if(n<=0 || n>10-received || m.msg_namelen || m.msg_controllen || m.msg_flags)return 12;
    received+=(int)n;
  }
  if(memcmp(request+1,text,10) || request[0]!=0x5a || request[11]!=0x5a)return 13;
  for(int i=0;i<16;++i)if(name[i]!=0xa5)return 14;
  ForceGc();
  char *payload=malloc(131072);char *answer=malloc(131074);if(!payload || !answer)return 15;
  for(int i=0;i<131072;++i)payload[i]=(char)(i*13+7);
  memset(answer,0x5a,131074);
  int sent=0;
  while(sent<131072){
    int part=(131072-sent)/3;
    struct iovec v[3]={{payload+sent,(size_t)part},{payload+sent+part,(size_t)part},{payload+sent+2*part,(size_t)(131072-sent-2*part)}};
    memset(&m,0,sizeof(m));m.msg_iov=v;m.msg_iovlen=3;
    ssize_t n=sendmsg(server,&m,0);if(n<=0 || n>131072-sent)return 16;
#ifdef BLINK_MANAGED_MESSAGES
    if(n>65536)return 17;
#endif
    sent+=(int)n;
  }
  if(shutdown(server,SHUT_WR))return 18;
  received=0;
  while(received<131072){
    int first=(131072-received)/2;
    struct iovec v[2]={{answer+1+received,(size_t)first},{answer+1+received+first,(size_t)(131072-received-first)}};
    memset(&m,0,sizeof(m));m.msg_iov=v;m.msg_iovlen=2;
    ssize_t n=recvmsg(client,&m,0);if(n<=0 || n>131072-received)return 19;received+=(int)n;
  }
  ForceGc();
  if(memcmp(answer+1,payload,131072) || answer[0]!=0x5a || answer[131073]!=0x5a)return 20;
  char byte;struct iovec tail={&byte,1};memset(&m,0,sizeof(m));m.msg_iov=&tail;m.msg_iovlen=1;
  if(recvmsg(client,&m,0)!=0 || Peer(client,&local))return 21;
  free(payload);free(answer);
  if(close(client) || close(server) || close(listener))return 22;
  return 0;
}
#ifdef BLINK_MANAGED_MESSAGES
int WaitMessage(int fd,int zero) {
  char data[8];struct iovec v={data,zero?0:sizeof(data)};struct msghdr m;memset(&m,0,sizeof(m));m.msg_iov=&v;m.msg_iovlen=1;m.msg_flags=123;m.msg_namelen=7;
  ssize_t result=recvmsg(fd,&m,0);
  return result==-1 && errno==125 && m.msg_flags==123 && m.msg_namelen==7?0:1;
}
#endif
