#define _GNU_SOURCE 1
#include <signal.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
static void Handler(int signal){(void)signal;}
int main(void){
  struct sigaction action={0},old;
  action.sa_handler=Handler;memset(&action.sa_mask,255,sizeof(action.sa_mask));
  action.sa_flags=SA_SIGINFO|SA_RESTART;
  if(sigaction(SIGUSR1,&action,0)||sigaction(SIGUSR1,0,&old))return 1;
  uint64_t low;memcpy(&low,&old.sa_mask,8);
  printf("mask=%016llx flags=%x size=%lu\n",(unsigned long long)low,old.sa_flags,(unsigned long)sizeof(old));
  errno=0;int rc=sigaction(SIGKILL,0,&old);printf("queryKILL=%d errno=%d\n",rc,errno);
  errno=0;rc=sigaction(SIGSTOP,0,&old);printf("querySTOP=%d errno=%d\n",rc,errno);
  errno=0;rc=sigaction(32,0,&old);printf("query32=%d errno=%d\n",rc,errno);
  errno=0;rc=sigaction(33,0,&old);printf("query33=%d errno=%d\n",rc,errno);
  signal(SIGUSR2,Handler);sigaction(SIGUSR2,0,&old);memcpy(&low,&old.sa_mask,8);
  printf("signal-mask=%016llx flags=%x\n",(unsigned long long)low,old.sa_flags);
  return 0;
}
