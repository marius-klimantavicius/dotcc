#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
static void probe(int id,const char *name,const char *service,int flags,int family,int type,int protocol) {
    struct addrinfo h,*r=NULL; memset(&h,0,sizeof h); h.ai_flags=flags; h.ai_family=family; h.ai_socktype=type; h.ai_protocol=protocol;
    errno=123; int e=getaddrinfo(name,service,&h,&r);
    printf("FORWARD %d %s %s %d %d %d %d %d %d",id,name?name:"-",service?service:"-",flags,family,type,protocol,e,errno);
    for(struct addrinfo *p=r;p;p=p->ai_next) {
        printf(" node=%d,%d,%d,%d,%u,%s,",p->ai_flags,p->ai_family,p->ai_socktype,p->ai_protocol,(unsigned)p->ai_addrlen,p->ai_canonname?p->ai_canonname:"-");
        for(unsigned i=0;i<p->ai_addrlen;i++) printf("%02x",((unsigned char*)p->ai_addr)[i]);
    }
    putchar('\n'); freeaddrinfo(r);
}
int main(void) {
    int id=0;
    const char *names[]={"127.0.0.1","::1","localhost",NULL};
    const char *services[]={"80","domain","exec","65536","65535","invalid!service",NULL};
    for(unsigned i=0;i<4;i++) for(unsigned j=0;j<7;j++) probe(id++,names[i],services[j],0,0,1,0);
    int flags[]={1,2,4,8,16,24,32,1024,2048,-1};
    for(unsigned i=0;i<10;i++) probe(id++,"127.0.0.1","53",flags[i],0,0,0);
    probe(id++,"invalid!host","80",4,0,1,0);
    probe(id++,"127.0.0.1","domain",1024,0,1,0);
    probe(id++,"127.0.0.1","53",24,10,1,0);
    probe(id++,"127.0.0.1","53",8,10,1,0);
    probe(id++,"127.0.0.1","53",0,99,1,0);
    probe(id++,"127.0.0.1","53",0,0,99,0);
    probe(id++,"127.0.0.1","53",0,0,1,17);
    probe(id++,"127.0.0.1","53",0,0,2,6);
    probe(id++,"127.0.0.1","domain",0,0,2,17);
    freeaddrinfo(NULL); return 0;
}
