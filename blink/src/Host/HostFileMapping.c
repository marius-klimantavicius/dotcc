#include "HostFileMapping.h"

ssize_t blink_io_pread(int, void *, size_t, off_t);
ssize_t blink_io_mapping_pread(int, void *, size_t, off_t);
int blink_io_mapping_read_at_length(int, off_t *);
int blink_io_mapping_begin(int);
int blink_io_mapping_end(void);
int blink_io_shared_file_page(int, off_t, void **, uint64_t *);
int blink_io_shared_file_release(uint64_t);
int blink_io_shared_file_available(uint64_t);
int blink_io_shared_file_readonly(int);
int blink_io_memory_file_budget(size_t, size_t, uint64_t *);
int blink_io_memory_file_account(uint64_t, size_t);
int blink_io_memory_file_budget_release(uint64_t);

/* Actual C definitions keep function-pointer identity available in translated
 * C storage. The fixed callbacks borrow the current instance synchronously. */
ssize_t pread(int fd, void *destination, size_t length, off_t offset) {
  return blink_io_pread(fd, destination, length, offset);
}
static ssize_t ReadAt(int fd, void *destination, size_t length, off_t offset) {
  return blink_io_mapping_pread(fd, destination, length, offset);
}
static int ReadLength(int fd, off_t *length) {
  return blink_io_mapping_read_at_length(fd, length);
}
int BlinkHostFileMappingBegin(int fd) { return blink_io_mapping_begin(fd); }
int BlinkHostFileMappingEnd(void) { return blink_io_mapping_end(); }
static int AcquireShared(int fd, off_t offset, void **address, uint64_t *lease) {
  return blink_io_shared_file_page(fd, offset, address, lease);
}
static int ReleaseShared(uint64_t lease) {
  return blink_io_shared_file_release(lease);
}
static int SharedAvailable(uint64_t lease) {
  return blink_io_shared_file_available(lease);
}
static int SharedReadOnly(int fd) {
  return blink_io_shared_file_readonly(fd);
}
static int BindBudget(size_t limit, size_t bytes, uint64_t *token) {
  return blink_io_memory_file_budget(limit, bytes, token);
}
static int AccountBudget(uint64_t token, size_t bytes) {
  return blink_io_memory_file_account(token, bytes);
}
static int ReleaseBudget(uint64_t token) {
  return blink_io_memory_file_budget_release(token);
}
int BlinkHostMemoryEnablePrivateFiles(void) {
  if (BlinkHostMemorySetFileReader(ReadAt, ReadLength)) return -1;
  if (BlinkHostMemorySetSharedFiles(AcquireShared, ReleaseShared, SharedAvailable, SharedReadOnly)) return -1;
  return BlinkHostMemorySetFileBudget(BindBudget, AccountBudget, ReleaseBudget);
}
