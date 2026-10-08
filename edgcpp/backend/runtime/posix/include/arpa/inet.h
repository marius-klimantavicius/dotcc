#ifndef EDG_POSIX_ARPA_INET_H
#define EDG_POSIX_ARPA_INET_H
#include <netinet/in.h>
#ifdef __cplusplus
extern "C" {
#endif
in_addr_t inet_addr(const char *);
int inet_aton(const char *, struct in_addr *);
int inet_pton(int, const char *, void *);
#ifdef __cplusplus
}
#endif
#endif
