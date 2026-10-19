#define _GNU_SOURCE 1
#include <stddef.h>
#include <errno.h>
#include <dlfcn.h>
#include <unistd.h>
#include <sys/syscall.h>
/* Test process only: the runtime has no allocator hook. Fail exactly one
   48-byte addrinfo +16-byte IPv4 clone on the arming OS thread. The host's
   numeric getaddrinfo uses malloc, independently checked by the fault count.
   This fixture is never linked into a product or used as a resolver backend. */
extern void *__libc_calloc(size_t, size_t);
static _Thread_local int armed, countdown, matched, failed;
static _Thread_local int arm_tid, fault_tid;
static _Thread_local void *fault_caller;
void resolver_test_arm(int after) { countdown=after; matched=failed=0; fault_tid=0; fault_caller=NULL; arm_tid=(int)syscall(SYS_gettid); armed=1; }
void resolver_test_disarm(int *calls,int *failures) { armed=0; *calls=matched; *failures=failed; }
void resolver_test_observe(int *arm,int *fault,void **caller) { *arm=arm_tid; *fault=fault_tid; *caller=fault_caller; }
const char *resolver_test_caller_module(void) {
    Dl_info info;
    return dladdr(fault_caller,&info)!=0 && info.dli_fname ? info.dli_fname : "<managed-jit>";
}
void *calloc(size_t count,size_t size) {
    if(armed && ((count==1 && size==64)||(count==64 && size==1))) {
        matched++;
        if(countdown--==0) { armed=0; failed++; fault_tid=(int)syscall(SYS_gettid); fault_caller=__builtin_return_address(0); errno=ENOMEM; return NULL; }
    }
    return __libc_calloc(count,size);
}
