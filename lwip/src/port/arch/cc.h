#ifndef DOTCC_LWIP_ARCH_CC_H
#define DOTCC_LWIP_ARCH_CC_H
#include <stdint.h>
#include <stddef.h>
#include <sys/types.h>
#if defined(DOTCC_LWIP_TOOL)
#include <unistd.h>
#else
#define LWIP_NO_UNISTD_H 1
#endif
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#ifdef DOTCC_LWIP_IP6_HOP_LIMIT
#include "ip6-hop-limit-value.h"
#endif
#ifndef BYTE_ORDER
#define BYTE_ORDER LITTLE_ENDIAN
#endif
#define PACK_STRUCT_USE_INCLUDES
#define PACK_STRUCT_STRUCT
#if !defined(DOTCC_LWIP_TOOL)
extern uint32_t lwip_host_random(void);
extern void lwip_host_assert(const char *message, const char *file, int line);
extern void lwip_host_assert_core_locked(void);
extern void lwip_host_mark_tcpip_thread(void);
extern int lwip_host_worker_stopping(void);
extern void lwip_host_lock_core(void);
extern void lwip_host_unlock_core(void);
#define LWIP_RAND() lwip_host_random()
#define LWIP_PLATFORM_ASSERT(message) lwip_host_assert(message, __FILE__, __LINE__)
#define LWIP_PLATFORM_DIAG(message) do { printf message; } while (0)
#else
#define LWIP_RAND() ((uint32_t)rand())
#endif
#endif
