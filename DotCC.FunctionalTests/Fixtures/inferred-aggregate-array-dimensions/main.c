#include <stdio.h>

struct Pair { int a; int b; };
union Value { int number; unsigned bits; };
static const struct Pair table[][3] = {{{1,2}}, {{3,4},{5,6}}};
static struct Pair cube[][2][2] = {{{{7,8}}, {{9,10},{11,12}}}, {{{13,14}}}};
static struct Pair designated[][2] = {[2][1] = {15,16}, [0] = {{17,18}}};
static union Value unions[][2] = {{{19}}, {{20},{21}}};

int main(void) {
    struct Pair value = {22,23};
    struct Pair copied[][2] = {value,value,value};
    struct Pair local[][2] = {{{24,25}}, {{26,27},{28,29}}};
    static struct Pair saved[][2] = {{{30,31}}, {{32,33}}};
    printf("table %zu %zu %d %d %d\n", sizeof(table), sizeof(table[0]),
        table[0][0].b, table[0][1].a, table[1][1].b);
    printf("cube %zu %d %d %d\n", sizeof(cube), cube[0][1][1].b,
        cube[1][0][0].b, cube[1][1][1].a);
    printf("designated %zu %d %d %d\n", sizeof(designated), designated[0][0].b,
        designated[1][1].a, designated[2][1].b);
    printf("union %zu %d %d %d\n", sizeof(unions), unions[0][1].number,
        unions[1][0].number, unions[1][1].number);
    printf("local %zu %d %d saved %zu %d copied %zu %d %d\n", sizeof(local),
        local[0][1].b, local[1][1].b, sizeof(saved), saved[1][0].b,
        sizeof(copied), copied[1][0].b, copied[1][1].a);
    return 0;
}
