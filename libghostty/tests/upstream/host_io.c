/* Read-only test fixture service. Terminal parsing/state/assertions stay in Zig.
 * The upstream test runner enters this service sequentially. Directory tokens
 * retain their path/iterator; files use validated ordinary host descriptors. */
#define _POSIX_C_SOURCE 200809L
#define _FILE_OFFSET_BITS 64
#include <dirent.h>
#include <errno.h>
#include <fcntl.h>
#include <limits.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <sys/random.h>
#include <unistd.h>

struct HostDirectory {
    int token;
    DIR *stream;
    char *path;
    char *pending;
    int pending_kind;
    struct HostDirectory *next;
};
struct HostFile { int fd; struct HostFile *next; };
static struct HostDirectory *directories;
static struct HostFile *files;
static int next_directory = 1;

static int io_error(void) { return errno > 0 ? -errno : -EIO; }
static struct HostDirectory *find_directory(int token) {
    struct HostDirectory *item = directories;
    while (item && item->token != token) item = item->next;
    return item;
}
static int known_file(int fd) {
    struct HostFile *item = files;
    while (item && item->fd != fd) item = item->next;
    return item != NULL;
}
static int joined_path(int base, const uint8_t *path, size_t len, char **out) {
    if (!path || !out || memchr(path, 0, len)) return -EINVAL;
    if (!len) return -ENOENT;
    const char *prefix = "";
    if (path[0] != '/' && base != -100) {
        struct HostDirectory *directory = find_directory(base);
        if (!directory) return -EBADF;
        prefix = directory->path;
    }
    size_t before = strlen(prefix);
    if (len > SIZE_MAX - 2 || before > SIZE_MAX - len - 2) return -ENAMETOOLONG;
    char *result = malloc(before + len + 2);
    if (!result) return -ENOMEM;
    if (before) { memcpy(result, prefix, before); result[before++] = '/'; }
    memcpy(result + before, path, len);
    result[before + len] = 0;
    *out = result;
    return 0;
}

int dotcc_upstream_dir_open(int base, const uint8_t *path, size_t len, int *out) {
    if (!out) return -EINVAL;
    char *full = NULL;
    int status = joined_path(base, path, len, &full);
    if (status) return status;
    DIR *stream = opendir(full);
    if (!stream) { status = io_error(); free(full); return status; }
    if (next_directory == INT_MAX) { closedir(stream); free(full); return -EMFILE; }
    struct HostDirectory *directory = calloc(1, sizeof(*directory));
    if (!directory) { closedir(stream); free(full); return -ENOMEM; }
    directory->token = next_directory++;
    directory->stream = stream;
    directory->path = full;
    directory->next = directories;
    directories = directory;
    *out = directory->token;
    return 0;
}

/* Short buffers retain the pending entry so a correctly sized retry loses no
 * fixture. Entry names are byte slices, without an extra terminating byte. */
int dotcc_upstream_dir_next(int token, uint8_t *name, size_t capacity,
                            size_t *length, int *kind) {
    if (!length || !kind || (!name && capacity)) return -EINVAL;
    struct HostDirectory *directory = find_directory(token);
    if (!directory) return -EBADF;
    while (!directory->pending) {
        errno = 0;
        struct dirent *entry = readdir(directory->stream);
        if (!entry) {
            if (errno) return io_error();
            *length = 0; *kind = 0;
            return 1;
        }
        if (!strcmp(entry->d_name, ".") || !strcmp(entry->d_name, "..")) continue;
        size_t len = strlen(entry->d_name);
        char *full = NULL;
        int status = joined_path(token, (const uint8_t *)entry->d_name, len, &full);
        if (status) return status;
        struct stat info;
        status = stat(full, &info);
        int failure = status ? io_error() : 0;
        free(full);
        if (failure) return failure;
        directory->pending = malloc(len + 1);
        if (!directory->pending) return -ENOMEM;
        memcpy(directory->pending, entry->d_name, len + 1);
        directory->pending_kind = S_ISREG(info.st_mode) ? 1 : S_ISDIR(info.st_mode) ? 2 : 0;
    }
    size_t len = strlen(directory->pending);
    *length = len;
    if (capacity < len) return -ENOBUFS;
    if (len) memcpy(name, directory->pending, len);
    *kind = directory->pending_kind;
    free(directory->pending);
    directory->pending = NULL;
    return 0;
}

int dotcc_upstream_file_open(int base, const uint8_t *path, size_t len, int *out) {
    if (!out) return -EINVAL;
    char *full = NULL;
    int status = joined_path(base, path, len, &full);
    if (status) return status;
    int fd = open(full, O_RDONLY);
    status = fd < 0 ? io_error() : 0;
    free(full);
    if (status) return status;
    struct stat info;
    if (fstat(fd, &info)) { status = io_error(); close(fd); return status; }
    if (!S_ISREG(info.st_mode)) { close(fd); return S_ISDIR(info.st_mode) ? -EISDIR : -EINVAL; }
    struct HostFile *item = malloc(sizeof(*item));
    if (!item) { close(fd); return -ENOMEM; }
    item->fd = fd; item->next = files; files = item;
    *out = fd;
    return 0;
}

int dotcc_upstream_file_size(int fd, uint64_t *out) {
    if (!out) return -EINVAL;
    if (!known_file(fd)) return -EBADF;
    struct stat info;
    if (fstat(fd, &info)) return io_error();
    if (info.st_size < 0) return -EIO;
    *out = (uint64_t)info.st_size;
    return 0;
}

int dotcc_upstream_file_read_at(int fd, uint64_t offset, uint8_t *buffer,
                                size_t capacity, size_t *read_count) {
    if (!read_count || (!buffer && capacity) || offset > INT64_MAX || capacity > INT64_MAX) return -EINVAL;
    if (!known_file(fd)) return -EBADF;
    ssize_t count;
    do { count = pread(fd, buffer, capacity, (off_t)offset); } while (count < 0 && errno == EINTR);
    if (count < 0) return io_error();
    *read_count = (size_t)count;
    return 0;
}

void dotcc_upstream_dir_close(int token) {
    struct HostDirectory **slot = &directories;
    while (*slot && (*slot)->token != token) slot = &(*slot)->next;
    if (!*slot) return;
    struct HostDirectory *item = *slot;
    *slot = item->next;
    closedir(item->stream);
    free(item->pending); free(item->path); free(item);
}
void dotcc_upstream_file_close(int fd) {
    struct HostFile **slot = &files;
    while (*slot && (*slot)->fd != fd) slot = &(*slot)->next;
    if (!*slot) return;
    struct HostFile *item = *slot;
    *slot = item->next;
    close(fd); free(item);
}
size_t dotcc_upstream_io_live(void) {
    size_t count = 0;
    struct HostDirectory *directory = directories;
    struct HostFile *file = files;
    while (directory) { count++; directory = directory->next; }
    while (file) { count++; file = file->next; }
    return count;
}

/* This is a provider operation, never a deterministic test RNG. Both the
 * native OS and translated BCL path supply cryptographic random bytes. */
int dotcc_upstream_random_secure(uint8_t *buffer, size_t length) {
    if ((!buffer && length) || length > INT64_MAX) return -EINVAL;
    size_t done = 0;
    while (done < length) {
        ssize_t count = getrandom(buffer + done, length - done, 0);
        if (count < 0) { if (errno == EINTR) continue; return io_error(); }
        if (count == 0) return -EIO;
        done += (size_t)count;
    }
    return 0;
}
