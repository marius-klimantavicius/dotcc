#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
static void run(const char *node,const char *service,const struct addrinfo *hints) {
    struct addrinfo *r=NULL; errno=123; int e=getaddrinfo(node,service,hints,&r);
    printf("PREVIOUS %s %s hints=%s flags=%d family=%d type=%d proto=%d error=%d errno=%d",node,service,hints?"explicit":"NULL",hints?hints->ai_flags:0,hints?hints->ai_family:0,hints?hints->ai_socktype:0,hints?hints->ai_protocol:0,e,errno);
    for(struct addrinfo *p=r;p;p=p->ai_next) {
        printf(" node=%d,%d,%d,%d,%u,%s,",p->ai_flags,p->ai_family,p->ai_socktype,p->ai_protocol,(unsigned)p->ai_addrlen,p->ai_canonname?p->ai_canonname:"-");
        for(unsigned i=0;i<p->ai_addrlen;i++) printf("%02x",((unsigned char*)p->ai_addr)[i]);
    }
    putchar('\n'); freeaddrinfo(r);
}
int main(void) {
    struct addrinfo hints; memset(&hints,0,sizeof hints); hints.ai_flags=AI_NUMERICHOST;
    run("localhost","445",NULL);
    run("127.0.0.1","65536",&hints);
    run("127.0.0.1","invalid!service",&hints);
    return 0;
}
