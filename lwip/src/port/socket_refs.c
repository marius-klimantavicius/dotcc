/* Authored owning-call admission boundary. Original socket algorithms unchanged.
 * The public debug getter does not pin fd lifetime; original static get/done do.
 * Caller must release exactly once and retain the owning translated instance. */
#include "api/sockets.c"
#if LWIP_SOCKET && LWIP_NETCONN_FULLDUPLEX
void *lwip_owned_socket_pin(int descriptor) {return get_socket(descriptor);}
void lwip_owned_socket_unpin(void *handle) {done_socket((struct lwip_sock*)handle);}
#endif
