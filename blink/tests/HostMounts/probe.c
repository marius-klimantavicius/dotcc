#define _POSIX_C_SOURCE 200809L
#include <fcntl.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
int main(void) {
  int fd = open("normal", O_CREAT | O_EXCL | O_RDWR, 0600); CHECK(fd >= 0);
  CHECK(write(fd, "abcdef", 6) == 6);
  int copy = dup(fd); CHECK(copy >= 0);
  CHECK(lseek(fd, 2, SEEK_SET) == 2); char bytes[2];
  CHECK(read(copy, bytes, 2) == 2 && !memcmp(bytes, "cd", 2)); CHECK(lseek(fd, 0, SEEK_CUR) == 4);
  CHECK(pwrite(fd, "XY", 2, 0) == 2); CHECK(fcntl(copy, F_SETFL, O_APPEND) == 0);
  CHECK(write(fd, "!", 1) == 1); CHECK(rename("normal", "renamed") == 0); CHECK(unlink("renamed") == 0);
  CHECK(pread(fd, bytes, 2, 0) == 2 && !memcmp(bytes, "XY", 2)); CHECK(close(copy) == 0 && close(fd) == 0);
  puts("normal file descriptions: PASS"); return 0;
}
