#ifndef BLINK_HOST_LOCKS_H
#define BLINK_HOST_LOCKS_H
#include <fcntl.h>
#include <sys/file.h>
/* Preserve the native-checked struct blink_host_flock tag while routing calls
 * to a distinct CLR method. Bare flock designators are not in this profile. */
int blink_io_flock(int, int);
#define blink_host_flock(...) blink_io_flock(__VA_ARGS__)
#endif
