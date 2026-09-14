/* An owning, process-discarded test worker. No core call may follow disposal. */
#include "HostMemory.h"
#define main CoreProbe
#include "probe.c"
#undef main

static int driver_has_run;
int main(void) {
  if (driver_has_run) return 21;
  driver_has_run = 1;
  if (BlinkHostMemoryBegin(64 * 1024 * 1024)) return 20;
  int result = CoreProbe();
  printf("core retained-mappings=%zu charged-bytes=%zu\n",
         BlinkHostMemoryMappings(), BlinkHostMemoryBytes());
  /* FreeMachine runs after each case, but upstream retains slab-cache pointers.
   * This wrapper can run only once in a fresh worker; disposal requires that
   * the caller now discard all upstream static state with that worker. */
  BlinkHostMemoryDisposeWorker();
  return result;
}
