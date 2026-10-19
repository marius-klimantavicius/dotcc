#define _GNU_SOURCE 1
#include <netdb.h>
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <stdlib.h>
#include <sys/resource.h>
#include <unistd.h>

static int (*const host_r)(const char *,struct hostent *,char *,size_t,struct hostent **,int *)=gethostbyname_r;
int main(int argc,char **argv) {
    (void)argv;
    printf("HOSTABI %zu %zu %zu %zu %zu %zu %zu %zu\n",sizeof(size_t),sizeof(struct hostent),_Alignof(struct hostent),offsetof(struct hostent,h_name),offsetof(struct hostent,h_aliases),offsetof(struct hostent,h_addrtype),offsetof(struct hostent,h_length),offsetof(struct hostent,h_addr_list));
    if(argc>1) {
        // Limit this disposable native process, exhaust allocatable chunks,
        // and demand a real resolver resource failure. No success substitute.
        FILE *f=fopen("/proc/self/statm","r"); unsigned long pages=0;
        if(!f || fscanf(f,"%lu",&pages)!=1) return 20;
        fclose(f); struct rlimit lim={(rlim_t)pages*(rlim_t)sysconf(_SC_PAGESIZE)+1048576,RLIM_INFINITY};
        if(setrlimit(RLIMIT_AS,&lim)!=0) return 21;
        while(malloc(4096)!=NULL) { }
        while(malloc(16)!=NULL) { }
        struct addrinfo hints,*r=NULL; memset(&hints,0,sizeof hints); hints.ai_flags=AI_NUMERICHOST|AI_NUMERICSERV; hints.ai_socktype=SOCK_STREAM;
        int e=getaddrinfo("127.0.0.1","80",&hints,&r);
        printf("RESOURCE %d %d %d\n",e,errno,r!=NULL);
        if(r) freeaddrinfo(r);
        return e==EAI_MEMORY && r==NULL ? 0:22;
    }
    char buffer[65536]; struct hostent h,*r; int he,e;
    size_t sizes[]={0,1,8,16,32,64,128,1024,65536};
    for(unsigned i=0;i<sizeof sizes/sizeof *sizes;++i) {
        memset(&h,0,sizeof h); memset(buffer,0xa5,sizeof buffer); r=(void*)1; he=99; errno=123;
        e=host_r("ip6-localhost",&h,buffer,sizes[i],&r,&he);
        printf("BRIDGE %zu %d %d %d %d",sizes[i],e,he,errno,r!=NULL);
        if(r) {
            if(r!=&h || h.h_addrtype!=2 || h.h_length!=4) return 30;
            printf(" %s",h.h_name);
            for(char **p=h.h_aliases;*p;++p) printf(" alias=%s",*p);
            for(char **p=h.h_addr_list;*p;++p) printf(" addr=%u.%u.%u.%u",(unsigned char)(*p)[0],(unsigned char)(*p)[1],(unsigned char)(*p)[2],(unsigned char)(*p)[3]);
        } else if(e!=ERANGE) return 31;
        putchar('\n');
    }
    const char *names[]={"127.1","localhost","invalid!host"};
    for(unsigned i=0;i<sizeof names/sizeof *names;++i) {
        he=99; errno=123; r=(void*)1;
        e=host_r(names[i],&h,buffer,sizeof buffer,&r,&he);
        printf("ERROR %s %d %d %d %d\n",names[i],e,he,errno,r!=NULL);
    }
    return 0;
}
