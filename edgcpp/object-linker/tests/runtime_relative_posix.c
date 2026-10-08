#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#include <dirent.h>
#include <sys/stat.h>

#define CHECK(e) do { if (!(e)) { puts("relative POSIX check failed; exit status identifies source line"); return __LINE__; } } while (0)

int main(void) {
    CHECK(mkdir("relative-parent", 0700) == 0);
    int parent = open("relative-parent", O_RDONLY | O_DIRECTORY);
    CHECK(parent >= 0);
    CHECK(rename("relative-parent", "relative-renamed") == 0);
    CHECK(mkdir("relative-parent", 0700) == 0);
    int child = openat(parent, "entry", O_CREAT | O_EXCL | O_RDWR, 0600);
    CHECK(child >= 0 && write(child, "hello", 5) == 5);
    struct stat first, second;
    CHECK(stat("relative-renamed/entry", &first) == 0 && first.st_size == 5);
    CHECK(stat("relative-parent/entry", &second) == -1 && errno == ENOENT);
    CHECK(openat(child, "anything", O_RDONLY) == -1 && errno == ENOTDIR);
    CHECK(openat(-777, "anything", O_RDONLY) == -1 && errno == EBADF);
    CHECK(fdopendir(child) == 0 && errno == ENOTDIR && fcntl(child, F_GETFD) >= 0);

    char absolute[4096];
    CHECK(getcwd(absolute, sizeof absolute) != 0);
    strcat(absolute, "/relative-renamed/entry");
    int ignore_directory = openat(-777, absolute, O_RDONLY);
    CHECK(ignore_directory >= 0 && close(ignore_directory) == 0);
    CHECK(pathconf("relative-renamed", _PC_PATH_MAX) >= 256);

    CHECK(link("relative-renamed/entry", "relative-hard") == 0);
    CHECK(stat("relative-hard", &second) == 0 && first.st_ino == second.st_ino && first.st_dev == second.st_dev);
    CHECK(truncate("relative-hard", 2) == 0 && fstat(child, &first) == 0 && first.st_size == 2);
    CHECK(truncate("relative-hard", -1) == -1 && errno == EINVAL);
    CHECK(symlink("relative-renamed/entry", "relative-symbolic") == 0);
    CHECK(lstat("relative-symbolic", &second) == 0 && S_ISLNK(second.st_mode));
    char target[128] = {0};
    CHECK(readlink("relative-symbolic", target, sizeof target) == 22 && strcmp(target, "relative-renamed/entry") == 0);
    CHECK(openat(parent, "../relative-symbolic", O_RDONLY | O_NOFOLLOW) == -1 && errno == ELOOP);
    CHECK(symlink("relative-renamed", "relative-dir-symbolic") == 0);
    CHECK(lstat("relative-dir-symbolic", &second) == 0 && S_ISLNK(second.st_mode));
    CHECK(stat("relative-dir-symbolic", &second) == 0 && S_ISDIR(second.st_mode));
    CHECK(unlink("relative-dir-symbolic") == 0);

    CHECK(mkdir("relative-renamed/subdir", 0700) == 0);
    DIR* stream = fdopendir(parent);
    CHECK(stream != 0);
    // fdopendir takes ownership but the fd remains usable for recursive *at calls.
    int from_directory_stream = openat(parent, "entry", O_RDONLY);
    CHECK(from_directory_stream >= 0 && close(from_directory_stream) == 0);
    int dots = 0, dotdots = 0, entries = 0, subdirs = 0;
    errno = EDOM;
    struct dirent* entry;
    while ((entry = readdir(stream)) != 0) {
        if (!strcmp(entry->d_name, ".")) ++dots;
        else if (!strcmp(entry->d_name, "..")) ++dotdots;
        else if (!strcmp(entry->d_name, "entry")) ++entries;
        else if (!strcmp(entry->d_name, "subdir")) ++subdirs;
        else return 2;
    }
    CHECK(errno == EDOM && dots == 1 && dotdots == 1 && entries == 1 && subdirs == 1);
    CHECK(unlinkat(parent, "subdir", 0) == -1 && (errno == EISDIR || errno == EPERM));
    CHECK(unlinkat(parent, "subdir", AT_REMOVEDIR) == 0);
    CHECK(unlinkat(parent, "entry", 12345) == -1 && errno == EINVAL);
    CHECK(unlinkat(parent, "entry", 0) == 0);
    CHECK(stat("relative-renamed/entry", &first) == -1 && errno == ENOENT);
    CHECK(closedir(stream) == 0);
    CHECK(fcntl(parent, F_GETFD) == -1 && errno == EBADF);
    CHECK(unlink("relative-symbolic") == 0 && unlink("relative-hard") == 0);
    char retained[2];
    CHECK(pread(child, retained, 2, 0) == 2 && retained[0] == 'h' && retained[1] == 'e');
    CHECK(close(child) == 0 && rmdir("relative-renamed") == 0 && rmdir("relative-parent") == 0);
    puts("relative POSIX handles, links, ownership OK");
    return 0;
}
