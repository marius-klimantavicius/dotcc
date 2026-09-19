#include "HostFileMapping.h"

ssize_t blink_io_pread(int, void *, size_t, off_t);
int blink_io_read_at_length(int, off_t *);

/* Actual C definitions keep function-pointer identity available in translated
 * C storage. The fixed callbacks borrow the current instance synchronously. */
ssize_t pread(int fd, void *destination, size_t length, off_t offset) {
  return blink_io_pread(fd, destination, length, offset);
}
static int ReadLength(int fd, off_t *length) {
  return blink_io_read_at_length(fd, length);
}
int BlinkHostMemoryEnablePrivateFiles(void) {
  return BlinkHostMemorySetFileReader(pread, ReadLength);
}
