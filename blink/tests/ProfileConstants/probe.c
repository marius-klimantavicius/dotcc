#ifdef BLINK_PROFILE_CONSTANTS
#include "host-profile-constants.h"
#else
#define _GNU_SOURCE
#include <fcntl.h>
#include <sys/stat.h>
#include <netinet/in.h>
#include <time.h>
#endif
#include <stdio.h>
#define CHECK(name, value, type) \
  _Static_assert((name) == (value), #name " value"); \
  _Static_assert(_Generic((name), type: 1, default: 0), #name " type")
CHECK(AT_SYMLINK_FOLLOW, 1024, int);
CHECK(UTIME_NOW, 1073741823L, long);
CHECK(UTIME_OMIT, 1073741822L, long);
CHECK(IPPROTO_RAW, 255, int);
CHECK(IPPROTO_IPV6, 41, int);
CHECK(CLOCK_PROCESS_CPUTIME_ID, 2, int);
CHECK(CLOCK_THREAD_CPUTIME_ID, 3, int);
int main(void) {
  puts("profile constants: seven values and C types agree");
  return 0;
}
