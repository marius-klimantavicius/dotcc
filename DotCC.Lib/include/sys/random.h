#ifndef _DOTCC_SYS_RANDOM_H
#define _DOTCC_SYS_RANDOM_H
#include <sys/types.h>
#define GRND_NONBLOCK 0x01
#define GRND_RANDOM 0x02
#define GRND_INSECURE 0x04
/* Linux API declarations; these do not substitute a deterministic RNG. */
ssize_t getrandom(void *buffer, size_t length, unsigned int flags);
int getentropy(void *buffer, size_t length);
#endif
