/* Same C source, system-header native oracle and actual emitted target ABI.
   Fixture files/links are created by the owner before either execution. */
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>

int main(int argc, char **argv) {
    if (argc != 6) return 2;
    unsigned int uid = (unsigned int)atoi(argv[3]);
    unsigned int gid = (unsigned int)atoi(argv[4]);
    struct stat path, descriptor, link, followed;
    if (stat(argv[1], &path) || lstat(argv[2], &link) || stat(argv[2], &followed)) return 3;
    if (!S_ISREG(path.st_mode) || (path.st_mode & 0777) != 0640 ||
        path.st_uid != uid || path.st_gid != gid || path.st_size != 8 ||
        path.st_nlink != 1 || path.st_ino == 0 || path.st_dev == 0 ||
        path.st_blksize <= 0 || path.st_blocks <= 0 || path.st_mtime <= 0 ||
        !S_ISLNK(link.st_mode) || followed.st_ino != path.st_ino ||
        followed.st_dev != path.st_dev || link.st_ino == path.st_ino) return 4;
    int fd = open(argv[1], O_RDONLY);
    if (fd < 0 || fstat(fd, &descriptor) || descriptor.st_mode != path.st_mode ||
        descriptor.st_uid != uid || descriptor.st_gid != gid || descriptor.st_ino != path.st_ino ||
        descriptor.st_dev != path.st_dev || descriptor.st_size != 8) return 5;
    if (rename(argv[1], argv[5]) || unlink(argv[5]) || fstat(fd, &descriptor) ||
        descriptor.st_ino != path.st_ino || descriptor.st_nlink != 0 || descriptor.st_size != 8) return 6;
    if (close(fd)) return 7;
    errno = 0;
    if (stat(argv[2], &followed) != -1 || errno != ENOENT || lstat(argv[2], &link) || !S_ISLNK(link.st_mode)) return 8;
    puts("emitted stat metadata 1");
    return 0;
}
