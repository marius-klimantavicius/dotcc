#define _GNU_SOURCE 1
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <unistd.h>
#ifdef BLINK_MANAGED_QUERIES
#include "host-io.h"
#include "host-errors.h"
#endif
#define CHECK(x) do{if(!(x)){printf("socket query failure %d\n",__LINE__);return -1;}}while(0)
int SocketQueriesPrepare(int setting) {
  int fd=socket(AF_INET,SOCK_STREAM,0);CHECK(fd>=0);
  CHECK(!setsockopt(fd,SOL_SOCKET,SO_REUSEADDR,&setting,sizeof(setting)));
  CHECK(!setsockopt(fd,IPPROTO_TCP,TCP_NODELAY,&setting,sizeof(setting)));return fd;
}
int SocketQueriesObserve(int fd,int setting) {
  int value=-1;socklen_t length=sizeof(value);
  CHECK(!getsockopt(fd,SOL_SOCKET,SO_REUSEADDR,&value,&length) && length==4 && value==setting);
  value=-1;length=4;CHECK(!getsockopt(fd,IPPROTO_TCP,TCP_NODELAY,&value,&length) && length==4 && value==setting);return 0;
}
int SocketQueriesProbe(void) {
  int fd=SocketQueriesPrepare(1);CHECK(fd>=0 && !SocketQueriesObserve(fd,1));
  int value=-1;socklen_t length=4;errno=123;
  int (*query)(int,int,int,void *,socklen_t *)=getsockopt;
  CHECK(!query(fd,SOL_SOCKET,SO_TYPE,&value,&length) && value==SOCK_STREAM && length==4 && errno==123);
  value=4096;CHECK(!setsockopt(fd,SOL_SOCKET,SO_SNDBUF,&value,4));value=0;length=4;
  CHECK(!getsockopt(fd,SOL_SOCKET,SO_SNDBUF,&value,&length) && value>=4096 && length==4);
  value=4096;CHECK(!setsockopt(fd,SOL_SOCKET,SO_RCVBUF,&value,4));value=0;length=4;
  CHECK(!getsockopt(fd,SOL_SOCKET,SO_RCVBUF,&value,&length) && value>=4096 && length==4);
  unsigned char small[4]={0x55,0x55,0x55,0x55};length=1;
  CHECK(!getsockopt(fd,SOL_SOCKET,SO_TYPE,small,&length) && length==1 && small[0]==1 && small[1]==0x55);
  length=0;CHECK(!getsockopt(fd,SOL_SOCKET,SO_TYPE,0,&length) && length==0);
  length=4;value=71;CHECK(getsockopt(fd,12345,678,&value,&length)==-1 && errno==EOPNOTSUPP && value==71 && length==4);
  CHECK(getsockopt(fd,SOL_SOCKET,678,&value,&length)==-1 && errno==ENOPROTOOPT && value==71 && length==4);
  CHECK(!close(fd));value=71;length=4;
  CHECK(getsockopt(fd,SOL_SOCKET,SO_TYPE,&value,&length)==-1 && errno==EBADF && value==71 && length==4);
  puts("socket queries: real type/options, returned lengths, truncated output, errno: PASS");return 0;
}
#ifdef BLINK_MANAGED_QUERIES
int SocketQueriesPrivate(void) {
  int value=71;socklen_t length=4;
  CHECK(getsockopt(1,SOL_SOCKET,SO_TYPE,&value,&length)==-1 && errno==ENOTSOCK && value==71 && length==4);
  int fd=socket(AF_INET,SOCK_STREAM,0);CHECK(fd>=0);
  CHECK(getsockopt(fd,SOL_SOCKET,SO_TYPE,0,&length)==-1 && errno==EFAULT && length==4);
  CHECK(getsockopt(fd,SOL_SOCKET,SO_TYPE,&value,0)==-1 && errno==EFAULT && value==71);
  CHECK(!close(fd));return 0;
}
int SocketQueriesUnbound(void) {int value;socklen_t length=4;CHECK(getsockopt(0,SOL_SOCKET,SO_TYPE,&value,&length)==-1 && errno==ENODEV);return 0;}
#else
int main(void){return SocketQueriesProbe();}
#endif
