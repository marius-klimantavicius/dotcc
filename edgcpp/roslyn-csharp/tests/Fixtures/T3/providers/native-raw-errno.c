/* Test interference only, called outside generated provider gateways to model
   unrelated CLR/host work. No translated application implementation. */
#include <errno.h>
#include <stdint.h>
void raw_errno_write(int value) { errno = value; }
int raw_errno_read(void) { return errno; }
uintptr_t raw_errno_address(void) { return (uintptr_t)__errno_location(); }
