/* Authored synchronous owner-local signal profile. GNU atomic builtins retain
   source function pointers unchanged; generated globals provide owner storage. */
#include <signal.h>
#include <stdlib.h>
#include <errno.h>
#ifdef __cplusplus
extern "C" {
#endif
#if defined(__cplusplus)
[[noreturn]]
#else
_Noreturn
#endif
void __edg_signal_terminate(int);
typedef void (*handler_t)(int);
static handler_t handlers[6];
static int signal_index(int sig)
{
 switch(sig) {
 case SIGABRT:return 0;case SIGFPE:return 1;case SIGILL:return 2;
 case SIGINT:return 3;case SIGSEGV:return 4;case SIGTERM:return 5;
 default:return -1;
 }
}
handler_t signal(int sig,handler_t handler)
{
 int index=signal_index(sig);
 handler_t previous;
 if(index<0 || handler==SIG_ERR){errno=EINVAL;return SIG_ERR;}
 __atomic_exchange(&handlers[index],&handler,&previous,__ATOMIC_SEQ_CST);
 return previous;
}
int raise(int sig)
{
 int index=signal_index(sig);
 handler_t handler,reset=SIG_DFL;
 if(index<0){errno=EINVAL;return -1;}
 __atomic_load(&handlers[index],&handler,__ATOMIC_SEQ_CST);
 for(;;) {
  if(handler==SIG_IGN)return 0;
  if(handler==SIG_DFL)__edg_signal_terminate(sig);
  if(__atomic_compare_exchange(&handlers[index],&handler,&reset,0,
       __ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST))break;
 }
 handler(sig);
 return 0;
}
void abort(void)
{
 raise(SIGABRT);
 __edg_signal_terminate(SIGABRT);
}
#ifdef __cplusplus
}
#endif
