#include <netdb.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <string.h>
#include <stdio.h>
#include <errno.h>
#include <fcntl.h>
#define CHECK(v) do{if(!(v)){printf("FAIL line=%d errno=%d\n",__LINE__,errno);return __LINE__;}}while(0)
static int loopback(int family){
 int server=socket(family,SOCK_STREAM|SOCK_CLOEXEC,IPPROTO_TCP);CHECK(server>=0);
 struct sockaddr_storage address;memset(&address,0,sizeof address);socklen_t length;
 if(family==AF_INET){struct sockaddr_in*a=(struct sockaddr_in*)&address;a->sin_family=AF_INET;a->sin_addr.s_addr=inet_addr("127.0.0.1");a->sin_port=htons(0);length=sizeof *a;}
 else{struct sockaddr_in6*a=(struct sockaddr_in6*)&address;a->sin6_family=AF_INET6;a->sin6_addr=in6addr_loopback;a->sin6_port=htons(0);length=sizeof *a;}
 CHECK(bind(server,(struct sockaddr*)&address,length)==0);CHECK(listen(server,5)==0);
 CHECK(getsockname(server,(struct sockaddr*)&address,&length)==0);
 CHECK(fcntl(server,F_GETFD)==FD_CLOEXEC);
 int client=socket(family,SOCK_STREAM,0);CHECK(client>=0);CHECK(connect(client,(struct sockaddr*)&address,length)==0);
 struct sockaddr_storage peer;socklen_t peer_size=sizeof peer;int accepted=accept(server,(struct sockaddr*)&peer,&peer_size);CHECK(accepted>=0);
 CHECK(peer.ss_family==family&&peer_size==length);CHECK(fcntl(accepted,F_GETFD)==0);
 const char payload[]="translated socket bytes";char buffer[64];
 CHECK(write(client,payload,sizeof payload)==sizeof payload);CHECK(read(accepted,buffer,sizeof buffer)==sizeof payload);CHECK(memcmp(buffer,payload,sizeof payload)==0);
 CHECK(write(accepted,buffer,sizeof payload)==sizeof payload);CHECK(read(client,buffer,sizeof buffer)==sizeof payload);
 CHECK(shutdown(client,SHUT_WR)==0);CHECK(read(accepted,buffer,sizeof buffer)==0);
 CHECK(close(accepted)==0);CHECK(close(client)==0);CHECK(close(server)==0);
 return 0;
}
int main(void){int r=loopback(AF_INET);if(r)return r;r=loopback(AF_INET6);if(r)return r;puts("PASS translated public sockets IPv4+IPv6");return 0;}
