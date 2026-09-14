#ifndef BLINK_MANAGED_SYS_SOCKET_H
#define BLINK_MANAGED_SYS_SOCKET_H
#include <sys/types.h>
#include <sys/uio.h>
#include "abi.h"
#include "socket-constants.h"
typedef uint32_t socklen_t;
typedef uint16_t sa_family_t;
#define sockaddr blink_host_sockaddr
#define sockaddr_storage blink_host_sockaddr_storage
#define msghdr blink_host_msghdr
#define cmsghdr blink_host_cmsghdr
/* Keep the standard tag local to the authored header. A token macro for
 * linger also rewrites unrelated guest fields such as linger_linux.linger. */
struct linger { int32_t l_onoff; int32_t l_linger; };
#define ucred blink_host_ucred
#define socket blink_host_socket
#define bind blink_host_bind
#define listen blink_host_listen
#define accept blink_host_accept
#define connect blink_host_connect
#define send blink_host_send
#define recv blink_host_recv
#define sendto blink_host_sendto
#define recvfrom blink_host_recvfrom
#define sendmsg blink_host_sendmsg
#define recvmsg blink_host_recvmsg
#define setsockopt blink_host_setsockopt
#define getsockopt blink_host_getsockopt
#define getsockname blink_host_getsockname
#define getpeername blink_host_getpeername
#define shutdown blink_host_shutdown
#define sockatmark blink_host_sockatmark
int socket(int, int, int);
int bind(int, const struct sockaddr *, socklen_t);
int listen(int, int);
int accept(int, struct sockaddr *, socklen_t *);
int connect(int, const struct sockaddr *, socklen_t);
ssize_t send(int, const void *, size_t, int);
ssize_t recv(int, void *, size_t, int);
ssize_t sendto(int, const void *, size_t, int, const struct sockaddr *, socklen_t);
ssize_t recvfrom(int, void *, size_t, int, struct sockaddr *, socklen_t *);
ssize_t sendmsg(int, const struct msghdr *, int);
ssize_t recvmsg(int, struct msghdr *, int);
int setsockopt(int, int, int, const void *, socklen_t);
int getsockopt(int, int, int, void *, socklen_t *);
int getsockname(int, struct sockaddr *, socklen_t *);
int getpeername(int, struct sockaddr *, socklen_t *);
int shutdown(int, int);
int sockatmark(int);
/* The next-record walk remains an unresolved host helper until its bounds and
 * overflow behavior are qualified. No native ancillary functions are imported. */
#define CMSG_ALIGN(n) (((n) + sizeof(size_t) - 1) & ~(sizeof(size_t) - 1))
#define CMSG_DATA(c) ((unsigned char *)(c) + CMSG_ALIGN(sizeof(struct cmsghdr)))
#define CMSG_LEN(n) (CMSG_ALIGN(sizeof(struct cmsghdr)) + (n))
#define CMSG_SPACE(n) (CMSG_ALIGN(sizeof(struct cmsghdr)) + CMSG_ALIGN(n))
#define CMSG_FIRSTHDR(m) ((m)->msg_controllen >= sizeof(struct cmsghdr) ? (struct cmsghdr *)(m)->msg_control : (struct cmsghdr *)0)
#define CMSG_NXTHDR blink_host_cmsg_nxthdr
struct cmsghdr *CMSG_NXTHDR(const struct msghdr *, const struct cmsghdr *);
#endif
