/* An owning, process-discarded test worker. No core call may follow disposal. */
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#define BLINK_CORE_INITIALIZE_SYSTEM BlinkHostInitializeBoundResourceLimits
#define main CoreProbe
#include "probe.c"
#undef main

static int driver_has_run;
int main(void) {
  if (driver_has_run) return 21;
  driver_has_run = 1;
  if (BlinkHostMemoryBegin(64 * 1024 * 1024)) return 20;
  if (BlinkHostSignalActionsBegin()) { BlinkHostMemoryDisposeWorker(); return 23; }
  if (BlinkHostExitCallbacksBegin()) {
    BlinkHostSignalActionsEnd();
    BlinkHostMemoryDisposeWorker();
    return 24;
  }
  if (BlinkHostMemoryEnablePrivateFiles()) {
    BlinkHostExitCallbacksEnd();
    BlinkHostSignalActionsEnd();
    BlinkHostMemoryDisposeWorker();
    return 22;
  }
  int result = CoreProbe();
  /* Upstream atexit destructors still need mappings, virtual masks, private IO
   * and cached private environment values. Fatal unwinds skip this normal path. */
  if (BlinkHostExitCallbacksRun() && !result) result = 25;
  printf("core retained-mappings=%zu charged-bytes=%zu\n",
         BlinkHostMemoryMappings(), BlinkHostMemoryBytes());
  /* FreeMachine runs after each case, but upstream retains slab-cache pointers.
   * This wrapper can run only once in a fresh worker; disposal requires that
   * the caller now discard all upstream static state with that worker. */
  BlinkHostExitCallbacksEnd();
  BlinkHostSignalActionsEnd();
  BlinkHostMemoryDisposeWorker();
  return result;
}
