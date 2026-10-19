#define _DEFAULT_SOURCE 1
#define _POSIX_C_SOURCE 200809L
#include <signal.h>
#include <errno.h>
#include <dirent.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <stddef.h>
#include <pthread.h>
static volatile sig_atomic_t count, depth, maxdepth;
static void second(int sig) { (void)sig; ++count; ++depth; if (depth>maxdepth) maxdepth=depth; --depth; }
static void first(int sig) {
    ++count; ++depth; if (depth>maxdepth) maxdepth=depth;
    signal(sig,second); raise(sig); --depth;
}
static void handler(int sig) { if(sig==SIGINT) ++count; errno=35; }
int main(int argc,char **argv) {
    if(argc<2) return 90;
    if(!strcmp(argv[1],"host-abi")) {
        printf("host-action size=%zu handler=%zu mask=%zu flags=%zu restorer=%zu\n",sizeof(struct sigaction),offsetof(struct sigaction,sa_handler),offsetof(struct sigaction,sa_mask),offsetof(struct sigaction,sa_flags),offsetof(struct sigaction,sa_restorer)); return 0;
    }
    if(!strcmp(argv[1],"blocked")) {
        struct sigaction before, after, temporary; sigset_t block, previous, observed, pending, maskafter;
        memset(&before,0,sizeof(before)); memset(&after,0,sizeof(after)); memset(&temporary,0,sizeof(temporary));
        signal(SIGINT,handler); sigaction(SIGINT,NULL,&before);
        sigaddset(&before.sa_mask,SIGUSR1); sigaddset(&before.sa_mask,SIGUSR2); before.sa_flags|=SA_RESTART;
        if(sigaction(SIGINT,&before,NULL)) return 92;
        memset(&before,0,sizeof(before)); sigaction(SIGINT,NULL,&before);
        sigemptyset(&previous); sigemptyset(&observed); sigemptyset(&pending); sigemptyset(&maskafter);
        sigemptyset(&block); sigaddset(&block,SIGINT); pthread_sigmask(SIG_BLOCK,&block,&previous); pthread_sigmask(SIG_SETMASK,NULL,&observed);
        if(sigaction(SIGINT,&temporary,NULL)) return 93;
        int status=raise(SIGINT); int restore=sigaction(SIGINT,&before,NULL);
        sigaction(SIGINT,NULL,&after); pthread_sigmask(SIG_SETMASK,NULL,&maskafter); sigpending(&pending);
        /* glibc sigset_t is128 bytes; the kernel returns only _NSIG-1 bits.
           The remaining bytes (and structure padding) are unspecified. */
        int same=before.sa_handler==after.sa_handler && before.sa_flags==after.sa_flags && before.sa_restorer==after.sa_restorer && memcmp(&before.sa_mask,&after.sa_mask,(_NSIG-1)/8)==0;
        int masksame=memcmp(&observed,&maskafter,(_NSIG-1)/8)==0, waspending=sigismember(&pending,SIGINT);
        pthread_sigmask(SIG_SETMASK,&previous,NULL); sigpending(&pending);
        printf("blocked=%d action-same=%d mask-same=%d pending-before=%d delivered=%d pending-after=%d\n",status,same,masksame,waspending,count,sigismember(&pending,SIGINT));
        return restore || !same || !masksame || count!=1;
    }
    if(!strcmp(argv[1],"directory")) {
        errno=71; DIR *d=opendir(argv[2]);
        if(!d) { printf("errno=%d\n",errno); return 0; }
        int dot=0,parent=0,item=0; struct dirent *e;
        while((e=readdir(d))) { if(!strcmp(e->d_name,".")) dot=1; if(!strcmp(e->d_name,"..")) parent=1; if(!strcmp(e->d_name,"item")) item=1; }
        int status=closedir(d); printf("directory=%d,%d,%d close=%d\n",dot,parent,item,status); return 0;
    }
    if(!strcmp(argv[1],"default")) { signal(SIGINT,SIG_DFL); puts("before-default"); fflush(stdout); raise(SIGINT); puts("unexpected-return"); return 91; }
    if(!strcmp(argv[1],"ignore")) { signal(SIGINT,SIG_IGN); errno=71; int status=raise(SIGINT); printf("ignore=%d errno=%d\n",status,errno); return 0; }
    if(!strcmp(argv[1],"error")) { errno=0; void (*p)(int)=signal(-1,handler); printf("signal-error=%d errno=%d\n",p==SIG_ERR,errno); errno=0; int status=raise(-1); printf("raise-error=%d errno=%d\n",status!=0,errno); return 0; }
    if(!strcmp(argv[1],"reentrant")) { signal(SIGINT,first); int status=raise(SIGINT); signal(SIGINT,SIG_DFL); printf("reentrant=%d count=%d depth=%d\n",status,count,maxdepth); return 0; }
    signal(SIGINT,handler); errno=71; int status=raise(SIGINT); printf("sync=%d count=%d errno=%d\n",status,count,errno); signal(SIGINT,SIG_DFL); return count!=1;
}
