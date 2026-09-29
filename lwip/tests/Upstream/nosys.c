/* Test-only scheduling/applicability adapter. Original protocol tests unchanged. */
#include "lwip_check.h"
#include "lwip/netif.h"
#include "lwip/timeouts.h"
#if NO_SYS
/* Upstream's sockets_suite has this same zero-registration branch, but its
 * unguarded file-scope helpers require unavailable socket types under NO_SYS. */
Suite *sockets_suite(void) { return create_suite("SOCKETS", NULL, 0, NULL, NULL); }
/* Original IP teardown asks its scheduler to release pending loopback work.
 * NO_SYS uses the public main-loop APIs instead of a nonexistent tcpip worker. */
int tcpip_thread_poll_one(void) { netif_poll_all(); sys_check_timeouts(); return 0; }
#endif
