#ifndef DOTCC_LWIP_OPTS_H
#define DOTCC_LWIP_OPTS_H
#include "profile.h"
#ifdef DOTCC_TCPIP_WORKER
#define LWIP_TCPIP_THREAD_ALIVE() do { if (lwip_host_worker_stopping()) { UNLOCK_TCPIP_CORE(); return; } } while (0)
#endif
#if !NO_SYS && !defined(DOTCC_LWIP_UNIT)
#if LWIP_TCPIP_CORE_LOCKING
#define LOCK_TCPIP_CORE() lwip_host_lock_core()
#define UNLOCK_TCPIP_CORE() lwip_host_unlock_core()
#endif
#define LWIP_ASSERT_CORE_LOCKED() lwip_host_assert_core_locked()
#define LWIP_MARK_TCPIP_THREAD() lwip_host_mark_tcpip_thread()
#define LWIP_NETCONN_THREAD_SEM_GET() lwip_host_netconn_sem_get()
#define LWIP_NETCONN_THREAD_SEM_ALLOC() lwip_host_netconn_sem_alloc()
#define LWIP_NETCONN_THREAD_SEM_FREE() lwip_host_netconn_sem_free()
#endif
#endif
