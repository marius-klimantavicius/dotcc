/* One process is one discarded CPU-conformance worker. */
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#define CPU_CONFORMANCE_INITIALIZE_SYSTEM BlinkHostInitializeBoundResourceLimits
#define CPU_CONFORMANCE_NO_MAIN 1
#include "interpreter.c"
static int cpu_driver_has_run;
int CpuConformanceRun(int index) {
  if(index<0 || index>=CPU_CASES)return 2;
  if(cpu_cases[index].fault)return 2;
  if(cpu_driver_has_run)return 21;
  cpu_driver_has_run=1;
  if(BlinkHostMemoryBegin(64*1024*1024))return 20;
  if(BlinkHostSignalActionsBegin()){BlinkHostMemoryDisposeWorker();return 23;}
  if(BlinkHostExitCallbacksBegin()){
    BlinkHostSignalActionsEnd();BlinkHostMemoryDisposeWorker();return 24;
  }
  if(BlinkHostMemoryEnablePrivateFiles()){
    BlinkHostExitCallbacksEnd();BlinkHostSignalActionsEnd();BlinkHostMemoryDisposeWorker();return 22;
  }
  InitMap();InitBus();
  int result=CpuInterpreterCase(index);
  if(BlinkHostExitCallbacksRun() && !result)result=25;
  printf("{\"ownerMappings\":%zu,\"ownerBytes\":%zu}\n",
         BlinkHostMemoryMappings(),BlinkHostMemoryBytes());
  BlinkHostExitCallbacksEnd();BlinkHostSignalActionsEnd();BlinkHostMemoryDisposeWorker();
  /* No translated call may follow. Slab globals still reference discarded
   * mappings; the process owner must now tear down and exit. */
  return result;
}
