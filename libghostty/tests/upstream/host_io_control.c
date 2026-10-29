#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int dotcc_upstream_dir_open(int, const uint8_t *, size_t, int *);
int dotcc_upstream_dir_next(int, uint8_t *, size_t, size_t *, int *);
int dotcc_upstream_file_open(int, const uint8_t *, size_t, int *);
int dotcc_upstream_file_size(int, uint64_t *);
int dotcc_upstream_file_read_at(int, uint64_t, uint8_t *, size_t, size_t *);
void dotcc_upstream_dir_close(int);
void dotcc_upstream_file_close(int);
size_t dotcc_upstream_io_live(void);
int dotcc_upstream_random_secure(uint8_t *, size_t);
#define CHECK(x) do { if (!(x)) { fprintf(stderr, "FAIL host-io:%d %s\n", __LINE__, #x); exit(1); } } while (0)

int main(int argc, char **argv) {
    CHECK(argc == 2);
    CHECK(dotcc_upstream_io_live() == 0);
    CHECK(dotcc_upstream_random_secure(NULL, 0) == 0);
    CHECK(dotcc_upstream_random_secure(NULL, 1) == -EINVAL);
    uint8_t first_random[64], second_random[64];
    memset(first_random, 0, sizeof(first_random)); memset(second_random, 0, sizeof(second_random));
    CHECK(dotcc_upstream_random_secure(first_random, sizeof(first_random)) == 0);
    CHECK(dotcc_upstream_random_secure(second_random, sizeof(second_random)) == 0);
    CHECK(memcmp(first_random, second_random, sizeof(first_random)) != 0);
    int root, child, fd, kind;
    size_t len, count;
    uint64_t size;
    uint8_t bytes[64], tiny[1];
    CHECK(dotcc_upstream_dir_open(-100, (const uint8_t *)argv[1], strlen(argv[1]), &root) == 0);
    CHECK(dotcc_upstream_dir_next(root, tiny, sizeof(tiny), &len, &kind) == -ENOBUFS);
    size_t required = len;
    CHECK(dotcc_upstream_dir_next(root, bytes, sizeof(bytes), &len, &kind) == 0 && len == required);
    int file_count = kind == 1, dir_count = kind == 2;
    CHECK(dotcc_upstream_dir_next(root, bytes, sizeof(bytes), &len, &kind) == 0);
    file_count += kind == 1; dir_count += kind == 2;
    CHECK(file_count == 1 && dir_count == 1);
    CHECK(dotcc_upstream_dir_next(root, bytes, sizeof(bytes), &len, &kind) == 1 && len == 0);
    CHECK(dotcc_upstream_dir_next(root, bytes, sizeof(bytes), &len, &kind) == 1);
    CHECK(dotcc_upstream_file_open(root, (const uint8_t *)"bytes.bin", 9, &fd) == 0);
    CHECK(dotcc_upstream_file_size(fd, &size) == 0 && size == 9);
    CHECK(dotcc_upstream_file_read_at(fd, 2, bytes, 4, &count) == 0 && count == 4);
    CHECK(bytes[0] == 127 && bytes[1] == 128 && bytes[2] == 255 && bytes[3] == 'A');
    CHECK(dotcc_upstream_file_read_at(fd, 0, bytes, sizeof(bytes), &count) == 0 && count == 9);
    CHECK(bytes[0] == 0 && bytes[1] == 1 && bytes[7] == 0 && bytes[8] == 10);
    CHECK(dotcc_upstream_file_read_at(fd, 9, bytes, sizeof(bytes), &count) == 0 && count == 0);
    CHECK(dotcc_upstream_file_read_at(fd, UINT64_MAX, bytes, sizeof(bytes), &count) == -EINVAL);
    dotcc_upstream_file_close(fd); dotcc_upstream_file_close(fd);
    CHECK(dotcc_upstream_file_size(fd, &size) == -EBADF);
    CHECK(dotcc_upstream_dir_open(root, (const uint8_t *)"nested", 6, &child) == 0);
    CHECK(dotcc_upstream_file_open(child, (const uint8_t *)"data.txt", 8, &fd) == 0);
    CHECK(dotcc_upstream_file_read_at(fd, 0, bytes, sizeof(bytes), &count) == 0 && count == 6);
    CHECK(memcmp(bytes, "nested", 6) == 0);
    dotcc_upstream_file_close(fd); dotcc_upstream_dir_close(child);
    CHECK(dotcc_upstream_file_open(root, (const uint8_t *)"missing", 7, &fd) == -ENOENT);
    CHECK(dotcc_upstream_file_open(root, (const uint8_t *)"nested", 6, &fd) == -EISDIR);
    CHECK(dotcc_upstream_dir_open(123456, (const uint8_t *)"nested", 6, &child) == -EBADF);
    uint8_t nul_path[3] = {'a', 0, 'b'};
    CHECK(dotcc_upstream_file_open(root, nul_path, 3, &fd) == -EINVAL);
    dotcc_upstream_dir_close(root); dotcc_upstream_dir_close(root);
    CHECK(dotcc_upstream_io_live() == 0);
    puts("PASS host-io directory, positional binary reads, EOF, errors and resource cleanup");
    return 0;
}
