#ifndef BLINK_PRIVATE_MEMBARRIER_H
#define BLINK_PRIVATE_MEMBARRIER_H
/* Reviewed single-guest-thread/no-fork boundary. This is not a host syscall
 * declaration and does not claim synchronization across guest processes. */
int blink_host_membarrier(int command, unsigned flags, int cpu_id);
#endif
