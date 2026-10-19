/* Same C program uses the real native/DotCC fcntl.h. The external test driver
   supplies an independent native lock owner; no subprocess facility is shipped. */
#include <fcntl.h>
#include <sys/file.h>
#include <unistd.h>
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL line=%d errno=%d\n", __LINE__, errno); fflush(stdout); return 1; } checks++; } while (0)
static int phase(const char *name) {
    printf("phase %s\n", name);
    fflush(stdout);
    return getchar() == '\n';
}
static int set(int fd, short type, short whence, off_t start, off_t length) {
    struct flock lock = {0};
    lock.l_type = type; lock.l_whence = whence;
    lock.l_start = start; lock.l_len = length;
    return fcntl(fd, F_SETLK, &lock);
}
int main(int argc, char **argv) {
    int checks = 0;
    if (argc != 3) return 2;
    int peer_pid = atoi(argv[2]);
    CHECK(sizeof(struct flock) == 32 && _Alignof(struct flock) == 8);
    CHECK(offsetof(struct flock, l_type) == 0 && offsetof(struct flock, l_whence) == 2);
    CHECK(offsetof(struct flock, l_start) == 8 && offsetof(struct flock, l_len) == 16 && offsetof(struct flock, l_pid) == 24);
    struct flock lock = {0};
    CHECK(sizeof lock.l_type == 2 && sizeof lock.l_whence == 2);
    CHECK(sizeof lock.l_start == 8 && sizeof lock.l_len == 8 && sizeof lock.l_pid == 4);
    CHECK((off_t)-1 < 0 && (pid_t)-1 < 0);
    CHECK(F_GETLK == 5 && F_SETLK == 6 && F_SETLKW == 7);
    CHECK(F_RDLCK == 0 && F_WRLCK == 1 && F_UNLCK == 2);
    printf("layout flock=32 align=8 offsets=0,2,8,16,24 widths=2,2,8,8,4 signed=1,1\n");
    int fd = open(argv[1], O_RDWR);
    CHECK(fd >= 0);
    CHECK(fcntl(-1, F_GETLK, &lock) == -1 && errno == EBADF);
    CHECK(fcntl(fd, F_GETLK, (void*)0) == -1 && errno == EFAULT);
    CHECK(set(fd, 3, SEEK_SET, 0, 1) == -1 && errno == EINVAL);
    CHECK(set(fd, F_WRLCK, 3, 0, 1) == -1 && errno == EINVAL);
    CHECK(set(fd, F_WRLCK, SEEK_SET, -1, 1) == -1 && errno == EINVAL);
    int ro = open(argv[1], O_RDONLY), wo = open(argv[1], O_WRONLY);
    CHECK(ro >= 0 && wo >= 0);
    CHECK(set(ro, F_WRLCK, SEEK_SET, 0, 1) == -1 && errno == EBADF);
    CHECK(set(wo, F_RDLCK, SEEK_SET, 0, 1) == -1 && errno == EBADF);
    CHECK(close(ro) == 0 && close(wo) == 0);
    CHECK(lseek(fd, 40, SEEK_SET) == 40);
    lock.l_type = F_WRLCK; lock.l_whence = SEEK_CUR;
    lock.l_start = -10; lock.l_len = 5; lock.l_pid = 12345;
    CHECK(fcntl(fd, F_GETLK, &lock) == 0 && lock.l_type == F_UNLCK);
    CHECK(lock.l_whence == SEEK_CUR && lock.l_start == -10 && lock.l_len == 5 && lock.l_pid == 12345);
    CHECK(lseek(fd, 0, SEEK_CUR) == 40);
    errno = EDOM;
    CHECK(set(fd, F_WRLCK, SEEK_SET, 32, 16) == 0 && errno == EDOM);
    CHECK(phase("owned"));
    /* GETLK deliberately ignores this process's own locks. */
    lock.l_type = F_WRLCK; lock.l_whence = SEEK_SET; lock.l_start = 32; lock.l_len = 16;
    CHECK(fcntl(fd, F_GETLK, &lock) == 0 && lock.l_type == F_UNLCK);
    CHECK(set(fd, F_UNLCK, SEEK_SET, 36, 4) == 0);
    CHECK(phase("split"));
    CHECK(set(fd, F_RDLCK, SEEK_SET, 32, 16) == 0);
    CHECK(phase("shared"));
    CHECK(set(fd, F_UNLCK, SEEK_SET, 0, 0) == 0);
    CHECK(phase("peer-lock"));
    lock.l_type = F_RDLCK; lock.l_whence = SEEK_CUR; lock.l_start = 60; lock.l_len = 1;
    CHECK(fcntl(fd, F_GETLK, &lock) == 0);
    CHECK(lock.l_type == F_WRLCK && lock.l_whence == SEEK_SET && lock.l_start == 100 && lock.l_len == 20 && lock.l_pid == peer_pid);
    CHECK(lseek(fd, 0, SEEK_CUR) == 40);
    CHECK(set(fd, F_WRLCK, SEEK_SET, 100, 1) == -1 && (errno == EACCES || errno == EAGAIN));
    CHECK(phase("blocking"));
    lock.l_type = F_WRLCK; lock.l_whence = SEEK_SET; lock.l_start = 100; lock.l_len = 20;
    CHECK(fcntl(fd, F_SETLKW, &lock) == 0);
    CHECK(phase("acquired"));
    CHECK(set(fd, F_UNLCK, SEEK_SET, 0, 0) == 0);
    lock.l_type = F_WRLCK; lock.l_whence = SEEK_CUR; lock.l_start = 10; lock.l_len = 5;
    CHECK(fcntl(fd, F_SETLK, &lock) == 0);
    CHECK(lock.l_whence == SEEK_CUR && lock.l_start == 10 && lock.l_len == 5);
    CHECK(lseek(fd, 0, SEEK_CUR) == 40);
    CHECK(phase("current"));
    CHECK(set(fd, F_UNLCK, SEEK_SET, 0, 0) == 0);
    CHECK(set(fd, F_WRLCK, SEEK_END, -20, 8) == 0);
    CHECK(phase("end"));
    CHECK(set(fd, F_UNLCK, SEEK_SET, 0, 0) == 0);
    CHECK(set(fd, F_WRLCK, SEEK_SET, 80, -10) == 0);
    CHECK(phase("negative-length"));
    CHECK(set(fd, F_UNLCK, SEEK_SET, 0, 0) == 0);
    CHECK(set(fd, F_WRLCK, SEEK_SET, 5368709123L, 0) == 0);
    CHECK(phase("large-to-eof"));
    /* Opening and closing ANY descriptor for the inode releases POSIX locks. */
    int other = open(argv[1], O_RDWR);
    CHECK(other >= 0 && close(other) == 0);
    CHECK(phase("close-other"));
    CHECK(set(fd, F_WRLCK, SEEK_SET, 16, 4) == 0);
    CHECK(close(fd) == 0);
    CHECK(phase("closed"));
    CHECK(fcntl(fd, F_GETLK, &lock) == -1 && errno == EBADF);
    fd = open(argv[1], O_RDWR);
    CHECK(fd >= 0 && set(fd, F_WRLCK, SEEK_SET, 20, 4) == 0);
    CHECK(phase("reused"));
    CHECK(close(fd) == 0);
    CHECK(phase("record-done"));
    fd = open(argv[1], O_RDWR); other = open(argv[1], O_RDWR);
    CHECK(fd >= 0 && other >= 0);
    CHECK(flock(fd, LOCK_EX | LOCK_NB) == 0);
    CHECK(phase("flock-owned"));
    CHECK(close(other) == 0);
    CHECK(phase("flock-close-other"));
    CHECK(flock(fd, LOCK_SH | LOCK_NB) == 0);
    CHECK(phase("flock-shared"));
    CHECK(flock(fd, LOCK_UN) == 0);
    CHECK(phase("flock-peer"));
    CHECK(flock(fd, LOCK_EX | LOCK_NB) == -1 && (errno == EWOULDBLOCK || errno == EAGAIN));
    CHECK(phase("flock-blocking"));
    CHECK(flock(fd, LOCK_EX) == 0);
    CHECK(phase("flock-acquired"));
    CHECK(close(fd) == 0);
    CHECK(phase("flock-closed"));
    printf("record-locks passed checks=%d\n", checks);
    return 0;
}
