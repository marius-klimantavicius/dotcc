/* Shared portable network declarations for selected musl and POSIX headers. */
#ifndef EDG_NETWORK_ABI_H
#define EDG_NETWORK_ABI_H
#include <stdint.h>
typedef unsigned int socklen_t;
#ifndef AF_INET
#define AF_INET 2
#define AF_INET6 10
#endif
struct in6_addr {
    union {
        uint8_t __s6_addr[16];
        uint16_t __s6_addr16[8];
        uint32_t __s6_addr32[4];
    } __in6_union;
};
#define s6_addr __in6_union.__s6_addr
#define s6_addr16 __in6_union.__s6_addr16
#define s6_addr32 __in6_union.__s6_addr32
#define IN6ADDR_ANY_INIT {{{0}}}
#define IN6ADDR_LOOPBACK_INIT {{{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1}}}
#ifdef __cplusplus
extern "C" {
#endif
extern const struct in6_addr in6addr_any;
extern const struct in6_addr in6addr_loopback;
uint16_t htons(uint16_t);
uint16_t ntohs(uint16_t);
uint32_t htonl(uint32_t);
uint32_t ntohl(uint32_t);
const char *inet_ntop(int, const void *, char *, socklen_t);
#ifdef __cplusplus
}
#endif
#endif
