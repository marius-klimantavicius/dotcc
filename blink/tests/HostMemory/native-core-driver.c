#include "HostMemory.h"
#include <stdio.h>

int NativeCoreProbe(void);
int main(void) {
  if (BlinkHostMemoryBegin(64 * 1024 * 1024)) return 20;
  int result = NativeCoreProbe();
  printf("native-core retained-mappings=%zu charged-bytes=%zu\n",
         BlinkHostMemoryMappings(), BlinkHostMemoryBytes());
  /* No upstream function is called after disposal. This oracle process exits
   * immediately, discarding g_allocator/g_hostpages/g_bus with its worker. */
  BlinkHostMemoryDisposeWorker();
  return result;
}
