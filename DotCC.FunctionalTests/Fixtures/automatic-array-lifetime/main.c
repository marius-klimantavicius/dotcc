#include <stdio.h>
#include <stdint.h>

struct item { int first, second; };
struct __attribute__((aligned(32))) aligned_item { int value; };
static int effects;
static int next(void) { return ++effects; }

static int recursive(int depth)
{
    int total = 0;
    for (int i = 0; i < 2; i++) {
        unsigned char state[10240] = { 0 };
        state[0] = depth + 1;
        state[10239] = i + 1;
        if (depth) total += recursive(depth - 1);
        if (state[0] != depth + 1 || state[10239] != i + 1) return -100000;
        total += state[0] + state[10239];
    }
    return total;
}

int main(void)
{
    int checksum = 0;
    int __dotcc_array_storage0_new = 7;
    int __dotcc_array_storage0[2] = { 8, 9 };
    for (int i = 0; i < 4096; i++) {
        /* More than40MiB of dynamic localloc would exhaust the CLR stack. */
        unsigned char buffer[10240] = { 0 };
        if (buffer[0] || buffer[5119] || buffer[10239]) return 1;
        buffer[0] = 17; buffer[5119] = 31; buffer[10239] = 53;
        int initialized[3] = { next(), 7, next() };
        if (initialized[1] != 7 || initialized[2] != initialized[0] + 1) return 2;
        checksum += initialized[0] + initialized[2];
        void *pointers[2] = { buffer, 0 };
        if (pointers[0] != buffer || pointers[1]) return 3;
        pointers[1] = buffer + 1;
        struct item entries[2] = { { i, i + 1 }, { 0 } };
        if (entries[0].second != i + 1 || entries[1].first) return 4;
        int matrix[2][3] = { { i, 2, 3 }, { 4, 5, 6 } };
        if (matrix[0][0] != i || matrix[1][2] != 6) return 5;
        struct aligned_item aligned[2] = { { i }, { i + 1 } };
        if (((uintptr_t)&aligned[0] & 31) || ((uintptr_t)&aligned[1] & 31)
                || aligned[0].value != i || aligned[1].value != i + 1) return 6;
        for (int j = 0; j < 3; j++) {
            unsigned char nested[13] = { 9 };
            if (nested[0] != 9 || nested[12]) return 7;
            nested[12] = 1;
            if (buffer[5119] != 31) return 8;
        }
        if (i % 2) continue;
        if (buffer[10239] != 53) return 9;
    }
    int visits = 0;
again:
    {
        unsigned char restarted[16384] = { 0 };
        if (restarted[0] || restarted[16383]) return 10;
        restarted[0] = 2; restarted[16383] = 3;
        visits++;
    }
    if (visits < 2048) goto again;
    if (effects != 8192 || checksum != 33558528 || recursive(4) != 207) return 11;
    if (__dotcc_array_storage0_new != 7 || __dotcc_array_storage0[1] != 9) return 12;
    printf("4096 2048 8192 33558528 207\n");
    return 0;
}
