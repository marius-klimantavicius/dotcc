/* Deliberately hostile authored abort provider: verifies runtime containment,
   not valid user abort/terminate-handler behavior under ISO C++. */
#include <setjmp.h>
#include <threads.h>
#include <stdlib.h>
extern "C" [[noreturn]] void __edg_cxx_terminate_default() noexcept;
static void (*callback)();
static void (*cleanup)();
static int behavior;
static jmp_buf destination;
struct Probe {~Probe(){cleanup();}};
extern "C" void abort(void)
{
 Probe probe;
 if(behavior==0)callback();
 if(behavior==1)longjmp(destination,1);
 if(behavior==2)thrd_exit(12);
 _Exit(97);
}
static int worker(void*){__edg_cxx_terminate_default();return 97;}
extern "C" void terminate_abort_boundary(int mode,void(*foreign)(),void(*observe)())
{
 behavior=mode;callback=foreign;cleanup=observe;
 if(mode==2){thrd_t thread;if(thrd_create(&thread,worker,0))return;int result;thrd_join(thread,&result);return;}
 if(setjmp(destination))return;
 __edg_cxx_terminate_default();
}
