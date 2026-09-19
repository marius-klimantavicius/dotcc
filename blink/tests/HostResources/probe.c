#include <errno.h>
#include <limits.h>
#include <stddef.h>
#include <stdio.h>
#include <sys/resource.h>
#include "host-resources.h"
#include "host-io.h"
#include "HostMemory.h"
#include <sys/mman.h>
#define CHECK(x) do { if(!(x))return __LINE__; } while(0)
void ResourceGc(void);
int ResourceAbi(void) {
 struct rlimit value;unsigned char *p=(unsigned char *)&value;
 CHECK(sizeof(value)==16 && _Alignof(struct rlimit)==8);
 CHECK((unsigned char *)&value.rlim_cur-p==0 && (unsigned char *)&value.rlim_max-p==8);
 value.rlim_cur=0x0102030405060708UL;
 CHECK(p[0]==8 && p[7]==1);
 CHECK(sizeof(struct rusage)==144 && _Alignof(struct rusage)==8);
 CHECK(sizeof(struct tms)==32 && _Alignof(struct tms)==8);
 return 0;
}
int UnboundResources(int error) {
 struct rlimit value={123,456};errno=0;
 CHECK(getrlimit(RLIMIT_NOFILE,&value)==-1 && errno==error && value.rlim_cur==123 && value.rlim_max==456);
 errno=0;CHECK(getpriority(PRIO_PROCESS,0)==-1 && errno==error);
 struct rusage usage={0};usage.ru_maxrss=123;
 CHECK(getrusage(RUSAGE_CHILDREN,&usage)==-1 && errno==error && usage.ru_maxrss==123);
 struct tms cpu={1,2,3,4};
 CHECK(times(&cpu)==-1 && errno==error && cpu.tms_utime==1 && cpu.tms_cstime==4);
 return 0;
}
int ResourcePolicy(unsigned long capacity,unsigned int pid) {
 struct rlimit value={123,456},requested;
 errno=123;CHECK(getrlimit(RLIMIT_NOFILE,&value)==0 && errno==123);
 CHECK(value.rlim_cur==capacity && value.rlim_max==capacity);
 CHECK(setrlimit(RLIMIT_NOFILE,&value)==0 && errno==123);
 requested.rlim_cur=capacity-1;requested.rlim_max=capacity;
 CHECK(setrlimit(RLIMIT_NOFILE,&requested)==-1 && errno==EPERM);
 requested.rlim_cur=capacity+1;requested.rlim_max=capacity;
 CHECK(setrlimit(RLIMIT_NOFILE,&requested)==-1 && errno==EINVAL);
 requested.rlim_cur=RLIM_INFINITY;requested.rlim_max=RLIM_INFINITY;
 CHECK(setrlimit(RLIMIT_NOFILE,&requested)==-1 && errno==EPERM);
 requested.rlim_cur=0;requested.rlim_max=0;
 CHECK(setrlimit(RLIMIT_NOFILE,&requested)==-1 && errno==EPERM);
 CHECK(getrlimit(RLIMIT_NOFILE,0)==-1 && errno==EFAULT);
 CHECK(setrlimit(RLIMIT_NOFILE,0)==-1 && errno==EFAULT);
 value.rlim_cur=123;value.rlim_max=456;
 CHECK(getrlimit(-1,&value)==-1 && errno==EINVAL && value.rlim_cur==123 && value.rlim_max==456);
 CHECK(getrlimit(INT_MAX,&value)==-1 && errno==EINVAL && value.rlim_cur==123 && value.rlim_max==456);
 CHECK(getrlimit(RLIMIT_CPU,&value)==-1 && errno==EOPNOTSUPP && value.rlim_cur==123 && value.rlim_max==456);
 for(int which=PRIO_PROCESS;which<=PRIO_USER;++which){
   unsigned int target=which==PRIO_USER?0:pid;
   errno=123;CHECK(getpriority(which,0)==0 && errno==123);
   CHECK(getpriority(which,target)==0 && errno==123);
   CHECK(setpriority(which,target,0)==0 && errno==123);
   CHECK(setpriority(which,target,-1)==-1 && errno==EPERM);
   CHECK(setpriority(which,target,INT_MIN)==-1 && errno==EPERM);
   CHECK(setpriority(which,target,INT_MAX)==-1 && errno==EPERM);
   CHECK(getpriority(which,(unsigned int)-1)==-1 && errno==ESRCH);
 }
 CHECK(getpriority(-1,0)==-1 && errno==EINVAL);
 CHECK(setpriority(3,0,0)==-1 && errno==EINVAL);
 struct rusage usage={0};usage.ru_maxrss=123;
 CHECK(getrusage(RUSAGE_SELF,&usage)==-1 && errno==EOPNOTSUPP && usage.ru_maxrss==123);
 CHECK(getrusage(99,&usage)==-1 && errno==EINVAL && usage.ru_maxrss==123);
 CHECK(getrusage(RUSAGE_CHILDREN,0)==-1 && errno==EFAULT);
 CHECK(!getrusage(RUSAGE_CHILDREN,&usage));
 unsigned char *bytes=(unsigned char*)&usage;
 for(unsigned long i=0;i<sizeof(usage);++i)CHECK(bytes[i]==0);
 struct tms cpu={1,2,3,4};
 CHECK(times(&cpu)==-1 && errno==EOPNOTSUPP && cpu.tms_utime==1 && cpu.tms_stime==2 && cpu.tms_cutime==3 && cpu.tms_cstime==4);
 ResourceGc();
 CHECK(getrlimit(RLIMIT_NOFILE,&value)==0 && value.rlim_cur==capacity && value.rlim_max==capacity);
 CHECK(getpriority(PRIO_PROCESS,pid)==0);
 return 0;
}

int MemoryResourcePolicy(unsigned long budget) {
 struct rlimit value={123,456};
 CHECK(BlinkHostMemoryLimit()==0);
 CHECK(getrlimit(RLIMIT_AS,&value)==-1 && errno==ENODEV && value.rlim_cur==123 && value.rlim_max==456);
 CHECK(BlinkHostMemoryBegin((size_t)-1)==-1 && errno==EINVAL && BlinkHostMemoryLimit()==0);
 CHECK(BlinkHostMemoryBegin(budget)==0 && BlinkHostMemoryLimit()==budget);
 for(int pass=0;pass<2;++pass){
   int resource=pass?RLIMIT_DATA:RLIMIT_AS;
   errno=123;CHECK(getrlimit(resource,&value)==0 && errno==123 && value.rlim_cur==budget && value.rlim_max==budget);
   CHECK(setrlimit(resource,&value)==0 && errno==123);
   value.rlim_cur=budget-1;CHECK(setrlimit(resource,&value)==-1 && errno==EPERM);
 }
 void *mapping=blink_host_mmap(0,4096,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
 CHECK(mapping!=MAP_FAILED && BlinkHostMemoryBytes()>4096);
 CHECK(blink_host_mmap(0,budget,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0)==MAP_FAILED && errno==ENOMEM);
 CHECK(getrlimit(RLIMIT_AS,&value)==0 && value.rlim_cur==budget && value.rlim_max==budget);
 CHECK(BlinkHostMemoryEnd()==-1 && errno==EBUSY && BlinkHostMemoryLimit()==budget);
 ResourceGc();
 CHECK(BlinkHostMemoryLimit()==budget);
 CHECK(blink_host_munmap(mapping,4096)==0 && BlinkHostMemoryEnd()==0 && BlinkHostMemoryLimit()==0);
 value.rlim_cur=123;value.rlim_max=456;
 CHECK(getrlimit(RLIMIT_DATA,&value)==-1 && errno==ENODEV && value.rlim_cur==123 && value.rlim_max==456);
 CHECK(BlinkHostMemoryBegin(budget)==0);
 BlinkHostMemoryDisposeWorker();CHECK(BlinkHostMemoryLimit()==0);
 return 0;
}
