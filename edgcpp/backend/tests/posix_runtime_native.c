#define _POSIX_C_SOURCE 200809L
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <errno.h>
#include <string.h>
int main(int argc, char **argv) {
    if (argc != 3) return 2;
    int fd = open(argv[1], O_RDWR);
    if (fd < 0) return 3;
    struct flock lock = {.l_type=F_WRLCK,.l_whence=SEEK_SET,.l_start=40,.l_len=20};
    int result = fcntl(fd, F_SETLK, &lock);
    if (result == 0 && strcmp(argv[2], "hold") == 0) {
        puts("held"); fflush(stdout); (void)getchar();
    }
    else if (result == 0) puts("free");
    else if (errno == EACCES || errno == EAGAIN) puts("busy");
    else return 4;
    close(fd);
    return 0;
}
