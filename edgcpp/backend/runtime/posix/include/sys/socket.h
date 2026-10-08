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
/* Proposed additions to existing portable sys/socket.h; design only. */
#define SOCK_STREAM 1
#define SOCK_NONBLOCK 04000
#define SOCK_CLOEXEC 02000000
#define SHUT_RD 0
#define SHUT_WR 1
#define SHUT_RDWR 2
#ifdef __cplusplus
extern "C" {
#endif
int socket(int, int, int);
int bind(int, const struct sockaddr *, socklen_t);
int listen(int, int);
int accept(int, struct sockaddr *__restrict, socklen_t *__restrict);
int connect(int, const struct sockaddr *, socklen_t);
int getsockname(int, struct sockaddr *__restrict, socklen_t *__restrict);
int shutdown(int, int);
#ifdef __cplusplus
}
#endif
/* Add IPPROTO_TCP=6 to netinet/in.h. AF/PF and structs already exist.
   Byte-order declarations are already provided by network_abi.h. */

#endif
