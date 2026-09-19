#ifndef BLINK_CAMPAIGN_HOST_MESSAGES_H
#define BLINK_CAMPAIGN_HOST_MESSAGES_H
#include <sys/socket.h>
/* The authored socket profile supplies these typed callback aliases.
 * IPv4/TCP only: no control messages or per-call message flags are supported. */
int getpeername(int, struct sockaddr *, socklen_t *);
ssize_t sendmsg(int, const struct msghdr *, int);
ssize_t recvmsg(int, struct msghdr *, int);
#endif
