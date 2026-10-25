#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>
#include <unistd.h>

#define PAGE 4096
#define CHECK(x) do { if (!(x)) { result = __LINE__; goto Cleanup; } } while (0)

/* Ordinary Linux mapping workload. Never read/write the whole absent pages:
 * replace them with anonymous BSS or a real file page before accessing them. */
int main(void) {
  unsigned char contents[PAGE + 37];
  unsigned char *mapping = MAP_FAILED;
  int result = 0, fd = -1;
  for (unsigned i = 0; i < sizeof(contents); ++i) contents[i] = i % 251;
  fd = open("mapping-data", O_CREAT | O_TRUNC | O_RDWR, 0600);
  CHECK(fd >= 0);
  CHECK(write(fd, contents, sizeof(contents)) == sizeof(contents));
  CHECK(lseek(fd, 19, SEEK_SET) == 19);
  mapping = mmap(0, PAGE * 4, PROT_READ, MAP_PRIVATE, fd, 0);
  CHECK(mapping != MAP_FAILED);
  for (unsigned i = 0; i < sizeof(contents); ++i) CHECK(mapping[i] == contents[i]);
  for (unsigned i = sizeof(contents); i < PAGE * 2; ++i) CHECK(mapping[i] == 0);
  CHECK(lseek(fd, 0, SEEK_CUR) == 19);
  CHECK(!mprotect(mapping, PAGE * 4, PROT_NONE));
  CHECK(!mprotect(mapping, PAGE * 4, PROT_READ | PROT_WRITE));
  mapping[PAGE + 10] = 253;
  CHECK(mapping[PAGE + 10] == 253);
  unsigned char byte;
  CHECK(pread(fd, &byte, 1, PAGE + 10) == 1 && byte == contents[PAGE + 10]);
  CHECK(mmap(mapping + PAGE * 2, PAGE, PROT_READ | PROT_WRITE,
             MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED, -1, 0) == mapping + PAGE * 2);
  for (unsigned i = PAGE * 2; i < PAGE * 3; ++i) CHECK(!mapping[i]);
  mapping[PAGE * 2 + 91] = 77;
  CHECK(!madvise(mapping + PAGE * 2, PAGE, MADV_DONTNEED));
  CHECK(!mapping[PAGE * 2 + 91] && mapping[PAGE + 10] == 253);
  CHECK(mmap(mapping + PAGE * 3, PAGE, PROT_READ, MAP_PRIVATE | MAP_FIXED,
             fd, 0) == mapping + PAGE * 3);
  for (unsigned i = 0; i < PAGE; ++i) CHECK(mapping[PAGE * 3 + i] == contents[i]);
  CHECK(!munmap(mapping + PAGE * 2, PAGE));
  CHECK(mmap(mapping + PAGE * 2, PAGE, PROT_READ | PROT_WRITE,
             MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED_NOREPLACE, -1, 0) == mapping + PAGE * 2);
  mapping[PAGE * 2 + 91] = 78;
  CHECK(mmap(mapping + PAGE * 2, PAGE, PROT_READ | PROT_WRITE,
             MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED_NOREPLACE, -1, 0) == MAP_FAILED && errno == EEXIST);
  CHECK(mapping[PAGE * 2 + 91] == 78 && mapping[PAGE + 10] == 253);
  CHECK(!close(fd)); fd = -1;
  CHECK(mapping[123] == contents[123] && mapping[PAGE * 3 + 123] == contents[123]);
Cleanup:
  if (mapping != MAP_FAILED && munmap(mapping, PAGE * 4) && !result) result = __LINE__;
  if (fd >= 0 && close(fd) && !result) result = __LINE__;
  if (unlink("mapping-data") && !result) result = __LINE__;
  if (result) fprintf(stderr, "mapping workload failed at line %d\n", result);
  else puts("guest-private-mapping OK bytes tails protection bss fixed noreplace discard close");
  return !!result;
}
