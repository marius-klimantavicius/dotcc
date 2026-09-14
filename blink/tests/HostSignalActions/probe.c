#define _GNU_SOURCE 1
#include <signal.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#ifdef BLINK_PRIVATE_ACTIONS
#include "HostSignalActions.h"
#endif
#define CHECK(x) do { if(!(x)){printf("action failure %d\n",__LINE__);return __LINE__;} } while(0)
static _Thread_local int calls;
static void Simple(int sig){calls=sig;}
static void Info(int sig,siginfo_t *info,void *context){calls=sig+(info!=0)+(context==(void *)(uintptr_t)42);}
static uint64_t Mask(const struct sigaction *action){uint64_t mask;memcpy(&mask,&action->sa_mask,8);return mask;}
static int PublicFlags(const struct sigaction *action){return action->sa_flags & ~0x04000000;}
int ActionProbe(void) {
  struct sigaction action={0},old,query;
  CHECK(!sigaction(SIGUSR1,&action,0));
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==SIG_DFL);
  action.sa_handler=Simple;action.sa_flags=SA_RESTART;
  memset(&action.sa_mask,255,sizeof(action.sa_mask));
  CHECK(!sigaction(SIGUSR1,&action,&old) && old.sa_handler==SIG_DFL);
  memset(&action,0,sizeof(action)); // Registry must retain a value copy.
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==Simple && PublicFlags(&query)==SA_RESTART);
  CHECK(Mask(&query)==(UINT64_MAX & ~(UINT64_C(1)<<(SIGKILL-1) | UINT64_C(1)<<(SIGSTOP-1))));
  calls=0;query.sa_handler(17);CHECK(calls==17);
  CHECK(signal(SIGUSR1,SIG_IGN)==Simple);
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==SIG_IGN && PublicFlags(&query)==SA_RESTART && Mask(&query)==(UINT64_C(1)<<(SIGUSR1-1)));
  CHECK(signal(SIGUSR1,SIG_DFL)==SIG_IGN);
  action.sa_sigaction=Info;action.sa_flags=SA_SIGINFO;
  CHECK(!sigaction(SIGUSR1,&action,&old) && old.sa_handler==SIG_DFL);
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_sigaction==Info && PublicFlags(&query)==SA_SIGINFO);
  siginfo_t information;calls=0;query.sa_sigaction(19,&information,(void *)(uintptr_t)42);CHECK(calls==21);
  CHECK(!sigaction(SIGKILL,0,&query));CHECK(!sigaction(SIGSTOP,0,&query));
  CHECK(sigaction(SIGKILL,&action,0)==-1 && errno==EINVAL);
  CHECK(sigaction(SIGSTOP,&action,0)==-1 && errno==EINVAL);
  CHECK(sigaction(0,0,0)==-1 && errno==EINVAL);CHECK(sigaction(65,0,0)==-1 && errno==EINVAL);
  CHECK(sigaction(32,0,0)==-1 && errno==EINVAL);CHECK(sigaction(33,0,0)==-1 && errno==EINVAL);
  CHECK(signal(SIGKILL,SIG_DFL)==SIG_ERR && errno==EINVAL);
  errno=123;CHECK(!sigaction(SIGUSR1,0,&query) && errno==123);
  CHECK(signal(SIGUSR1,SIG_DFL)==(void (*)(int))Info);
  puts("private disposition registration, masks, queries and function pointers: PASS");return 0;
}
#ifdef BLINK_PRIVATE_ACTIONS
int ActionPrivate(void) {
  int (*registration)(int,const struct sigaction *,struct sigaction *)=blink_host_register_sigaction;
  struct sigaction action={0},old,copy,query;
  memset(&old,0x5a,sizeof(old));memcpy(&copy,&old,sizeof(copy));
  action.sa_handler=Simple;action.sa_flags=SA_ONSTACK;
  CHECK(sigaction(SIGUSR1,&action,&old)==-1 && errno==ENOTSUP && !memcmp(&old,&copy,sizeof(old)));
  action.sa_flags=SA_NODEFER;CHECK(sigaction(SIGUSR1,&action,0)==-1 && errno==ENOTSUP);
  action.sa_flags=SA_NOCLDSTOP;CHECK(sigaction(SIGCHLD,&action,0)==-1 && errno==ENOTSUP);
  action.sa_flags=SA_NOCLDWAIT;CHECK(sigaction(SIGCHLD,&action,0)==-1 && errno==ENOTSUP);
  action.sa_flags=0;action.sa_handler=SIG_ERR;CHECK(sigaction(SIGUSR1,&action,0)==-1 && errno==EINVAL);
  CHECK(!registration(SIGUSR1,0,&query) && query.sa_handler==SIG_DFL);
  CHECK(BlinkHostSignalActionsBegin()==-1 && errno==EBUSY);
  action.sa_flags=SA_SIGINFO;action.sa_handler=SIG_IGN;
  CHECK(!sigaction(SIGUSR1,&action,0));CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==SIG_IGN);
  action.sa_flags=0;action.sa_handler=Simple;
  CHECK(!sigaction(SIGUSR1,&action,&action) && action.sa_handler==SIG_IGN);
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==Simple && query.sa_restorer==0);
  for(int i=1;i<16;++i)CHECK(((uint64_t *)&query.sa_mask)[i]==0);
  BlinkHostSignalActionsEnd();
  CHECK(sigaction(SIGUSR1,0,&query)==-1 && errno==ENODEV);
  CHECK(signal(SIGUSR1,SIG_IGN)==SIG_ERR && errno==ENODEV);
  CHECK(!BlinkHostSignalActionsBegin());
  CHECK(!sigaction(SIGUSR1,0,&query) && query.sa_handler==SIG_DFL && query.sa_flags==0 && Mask(&query)==0);
  return 0;
}
int ActionWorkerSet(int second) {
  struct sigaction action={0};action.sa_handler=second?SIG_IGN:Simple;
  CHECK(!sigaction(SIGUSR2,&action,0));return 0;
}
int ActionWorkerRead(int second) {
  struct sigaction query;CHECK(!sigaction(SIGUSR2,0,&query));
  CHECK(query.sa_handler==(second?SIG_IGN:Simple));
  if(!second){calls=0;query.sa_handler(31);CHECK(calls==31);}return 0;
}
#endif
#ifndef BLINK_MANAGED_ACTIONS
#ifdef BLINK_PRIVATE_ACTIONS
int NativeActionGuard(int);
#endif
int main(void){
#ifdef BLINK_PRIVATE_ACTIONS
  if(NativeActionGuard(0) || BlinkHostSignalActionsBegin())return 1;
#endif
  int result=ActionProbe();
#ifdef BLINK_PRIVATE_ACTIONS
  if(!result)result=ActionPrivate();
  BlinkHostSignalActionsEnd();
  if(!result)result=NativeActionGuard(1);
#endif
  return result;
}
#endif
