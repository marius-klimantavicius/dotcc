#ifndef EDG_POSIX_SYS_SOCKET_H
#define EDG_POSIX_SYS_SOCKET_H
#include <sys/types.h>
/* Portable Linux/musl family numbers; never passed to host APIs unchanged. */
typedef unsigned short sa_family_t;
typedef unsigned int socklen_t;
#define AF_UNSPEC 0
#define AF_INET 2
#define AF_INET6 10
#define PF_UNSPEC AF_UNSPEC
#define PF_INET AF_INET
#define PF_INET6 AF_INET6
struct sockaddr { sa_family_t sa_family; char sa_data[14]; };
struct sockaddr_storage {
    sa_family_t ss_family;
    char __padding[118];
    unsigned long __alignment;
};
/* Socket I/O is not implemented by this profile. */
#endif
