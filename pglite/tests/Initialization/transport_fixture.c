/* A deliberately small translated I/O fixture. No database/bootstrap behavior
 * is implemented here; this tests byte transport and host exit propagation. */
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/uio.h>
#include <string.h>

int fixture_copy(const char *input, const char *output, int status)
{
    FILE *source = fopen(input, "rb");
    FILE *target = fopen(output, "wb");
    int ch;
    if (source == NULL || target == NULL)
    {
        if (source != NULL) fclose(source);
        if (target != NULL) fclose(target);
        return 81;
    }
    while ((ch = fgetc(source)) != EOF)
        if (fputc(ch, target) == EOF) { fclose(source); fclose(target); return 82; }
    if (ferror(source)) { fclose(source); fclose(target); return 83; }
    if (fclose(source) != 0) { fclose(target); return 84; }
    if (fclose(target) != 0) return 85;
    return status;
}

int fixture_version(const char *output, int status)
{
    FILE *target = fopen(output, "wb");
    if (target == NULL) return 86;
    if (fputs("translated I/O fixture\n", target) == EOF) { fclose(target); return 87; }
    if (fclose(target) != 0) return 88;
    return status;
}

int fixture_positioned_io(const char *path)
{
    int descriptor = open(path, O_RDWR | O_CREAT | O_TRUNC, 0600);
    char first[3] = {'#', '#', '#'};
    char second[4] = {'#', '#', '#', '#'};
    struct iovec vectors[2] = {{first, 2}, {second, 3}};
    int result = 0;
    if (descriptor < 0) return 90;
    if (lseek(descriptor, 7, SEEK_SET) != 7) result = 91;
    else if (pwrite(descriptor, "abcdefgh", 8, 3) != 8) result = 92;
    else if (lseek(descriptor, 0, SEEK_CUR) != 7) result = 93;
    else if (preadv(descriptor, vectors, 2, 4) != 5) result = 94;
    else if (memcmp(first, "bc#", 3) != 0 || memcmp(second, "def#", 4) != 0) result = 95;
    else if (pwritev(descriptor, vectors, 2, 14) != 5) result = 96;
    else if (pread(descriptor, first, 2, 14) != 2 || memcmp(first, "bc", 2) != 0) result = 97;
    else if (lseek(descriptor, 0, SEEK_CUR) != 7) result = 98;
    else if (syncfs(descriptor) != 0) result = 99;
    if (close(descriptor) != 0 && result == 0) result = 100;
    return result;
}

#ifdef DPG_POSIO_NATIVE_MAIN
int main(int argc, char **argv) { return argc == 2 ? fixture_positioned_io(argv[1]) : 89; }
#endif
