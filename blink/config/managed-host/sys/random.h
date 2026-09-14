#ifndef BLINK_MANAGED_HOST_RANDOM_H
#define BLINK_MANAGED_HOST_RANDOM_H
#include <stddef.h>
#include <sys/types.h>
#define GRND_NONBLOCK 1
#define GRND_RANDOM 2
#define getrandom blink_host_getrandom
#define getentropy blink_host_getentropy
ssize_t getrandom(void *, size_t, unsigned int);
int getentropy(void *, size_t);
#endif
