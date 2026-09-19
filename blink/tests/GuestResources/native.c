#include <errno.h>
#include <stdio.h>
#include <string.h>
#include "blink/machine.h"
#include "blink/endian.h"
#include "GuestResources.h"
#include "HostMemory.h"
#define CHECK(x) do { if (!(x)) { fprintf(stderr,"line %d\n",__LINE__); return 1; } } while(0)
void TerminateSignal(struct Machine *m, int sig, int code) { (void)m;(void)sig;(void)code; }
int main(void) {
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  CHECK(s);
  CHECK(BlinkHostInitializeResourceLimits(s, 19) == -1 && errno == ENODEV);
  CHECK(BlinkHostMemoryBegin(65537) == 0);
  CHECK(BlinkHostInitializeResourceLimits(s, 19) == 0);
  CHECK(GetFileDescriptorLimit(s) == 19);
  CHECK(GetMaxVss(s) == 16 && GetMaxRss(s) == 16);
  CHECK(Read64(s->rlim[RLIMIT_DATA_LINUX].max) == 65537);
  CHECK(Read64(s->rlim[RLIMIT_CPU_LINUX].max) == RLIM_INFINITY_LINUX);
  CHECK(BlinkHostInitializeResourceLimits(s, 19) == -1 && errno == EBUSY);
  FreeSystem(s);
  CHECK(BlinkHostMemoryEnd() == 0);
  puts("actual NewSystem and limit consumers: PASS");
  return 0;
}
