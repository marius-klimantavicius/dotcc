#include <stdio.h>

static int root = 7;
static int plus(int n) { return n + 3; }
typedef int (*Callback)(int);
struct Relocation { int value; int *owner; Callback apply; };
static struct Relocation relocated[128] = {
    [0] = {11, &root, plus}, [127] = {19, &root, plus}
};
static struct Relocation omitted_pointers[128] = {{23}};

/* This representation exceeds the scalar serializer's supported 64 bits. */
struct WideInteger { unsigned __int128 value; };
static struct WideInteger unsupported[65] = {
    [0] = {((unsigned __int128)1 << 100) + 7},
    [64] = {((unsigned __int128)3 << 96) + 9}
};

static int calls;
static int next(void) { return ++calls; }
struct Effect { int value; int second; int third; };
static int effects(int select) {
    calls = 0;
    struct Effect *rows = select
        ? (struct Effect[128]){{next(), 10}, {next(), 20}}
        : (struct Effect[128]){{10 + next(), 30}, {10 + next(), 40}};
    return calls == 2 && rows[0].value == (select ? 1 : 11)
        && rows[1].value == (select ? 2 : 12) && rows[127].value == 0
        && rows[0].second == (select ? 10 : 30) && rows[1].third == 0;
}

int main(void) {
    int pointers = relocated[0].owner == &root && relocated[127].owner == &root
        && relocated[0].apply == plus && relocated[127].apply(4) == 7
        && relocated[1].owner == 0 && relocated[1].apply == 0
        && omitted_pointers[0].value == 23 && omitted_pointers[0].owner == 0
        && omitted_pointers[127].apply == 0;
    *relocated[0].owner = 17;
    pointers = pointers && *relocated[127].owner == 17 && relocated[0].apply(root) == 20;
    int wide = (unsigned long long)(unsupported[0].value >> 64) == (1ULL << 36)
        && (unsigned long long)unsupported[0].value == 7
        && unsupported[1].value == 0
        && (unsigned long long)(unsupported[64].value >> 64) == (3ULL << 32)
        && (unsigned long long)unsupported[64].value == 9;
    int first=effects(0), second=effects(1);
    printf("pointers=%d wide=%d effects=%d,%d calls=%d\n", pointers, wide, first, second, calls);
    return pointers && wide && first && second ? 0 : 1;
}
