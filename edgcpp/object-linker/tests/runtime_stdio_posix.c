#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/stat.h>
#include <sys/time.h>

/* This initializer must refer to the provider's cell, not a private copy. */
static FILE **standard_output = &stdout;

int main(void) {
    char payload[] = "shared FILE and descriptor state";
    char buffer[64] = {0};
    if (standard_output != &stdout || *standard_output != stdout || !stdin || !stderr)
        return 1;
    FILE *file = tmpfile();
    if (!file) return 2;
    FILE *saved = stdout;
    stdout = file;
    if (*standard_output != file) return 3;
    if (fputs(payload, stdout) < 0) return 4;
    stdout = saved;
    if (fflush(file) || ftell(file) != (long)strlen(payload)) return 5;
    fpos_t end;
    if (fgetpos(file, &end) || fseek(file, 0, SEEK_SET)) return 6;
    if (fread(buffer, 1, sizeof buffer, file) != strlen(payload)) return 7;
    if (strcmp(buffer, payload) || !feof(file)) return 8;
    clearerr(file);
    if (feof(file) || fsetpos(file, &end)) return 9;
    if (ftell(file) != (long)strlen(payload) || fclose(file)) return 10;

    /* Fixed local name is isolated by the verifier's per-process working dir. */
    const char *path = "dotobj-runtime-file.tmp";
    int fd = open(path, O_CREAT | O_EXCL | O_RDWR, 0600);
    if (fd < 0) return 11;
    if (write(fd, payload, strlen(payload)) != (long)strlen(payload)) return 12;
    struct stat info;
    if (fstat(fd, &info) || info.st_size != (long)strlen(payload)) return 13;
    if (lseek(fd, 0, SEEK_SET) != 0) return 14;
    memset(buffer, 0, sizeof buffer);
    if (read(fd, buffer, sizeof buffer) != (long)strlen(payload) || strcmp(buffer, payload)) return 15;
    FILE *stream = fdopen(fd, "r+");
    if (!stream || fileno(stream) != fd || fseek(stream, 0, SEEK_SET)) return 16;
    if (fputc('S', stream) != 'S' || fflush(stream) || fclose(stream)) return 17;
    if (stat(path, &info) || info.st_size != (long)strlen(payload)) return 18;
    struct timeval timestamps[2] = {{1700000000, 123456}, {1700001000, 654321}};
    if (utimes(path, timestamps) || stat(path, &info)) return 21;
    if (info.st_atim.tv_sec != 1700000000 || info.st_atim.tv_nsec != 123456000 ||
        info.st_mtime != 1700001000 || info.st_mtim.tv_nsec != 654321000) return 22;
    info.st_mtime = 7;
    if (info.st_mtim.tv_sec != 7 || unlink(path)) return 23;
    errno = 0;
    if (open(path, O_RDONLY) != -1 || errno != ENOENT) return 19;
    if (sysconf(_SC_NPROCESSORS_ONLN) < 1) return 20;
    return 0;
}
