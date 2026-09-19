#if defined(BLINK_NATIVE_PROFILE)
#include <setjmp.h>
#include <signal.h>
#include "abi.h"
typedef blink_host_signal_jump_storage core_profile_sigjmp_buf[1];
#define sigjmp_buf core_profile_sigjmp_buf
#endif
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "GuestResources.c"
bool FLAG_nolinear = true;
struct HostPages g_hostpages;
#define CHECK(x) do { if (!(x)) return __LINE__; } while(0)
void ResourceGc(void);
static void Fresh(struct System *s) {
  memset(s,0,sizeof(*s));
  for (int i=0;i<RLIM_NLIMITS_LINUX;++i) {
    Write64(s->rlim[i].cur,RLIM_INFINITY_LINUX);
    Write64(s->rlim[i].max,RLIM_INFINITY_LINUX);
  }
}
int ResourceSeedProbe(size_t budget, size_t descriptors) {
  struct System *s = malloc(sizeof(*s));
  struct System *before = malloc(sizeof(*s));
  CHECK(s && before);
  Fresh(s); memcpy(before,s,sizeof(*s));
  CHECK(BlinkHostInitializeResourceLimits(s,descriptors)==-1 && errno==ENODEV);
  CHECK(memcmp(s,before,sizeof(*s))==0);
  CHECK(BlinkHostMemoryBegin(budget)==0);
  CHECK(BlinkHostInitializeResourceLimits(0,descriptors)==-1 && errno==EFAULT);
  CHECK(BlinkHostInitializeResourceLimits(s,0)==-1 && errno==EINVAL);
  CHECK(BlinkHostInitializeResourceLimits(s,(size_t)INT_MAX+1)==-1 && errno==EINVAL);
  CHECK(memcmp(s,before,sizeof(*s))==0);
  s->cr3=4096; memcpy(before,s,sizeof(*s));
  CHECK(BlinkHostInitializeResourceLimits(s,descriptors)==-1 && errno==EBUSY);
  CHECK(memcmp(s,before,sizeof(*s))==0); Fresh(s);
  s->machines=(struct Dll *)s; memcpy(before,s,sizeof(*s));
  CHECK(BlinkHostInitializeResourceLimits(s,descriptors)==-1 && errno==EBUSY);
  CHECK(memcmp(s,before,sizeof(*s))==0); Fresh(s);
  Write64(s->rlim[0].cur,1); memcpy(before,s,sizeof(*s));
  CHECK(BlinkHostInitializeResourceLimits(s,descriptors)==-1 && errno==EBUSY);
  CHECK(memcmp(s,before,sizeof(*s))==0); Fresh(s);
  errno=123;
  CHECK(BlinkHostInitializeBoundResourceLimits(s)==0 && errno==123);
  ResourceGc();
  for(int i=0;i<RLIM_NLIMITS_LINUX;++i) {
    size_t want = i==RLIMIT_AS_LINUX || i==RLIMIT_DATA_LINUX ? budget :
        i==RLIMIT_NOFILE_LINUX ? descriptors : (size_t)RLIM_INFINITY_LINUX;
    CHECK(Read64(s->rlim[i].cur)==want && Read64(s->rlim[i].max)==want);
  }
  /* Independently inspect guest little-endian bytes, not a canceling roundtrip. */
  CHECK(s->rlim[RLIMIT_AS_LINUX].cur[0]==(budget &255));
  CHECK(s->rlim[RLIMIT_AS_LINUX].cur[1]==((budget>>8)&255));
  memcpy(before,s,sizeof(*s));
  CHECK(BlinkHostInitializeBoundResourceLimits(s)==-1 && errno==EBUSY);
  CHECK(memcmp(s,before,sizeof(*s))==0);
  CHECK(BlinkHostMemoryEnd()==0);
  free(before);free(s);return 0;
}

void ResourceSeedAbi(void) {
  struct System s;
  printf("System size=%zu rlim-offset=%zu rlim-bytes=%zu record=%zu\n",
         sizeof(s),(size_t)((char *)&s.rlim-(char *)&s),sizeof(s.rlim),sizeof(s.rlim[0]));
}
