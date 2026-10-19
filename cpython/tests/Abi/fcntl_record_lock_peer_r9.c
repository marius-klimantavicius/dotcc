/* Independent Linux process for shared-service tests only. Not product code. */
#include <fcntl.h>
#include <sys/file.h>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>

int main(int argc, char **argv) {
    if (argc != 2) return 2;
    int fd = open(argv[1], O_RDWR);
    if (fd < 0) return 3;
    printf("ready %d\n", (int)getpid()); fflush(stdout);
    char command[16]; int type, whence; long start, length;
    while (scanf("%15s %d %d %ld %ld", command, &type, &whence, &start, &length) == 5) {
        struct flock lock = {0};
        lock.l_type = type; lock.l_whence = whence; lock.l_start = start; lock.l_len = length;
        errno = 0;
        int result = command[0] == 'f' ? flock(fd, type) :
            fcntl(fd, command[0] == 'g' ? F_GETLK : F_SETLK, &lock);
        printf("result %d %d %d %d %ld %ld %d\n", result, errno,
            lock.l_type, lock.l_whence, (long)lock.l_start, (long)lock.l_len, (int)lock.l_pid);
        fflush(stdout);
    }
    return close(fd) != 0;
}
