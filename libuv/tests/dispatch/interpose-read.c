#define _GNU_SOURCE
#include <sys/syscall.h>
#include <unistd.h>

/* Real interposition makes read's provider differ from malloc/free/errno. */
ssize_t read(int descriptor, void* buffer, size_t count) {
  return syscall(SYS_read, descriptor, buffer, count);
}
