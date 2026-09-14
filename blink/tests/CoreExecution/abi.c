/* Native storage oracle for the explicit signal-jump host profile. This only
 * measures record layout; it supplies no instruction or host implementation. */
#include <setjmp.h>
#include <signal.h>
#include "abi.h"
typedef blink_host_signal_jump_storage core_profile_sigjmp_buf[1];
#define sigjmp_buf core_profile_sigjmp_buf
#include <stddef.h>
#include <stdio.h>
#include "blink/machine.h"
int main(void) {
  printf("abi Machine=%zu System=%zu ax=%zu ip=%zu flags=%zu onhalt=%zu\n",
         sizeof(struct Machine), sizeof(struct System), offsetof(struct Machine, ax),
         offsetof(struct Machine, ip), offsetof(struct Machine, flags),
         offsetof(struct Machine, onhalt));
  return 0;
}
