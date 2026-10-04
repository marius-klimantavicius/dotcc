/* Concurrent managed callers exercise the same owner heap and libc TLS used
   by THREADSAFE SQLite, without relying on SQLite to serialize this fixture. */
#include <errno.h>
#include <stdlib.h>

int owner_concurrency(int marker, int rounds, int (*rendezvous)(int)) {
  int iteration;
  errno = marker;
  if (rendezvous(marker) != marker || errno != marker) return 1;
  for (iteration = 0; iteration < rounds; ++iteration) {
    unsigned long length = 17 + (iteration % 251);
    unsigned long i;
    unsigned char *bytes = (unsigned char *)malloc(length);
    unsigned char *larger;
    if (!bytes) return 2;
    for (i = 0; i < length; ++i) bytes[i] = (unsigned char)(marker + i);
    larger = (unsigned char *)realloc(bytes, length + 41);
    if (!larger) { free(bytes); return 3; }
    for (i = 0; i < length; ++i) {
      if (larger[i] != (unsigned char)(marker + i)) { free(larger); return 4; }
    }
    free(larger);
    bytes = (unsigned char *)calloc(length, 3);
    if (!bytes) return 5;
    for (i = 0; i < length * 3; ++i) {
      if (bytes[i] != 0) { free(bytes); return 6; }
    }
    free(bytes);
    if (errno != marker) return 7;
  }
  return 0;
}

int owner_wait(int (*wait)(int)) { return wait(17); }
int owner_errno(void) { return errno; }
void owner_set_errno(int value) { errno = value; }
