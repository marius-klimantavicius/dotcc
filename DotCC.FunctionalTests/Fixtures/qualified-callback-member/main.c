#include <stdio.h>
struct Hasher {
    long (*const hash)(const void *, long);
    const char *name;
    int (*volatile other)(int);
    int (*callbacks[2])(int);
    int (*rows)[3];
    int (*const fixed_rows)[3];
};
static long hash_bytes(const void *data, long size) {
    const unsigned char *bytes = data;
    long result = 0;
    for (long i = 0; i < size; i++) result += bytes[i];
    return result;
}
static int increment(int x) { return x + 1; }
static int twice(int x) { return x * 2; }
int main(void) {
    int grid[2][3] = {{1, 2, 3}, {4, 5, 6}};
    struct Hasher value = {hash_bytes, "fixture", increment, {increment, twice}, grid, grid};
    printf("%ld %d %d %d %d\n", value.hash("ab", 2), value.other(5),
           value.callbacks[1](8), value.hash == hash_bytes,
           sizeof(struct Hasher) == 56 && value.rows[1][2] == 6 && value.fixed_rows[0][1] == 2);
    return 0;
}
