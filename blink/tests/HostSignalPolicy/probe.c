#ifdef BLINK_SIGNAL_POLICY
#include "host-signal-policy.h"
#include "host-identity.h"
#else
#define _POSIX_C_SOURCE 200809L
#include <signal.h>
#include <sys/time.h>
#include <unistd.h>
#endif
#include <errno.h>
#include <limits.h>
#include <stddef.h>
#include <stdio.h>
#define CHECK(x) do { if(!(x)) { printf("signal policy line %d errno %d\n",__LINE__,errno);return __LINE__; } } while(0)
static int Zero(const struct itimerval *v) {
 return !v->it_interval.tv_sec && !v->it_interval.tv_usec && !v->it_value.tv_sec && !v->it_value.tv_usec;
}
int SignalPolicyCommon(void) {
 CHECK(sizeof(struct itimerval)==32 && _Alignof(struct itimerval)==8 && offsetof(struct itimerval,it_value)==16);
 CHECK(kill(getpid(),0)==0);
 CHECK(kill(getpid(),65)==-1 && errno==EINVAL);
 for(int which=0;which<3;++which) {
  struct itimerval v={0},old={0};
  CHECK(!setitimer(which,&v,&old));
  CHECK(!getitimer(which,&v) && Zero(&v));
#ifdef BLINK_SIGNAL_POLICY
  /* The private header permits aliasing; glibc's restrict declaration does not. */
  CHECK(!setitimer(which,&v,&v) && Zero(&v));
#endif
  v.it_value.tv_usec=1000000;
  CHECK(setitimer(which,&v,&old)==-1 && errno==EINVAL);
 }
 struct itimerval v={0};
 CHECK(getitimer(99,&v)==-1 && errno==EINVAL);
 CHECK(alarm(0)==0);
 return 0;
}
#ifdef BLINK_SIGNAL_POLICY
void SignalPolicyGc(void);
static _Thread_local struct itimerval saved;
static _Thread_local struct itimerval *cached;
int SignalPolicyUnbound(void) {
 struct itimerval v={{7,8},{9,10}};
 CHECK(getitimer(0,&v)==-1 && errno==ENODEV && v.it_value.tv_sec==9);
 CHECK(kill(0,0)==-1 && errno==ENODEV);
 CHECK(alarm(0)==UINT_MAX && errno==ENODEV);
 CHECK(pause()==-1 && errno==ENODEV);
 return 0;
}
int SignalPolicyPrivate(int pid) {
 CHECK(getpid()==pid && !kill(0,0) && !kill(-pid,0));
 CHECK(kill(pid+1,0)==-1 && errno==ESRCH);
 CHECK(kill(pid,1)==-1 && errno==ENOTSUP);
 CHECK(kill(pid,-1)==-1 && errno==EINVAL);
 CHECK(kill(-1,0)==-1 && errno==ESRCH);
 saved.it_value.tv_sec=pid;cached=&saved;
 SignalPolicyGc();
 CHECK(cached->it_value.tv_sec==pid);
 int (*query)(int,struct itimerval*)=getitimer;
 CHECK(!query(0,cached) && Zero(cached));
 for(int which=0;which<3;++which) {
  struct itimerval value={{0,0},{1,0}},output={{3,4},{5,6}};
  CHECK(setitimer(which,&value,&output)==-1 && errno==ENOTSUP && output.it_value.tv_sec==5);
  value.it_value.tv_sec=0;value.it_interval.tv_usec=1;
  CHECK(setitimer(which,&value,&output)==-1 && errno==ENOTSUP && output.it_interval.tv_usec==4);
  value.it_interval.tv_usec=-1;
  CHECK(setitimer(which,&value,&output)==-1 && errno==EINVAL && output.it_value.tv_usec==6);
  CHECK(getitimer(which,0)==-1 && errno==EFAULT);
  CHECK(setitimer(which,0,&output)==-1 && errno==EFAULT && output.it_value.tv_sec==5);
 }
 CHECK(alarm(1)==UINT_MAX && errno==ENOTSUP);
 CHECK(pause()==-1 && errno==ENOTSUP);
 sigset_t mask={0};
 CHECK(sigsuspend(&mask)==-1 && errno==ENOTSUP);
 CHECK(sigsuspend(0)==-1 && errno==EFAULT);
 CHECK(!getitimer(0,cached) && Zero(cached));
 return 0;
}
#else
int main(void) {
 if(SignalPolicyCommon())return 1;
 puts("signal policy common invariants: PASS");return 0;
}
#endif
