#include <fcntl.h>
#include <sys/file.h>
#include <unistd.h>
#include <stdio.h>
#include <stddef.h>
#include <errno.h>

#define CHECK(x) do { if (!(x)) { printf("record-lock failure line=%d errno=%d\n", __LINE__, errno); return 1; } checks++; } while (0)
int main(void) {
    int checks = 0;
    const char *name = "dotcc-cpython-r9-record-lock-fixture.bin";
    struct flock lock = {0};
    CHECK(sizeof lock == 32 && _Alignof(struct flock) == 8);
    CHECK(offsetof(struct flock, l_type) == 0 && offsetof(struct flock, l_whence) == 2);
    CHECK(offsetof(struct flock, l_start) == 8 && offsetof(struct flock, l_len) == 16 && offsetof(struct flock, l_pid) == 24);
    CHECK(sizeof lock.l_start == 8 && sizeof lock.l_pid == 4 && (off_t)-1 < 0);
    int fd = open(name, O_CREAT | O_EXCL | O_RDWR, 0600);
    CHECK(fd >= 0);
    int other = open(name, O_RDWR);
    CHECK(other >= 0);
    CHECK(fcntl(fd, F_GETLK, (void*)0) == -1 && errno == EFAULT);
    lock.l_type = F_WRLCK; lock.l_start = 5368709123L; lock.l_len = 9;
    CHECK(fcntl(fd, F_SETLK, &lock) == 0);
    CHECK(lock.l_start == 5368709123L && lock.l_len == 9);
    CHECK(fcntl(other, F_GETLK, &lock) == 0 && lock.l_type == F_UNLCK);
    CHECK(flock(fd, LOCK_EX | LOCK_NB) == 0);
    CHECK(flock(other, LOCK_EX | LOCK_NB) == -1 && errno == EAGAIN);
    CHECK(flock(fd, LOCK_UN) == 0 && flock(other, LOCK_EX | LOCK_NB) == 0);
    CHECK(flock(other, LOCK_UN) == 0 && flock(fd, LOCK_SH | LOCK_NB) == 0);
    CHECK(flock(other, LOCK_SH | LOCK_NB) == 0);
    CHECK(close(fd) == 0 && close(other) == 0);
    CHECK(remove(name) == 0);
    printf("record-lock fixture passed checks=%d\n", checks);
    return 0;
}
