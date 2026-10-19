/* Focused native/emitted stdio controls; never accesses FILE after failed freopen. */
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#define CHECK(x) do { if (!(x)) { printf("freopen check failed line=%d errno=%d\n", __LINE__, errno); return 1; } checks++; } while (0)
int main(void) {
    int checks = 0;
    char buffer[64];
    char bytes[16] = {0};
    FILE *file = fopen("r7-old", "w+");
    CHECK(file != NULL);
    CHECK(setvbuf(file, buffer, _IOFBF, sizeof buffer) == 0);
    CHECK(fputc('A', file) == 'A');
    CHECK(freopen("r7-new", "w+", file) == file);
    CHECK(fputc('B', file) == 'B');
    CHECK(fclose(file) == 0);
    file = fopen("r7-old", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'A');
    CHECK(fclose(file) == 0);
    file = fopen("r7-new", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'B');
    CHECK(fclose(file) == 0);

    /* A same-path reopen must drain before w truncates, so only B remains. */
    file = fopen("r7-old", "w+");
    CHECK(file != NULL && setvbuf(file, buffer, _IOFBF, sizeof buffer) == 0);
    CHECK(fputc('A', file) == 'A');
    CHECK(freopen("r7-old", "w+", file) == file);
    CHECK(fputc('B', file) == 'B' && fclose(file) == 0);
    file = fopen("r7-old", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'B');
    CHECK(fclose(file) == 0);

    /* Failed open closes the old descriptor, after flushing its pending A. */
    file = fopen("r7-old", "w+");
    CHECK(file != NULL && setvbuf(file, buffer, _IOFBF, sizeof buffer) == 0);
    int fd = fileno(file);
    CHECK(fputc('A', file) == 'A');
    errno = 0;
    CHECK(freopen("r7-does-not-exist/child", "w+", file) == NULL && errno == ENOENT);
    errno = 0;
    CHECK(fcntl(fd, F_GETFD) == -1 && errno == EBADF);
    file = fopen("r7-old", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'A');
    CHECK(fclose(file) == 0);

    /* Invalid mode still closes the old association. */
    file = fopen("r7-old", "w+");
    CHECK(file != NULL && setvbuf(file, buffer, _IOFBF, sizeof buffer) == 0);
    fd = fileno(file);
    CHECK(fputc('A', file) == 'A');
    errno = 0;
    CHECK(freopen("r7-new", "invalid", file) == NULL && errno == EINVAL);
    errno = 0;
    CHECK(fcntl(fd, F_GETFD) == -1 && errno == EBADF);
    file = fopen("r7-old", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'A');
    CHECK(fclose(file) == 0);

    /* Old flush failure does not redirect the failed byte to the new file. */
    file = fopen("/dev/full", "w");
    CHECK(file != NULL && setvbuf(file, buffer, _IOFBF, sizeof buffer) == 0);
    CHECK(fputc('A', file) == 'A');
    CHECK(freopen("r7-new", "w+", file) == file && ferror(file) == 0 && feof(file) == 0);
    CHECK(fputc('B', file) == 'B' && fclose(file) == 0);
    file = fopen("r7-new", "r");
    CHECK(file != NULL && fread(bytes, 1, sizeof bytes, file) == 1 && bytes[0] == 'B');
    CHECK(fclose(file) == 0);
    CHECK(remove("r7-old") == 0 && remove("r7-new") == 0);
    printf("freopen-r7 passed checks=%d old=A new=B failure-closes=1 flush-error-resets=1\n", checks);
    return 0;
}
