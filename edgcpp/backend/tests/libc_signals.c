/* Synthetic source contracts for the synchronous owner-local signal profile. */
#include <signal.h>
#include <stdint.h>
#include <errno.h>
#include <stdlib.h>
#include <assert.h>
#include <stdio.h>
#ifndef __cplusplus
#include <setjmp.h>
#endif
#ifdef __cplusplus
extern "C" {
#endif
static volatile sig_atomic_t count, observed, reset_seen;
static void first(int sig){count+=1;observed=sig;reset_seen=signal(sig,SIG_IGN)==SIG_DFL;}
static void second(int sig){count+=10;observed=sig;reset_seen=signal(sig,SIG_IGN)==SIG_DFL;}
static void finish(int sig){_Exit(sig==SIGABRT?73:74);}
static void recursive_abort(int sig){(void)sig;abort();}
int signals_contract(void)
{
 int numbers[6]={SIGABRT,SIGFPE,SIGILL,SIGINT,SIGSEGV,SIGTERM};
 int invalid[6]={-2147483647-1,-1,0,1,16,2147483647};
 void (*(*install)(int,void(*)(int)))(int)=signal;
 int (*deliver)(int)=raise;
 int i;
 if(sizeof(sig_atomic_t)!=4 || (intptr_t)SIG_DFL!=0 ||
    (intptr_t)SIG_IGN!=1 || (intptr_t)SIG_ERR!=-1)return 1;
 for(i=0;i<6;++i){errno=79;if(install(numbers[i],SIG_IGN)!=SIG_DFL || errno!=79)return 2;
  if(deliver(numbers[i]) || install(numbers[i],SIG_DFL)!=SIG_IGN)return 3;}
 for(i=0;i<6;++i){errno=0;if(install(invalid[i],first)!=SIG_ERR || errno!=EINVAL)return 4;
  errno=0;if(deliver(invalid[i])!=-1 || errno!=EINVAL)return 5;}
 errno=0;if(install(SIGINT,SIG_ERR)!=SIG_ERR || errno!=EINVAL)return 6;
 if(install(SIGINT,first)!=SIG_DFL || install(SIGINT,second)!=first ||
    install(SIGINT,first)!=second)return 7;
 count=observed=reset_seen=0;errno=79;
 if(deliver(SIGINT) || count!=1 || observed!=SIGINT || !reset_seen || errno!=79)return 8;
 if(deliver(SIGINT) || count!=1 || install(SIGINT,SIG_DFL)!=SIG_IGN)return 9;
 return 0;
}
void signals_owner_install(int value)
{count=0;signal(SIGINT,value?second:first);}
int signals_owner_raise(void)
{raise(SIGINT);return count;}
int signals_owner_count(void){return count;}
uintptr_t signals_handler_address(void){return (uintptr_t)first;}
int signals_registration_stress(int which)
{
 int i;void(*old)(int);
 for(i=0;i<10000;++i){old=signal(SIGTERM,which?first:second);
  if(old!=SIG_DFL && old!=first && old!=second)return 1;}
 return 0;
}
void signals_stress_reset(void){signal(SIGTERM,SIG_DFL);}
#ifndef __cplusplus
static jmp_buf destination;
static void escape(int sig){longjmp(destination,sig);}
int signals_abort_escape(void)
{
 if(setjmp(destination)==0){signal(SIGABRT,escape);abort();}
 return signal(SIGABRT,SIG_DFL)==SIG_DFL?0:1;
}
#endif
static void finalizer(void){fputs("source-finalizer\n",stderr);}
#ifdef __cplusplus
}
struct signal_guard {~signal_guard(){fputs("source-unwound\n",stderr);}};
extern "C" {
#endif
void signals_fatal(int mode)
{
 atexit(finalizer);
#ifdef __cplusplus
 signal_guard guard;
#endif
 if(mode==0){signal(SIGTERM,SIG_DFL);raise(SIGTERM);}
 if(mode==1){signal(SIGABRT,SIG_IGN);abort();}
 if(mode==2){signal(SIGABRT,first);abort();}
 if(mode==3){signal(SIGABRT,recursive_abort);abort();}
 if(mode==4){signal(SIGABRT,finish);abort();}
 if(mode==5){signal(SIGABRT,finish);assert(!"synthetic signal assertion");}
 if(mode==6){signal(SIGINT,first);raise(SIGINT);signal(SIGINT,SIG_DFL);raise(SIGINT);}
}
#ifdef __cplusplus
}
#endif
