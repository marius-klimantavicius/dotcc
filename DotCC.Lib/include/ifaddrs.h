#ifndef DOTCC_IFADDRS_H
#define DOTCC_IFADDRS_H
#include <sys/socket.h>
/* Linux LP64 interface-list ABI; allocation belongs to getifaddrs/freeifaddrs. */
struct ifaddrs {
    struct ifaddrs *ifa_next;
    char *ifa_name;
    unsigned int ifa_flags;
    struct sockaddr *ifa_addr;
    struct sockaddr *ifa_netmask;
    union {
        struct sockaddr *ifu_broadaddr;
        struct sockaddr *ifu_dstaddr;
    } ifa_ifu;
    void *ifa_data;
};
#define ifa_broadaddr ifa_ifu.ifu_broadaddr
#define ifa_dstaddr ifa_ifu.ifu_dstaddr
int getifaddrs(struct ifaddrs **interfaces);
void freeifaddrs(struct ifaddrs *interfaces);
#endif
