#define _POSIX_C_SOURCE 200809L
#include <errno.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <sys/resource.h>
#include <sys/times.h>
#include <unistd.h>
#define CHECK(x) do { if(!(x))return __LINE__; } while(0)
int main(void) {
 struct rlimit current,after,invalid={1,0};
 CHECK(sizeof(rlim_t)==8 && sizeof(struct rlimit)==16 && _Alignof(struct rlimit)==8);
 CHECK(offsetof(struct rlimit,rlim_cur)==0 && offsetof(struct rlimit,rlim_max)==8);
 errno=0;CHECK(getrlimit(RLIMIT_NOFILE,&current)==0 && current.rlim_cur<=current.rlim_max);
 CHECK(setrlimit(RLIMIT_NOFILE,&current)==0);
 errno=0;CHECK(setrlimit(RLIMIT_NOFILE,&invalid)==-1 && errno==EINVAL);
 CHECK(getrlimit(RLIMIT_NOFILE,&after)==0 && after.rlim_cur==current.rlim_cur && after.rlim_max==current.rlim_max);
 after.rlim_cur=123;after.rlim_max=456;errno=0;
 CHECK(getrlimit(-1,&after)==-1 && errno==EINVAL && after.rlim_cur==123 && after.rlim_max==456);
 errno=0;int priority=getpriority(PRIO_PROCESS,0);CHECK(errno==0 && priority>=-20 && priority<=19);
 CHECK(getpriority(PRIO_PROCESS,(unsigned)getpid())==priority);
 CHECK(setpriority(PRIO_PROCESS,0,priority)==0);
 errno=0;CHECK(getpriority(-1,0)==-1 && errno==EINVAL);
 errno=0;CHECK(setpriority(-1,0,0)==-1 && errno==EINVAL);
 CHECK(sizeof(struct rusage)==144 && _Alignof(struct rusage)==8);
 CHECK(sizeof(struct tms)==32 && _Alignof(struct tms)==8);
 struct rusage usage={0};usage.ru_maxrss=123;
 CHECK(getrusage(99,&usage)==-1 && errno==EINVAL && usage.ru_maxrss==123);
 CHECK(!getrusage(RUSAGE_CHILDREN,&usage));
 unsigned char *bytes=(unsigned char*)&usage;
 for(unsigned long i=0;i<sizeof(usage);++i)CHECK(bytes[i]==0);
 puts("resource ABI and query/idempotent/error invariants: PASS");
 return 0;
}
