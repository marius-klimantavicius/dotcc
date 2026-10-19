#define _DEFAULT_SOURCE 1
#include <netdb.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>

static int (*const reverse_fn)(const struct sockaddr *, socklen_t, char *, socklen_t, char *, socklen_t, int) = getnameinfo;
static struct hostent *(*const host_fn)(const char *) = gethostbyname;
static int (*const resolve_fn)(const char *, const char *, const struct addrinfo *, struct addrinfo **) = getaddrinfo;
static void (*const release_fn)(struct addrinfo *) = freeaddrinfo;
static void bytes(const void *p, size_t n) {
    const unsigned char *b = p;
    for (size_t i = 0; i < n; ++i) printf("%02x", b[i]);
}
static void reverse_case(const char *id, struct sockaddr *a, socklen_t al,
                         int flags, unsigned hl, unsigned sl, int omit) {
    char h[128], s[64]; memset(h, 0xa5, sizeof h); memset(s, 0xa5, sizeof s);
    errno = 123;
    int r = reverse_fn(a, al, omit & 1 ? NULL : h + 1, hl,
                        omit & 2 ? NULL : s + 1, sl, flags);
    printf("NI %s %d %d ", id, r, errno); bytes(h, 30); putchar(' '); bytes(s, 30); putchar('\n');
}
int main(void) {
    printf("LAYOUT %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu %zu\n",
      sizeof(socklen_t), sizeof(struct addrinfo), _Alignof(struct addrinfo),
      offsetof(struct addrinfo, ai_addrlen), offsetof(struct addrinfo, ai_addr),
      offsetof(struct addrinfo, ai_canonname), offsetof(struct addrinfo, ai_next),
      sizeof(struct hostent), _Alignof(struct hostent), offsetof(struct hostent, h_name),
      offsetof(struct hostent, h_aliases), offsetof(struct hostent, h_addrtype),
      offsetof(struct hostent, h_length), offsetof(struct hostent, h_addr_list),
      sizeof(struct sockaddr), sizeof(struct sockaddr_in), sizeof(struct sockaddr_in6),
      sizeof(struct sockaddr_storage), sizeof(((struct addrinfo *)0)->ai_addrlen));
    printf("CONSTANTS %d %d %d %d %d %d %d %d %d %d %d\n", NI_NUMERICHOST, NI_NUMERICSERV, NI_NOFQDN,
      NI_NAMEREQD, NI_DGRAM, NI_MAXHOST, NI_MAXSERV, HOST_NOT_FOUND, TRY_AGAIN, NO_RECOVERY, NO_DATA);
    struct sockaddr_in a; memset(&a, 0, sizeof a); a.sin_family = AF_INET;
    inet_pton(AF_INET, "127.0.0.1", &a.sin_addr); a.sin_port = htons(80);
    int fs[] = {0,1,2,3,4,8,9,16,19,32,64,128,256,512,1024,-1};
    for (unsigned i=0; i<sizeof fs/sizeof *fs; ++i) {
        char id[32]; snprintf(id,sizeof id,"flags-%d",fs[i]);
        reverse_case(id,(struct sockaddr *)&a,sizeof a,fs[i],120,60,0);
    }
    unsigned ns[] = {0,1,2,3,8,9,10,11,120};
    for(unsigned i=0;i<sizeof ns/sizeof *ns;++i) {
        char id[32]; snprintf(id,sizeof id,"host-%u",ns[i]); reverse_case(id,(struct sockaddr *)&a,sizeof a,3,ns[i],60,0);
        snprintf(id,sizeof id,"serv-%u",ns[i]); reverse_case(id,(struct sockaddr *)&a,sizeof a,3,120,ns[i],0);
    }
    for(int i=0;i<4;++i) reverse_case(i==0?"outputs":i==1?"omit-host":i==2?"omit-serv":"omit-both",(struct sockaddr *)&a,sizeof a,3,120,60,i);
    reverse_case("zero-both",(struct sockaddr *)&a,sizeof a,3,0,0,0);
    reverse_case("zero-namereqd",(struct sockaddr *)&a,sizeof a,8,0,0,0);
    reverse_case("short",(struct sockaddr *)&a,15,3,120,60,0);
    reverse_case("long",(struct sockaddr *)&a,128,3,120,60,0);
    a.sin_family=99; reverse_case("family",(struct sockaddr *)&a,sizeof a,3,120,60,0); a.sin_family=AF_INET;
    inet_pton(AF_INET,"192.0.2.254",&a.sin_addr);
    reverse_case("no-ptr-required",(struct sockaddr *)&a,sizeof a,8,120,60,0);
    reverse_case("no-ptr-fallback",(struct sockaddr *)&a,sizeof a,0,120,60,0);
    int ports[]={0,53,80,443,512,65535};
    for(unsigned i=0;i<sizeof ports/sizeof *ports;++i) for(int udp=0;udp<2;++udp) {
        char id[32]; a.sin_port=htons(ports[i]); snprintf(id,sizeof id,"port-%d-%d",ports[i],udp);
        reverse_case(id,(struct sockaddr *)&a,sizeof a,1|(udp?16:0),120,60,0);
    }
    struct sockaddr_in6 a6; memset(&a6,0,sizeof a6); a6.sin6_family=AF_INET6; a6.sin6_port=htons(443);
    inet_pton(AF_INET6,"fe80::abcd",&a6.sin6_addr); a6.sin6_scope_id=12345; a6.sin6_flowinfo=0x12345678;
    reverse_case("v6-scope",(struct sockaddr *)&a6,sizeof a6,3,120,60,0);
    inet_pton(AF_INET6,"::1",&a6.sin6_addr); a6.sin6_scope_id=0;
    reverse_case("v6-loop",(struct sockaddr *)&a6,sizeof a6,3,120,60,0);
    const char *names[]={"127.0.0.1","127.1","localhost","marius-devtop.localdomain","ip6-localhost","invalid!host"};
    for(unsigned i=0;i<sizeof names/sizeof *names;++i) {
        errno=123; h_errno=99; struct hostent *h=host_fn(names[i]);
        printf("HOST %s %d %d %d",names[i],h!=NULL,h_errno,errno);
        if(h) {
            printf(" %s %d %d",h->h_name,h->h_addrtype,h->h_length);
            for(char **p=h->h_aliases;*p;++p) printf(" alias=%s",*p);
            for(char **p=h->h_addr_list;*p;++p) { printf(" addr="); bytes(*p,h->h_length); }
        } putchar('\n');
    }
    struct addrinfo hi; memset(&hi,0,sizeof hi); hi.ai_flags=AI_NUMERICHOST|AI_NUMERICSERV|AI_CANONNAME; hi.ai_socktype=SOCK_STREAM;
    struct addrinfo *r=NULL; errno=123; int e=resolve_fn("127.0.0.1","443",&hi,&r);
    printf("AI %d %d",e,errno);
    for(struct addrinfo *p=r;p;p=p->ai_next) { printf(" node=%d,%d,%d,%u,%s,",p->ai_family,p->ai_socktype,p->ai_protocol,(unsigned)p->ai_addrlen,p->ai_canonname?p->ai_canonname:"-"); bytes(p->ai_addr,p->ai_addrlen); }
    putchar('\n'); release_fn(r); release_fn(NULL);
    return 0;
}
