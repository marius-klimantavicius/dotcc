#define _GNU_SOURCE 1
#include <errno.h>
#include <poll.h>
int WaitReadable(int fd,int timeout);
#include <stdint.h>
#include <string.h>
#include <sys/socket.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_NETWORK
#include "host-io.h"
#endif
/* Encode native-checked sockaddr_in bytes without borrowing host BCL structs. */
int Prepare(int port) {
  unsigned char address[16]={2,0};
  address[2]=port>>8; address[3]=port;address[4]=127;address[7]=1;
  int fd=socket(AF_INET,SOCK_STREAM,0),yes=1;
  if(fd<0)return -1;
  if(setsockopt(fd,SOL_SOCKET,SO_REUSEADDR,&yes,sizeof(yes)) || bind(fd,(struct sockaddr *)address,16) || listen(fd,4)) { close(fd);return -1; }
  return fd;
}
int Port(int fd) {
  unsigned char address[16]; socklen_t length=16;
  if(getsockname(fd,(struct sockaddr *)address,&length) || length!=16 || address[0]!=2 || address[1] || address[4]!=127 || address[7]!=1)return -1;
  int port=address[2]*256+address[3];
  memset(address,0xa5,sizeof(address));length=3;
  if(getsockname(fd,(struct sockaddr *)address,&length) || length!=16 || address[0]!=2 || address[1] || address[3]!=0xa5)return -1;
  return port;
}
int SocketErrors(void) {
  if(send(1,"",0,0)!=-1 || errno!=ENOTSOCK)return 1;
  return 0;
}
int WaitAccept(int listener) { return accept(listener,0,0); }
int Serve(int listener) {
  if(WaitReadable(listener,5000)!=1)return 20;
  unsigned char address[16]; socklen_t length=sizeof(address);
  int fd=accept(listener,(struct sockaddr *)address,&length);
  if(fd<0 || length!=16 || address[0]!=2 || address[4]!=127 || address[7]!=1)return 1;
  char request[6];long n=0;
  while(n<6) { long k=recv(fd,request+n,6-n,0); if(k<=0)return 2; n+=k; }
  if(memcmp(request,"hello!",6))return 3;
  unsigned char buffer[4096];
  for(int i=0;i<4096;i++)buffer[i]=(unsigned char)(i*13+7);
  for(int chunk=0;chunk<32;chunk++) {
    n=0;while(n<4096) {long k=send(fd,buffer+n,4096-n,0);if(k<=0)return 4;n+=k;}
  }
  if(shutdown(fd,SHUT_WR) || close(fd) || close(listener))return 5;
  return 0;
}
int Reject(void) {
  int fd=socket(AF_INET,SOCK_STREAM,0);
  if(fd<0)return 1;
  unsigned char address[16]={2,0,0,80,192,0,2,1};
  int rc=connect(fd,(struct sockaddr *)address,16);
  close(fd);
  return rc;
}

int WaitReadable(int fd,int timeout) {
  struct pollfd p={fd,POLLIN,0};
  int result=poll(&p,1,timeout);
  if(result<0)return -1;
  return result==1 && p.revents==POLLIN ? 1 : result==0 && p.revents==0 ? 0 : -2;
}
int ReadinessChecks(int listener) {
  struct pollfd p[3]={{-1,-1,123},{99999,POLLIN,123},{listener,POLLIN,123}};
  if(poll(p,3,0)!=1 || p[0].revents || p[1].revents!=POLLNVAL || p[2].revents)return 1;
  if(WaitReadable(listener,10)!=0)return 2;
  return 0;
}
int WaitInvalid(int listener) {
  struct pollfd p={listener,POLLIN,0};
  return poll(&p,1,-1)==1 && p.revents==POLLNVAL ? 0 : 1;
}
int WaitEmpty(void) { return poll(0,0,-1); }
int Hangup(int listener) {
  int fd=accept(listener,0,0);char byte;
  if(fd<0 || recv(fd,&byte,1,0)!=0)return 1;
  struct pollfd p={fd,POLLIN|POLLOUT,0};
  if(poll(&p,1,0)!=1 || p.revents!=(POLLIN|POLLOUT))return 2;
  if(shutdown(fd,SHUT_WR))return 3;
  p.events=0;p.revents=0;
  if(poll(&p,1,0)!=1 || p.revents!=POLLHUP)return 4;
  if(close(fd) || close(listener))return 5;
  return 0;
}
