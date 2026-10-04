#define _DEFAULT_SOURCE 1
#define _POSIX_C_SOURCE 200809L
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#include <dirent.h>
#include <sys/stat.h>
#include <sys/resource.h>
#include <sys/mman.h>
#define CHECK(value) do { if (!(value)) { printf("POSIX stdio failure at line %d errno %d\n", __LINE__, errno); return 1; } } while (0)

int main(void)
{
    char cwd[4096], next[4096], temporary[] = "compiler-XXXXXX";
    struct stat status;
    struct rlimit limits;
    CHECK(getcwd(cwd, sizeof(cwd)) != NULL);
    CHECK(mkdir("compiler-input", 0700) == 0 && chdir("compiler-input") == 0);
    CHECK(getcwd(next, sizeof(next)) != NULL && strcmp(cwd, next) != 0);
    int fd = mkstemp(temporary);
    CHECK(fd >= 0);
    FILE *file = fdopen(fd, "w+");
    CHECK(file != NULL && fileno(file) == fd);
    CHECK(fwrite("ABC", 1, 3, file) == 3 && fflush(file) == 0);
    CHECK(fstat(fd, &status) == 0 && status.st_size == 3 && S_ISREG(status.st_mode));
    CHECK(lseek(fd, 0, SEEK_CUR) == 3);
    CHECK(fseek(file, 0, SEEK_SET) == 0 && fgetc(file) == 'A');
    unsigned char *mapped = (unsigned char *)mmap(NULL, 3, PROT_READ, MAP_PRIVATE, fd, 0);
    CHECK(mapped != MAP_FAILED && mapped[2] == 'C' && munmap(mapped, 3) == 0);
    CHECK(isatty(fd) == 0 && errno == ENOTTY);
    CHECK(fclose(file) == 0 && fstat(fd, &status) == -1 && errno == EBADF);
    file = fopen(temporary, "r");
    CHECK(file != NULL && fstat(fileno(file), &status) == 0 && status.st_size == 3);
    CHECK(fclose(file) == 0);
    DIR *directory = opendir(".");
    CHECK(directory != NULL);
    struct dirent *entry;
    int found = 0, dots = 0;
    errno = 0;
    while ((entry = readdir(directory)) != NULL) {
        if (strcmp(entry->d_name, temporary) == 0) found++;
        if (strcmp(entry->d_name, ".") == 0 || strcmp(entry->d_name, "..") == 0) dots++;
    }
    CHECK(errno == 0 && found == 1 && dots == 2 && closedir(directory) == 0);
    CHECK(getrlimit(RLIMIT_CPU, &limits) == 0 && limits.rlim_cur <= limits.rlim_max);
    CHECK(setrlimit(RLIMIT_CPU, &limits) == 0);
    CHECK(unlink(temporary) == 0 && chdir(cwd) == 0 && rmdir("compiler-input") == 0);
    printf("POSIX translated stdio host services passed\n");
    return 0;
}
