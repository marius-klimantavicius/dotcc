#include <stdio.h>
struct Pair { int x; int y; };
struct Record { int before; struct Pair pair; int after; };
struct Spec {
    const char *note;
    int flags;
    union { struct { int position; } index; struct Pair other; } begin;
    int kind;
    union { struct { int last; int step; int limit; } range; int other; } find;
};
static struct Spec specs[] = {
    {0,3,.begin.index={2},4,.find.range={5,6,7}},
    {.flags=8, .begin.other={9,10}, 11, .find.other=12}
};
static struct Record records[] = {
    {1,.pair.x=2,3,4},
    {.pair.y=6,7},
    {8,.pair={9,10},.pair.x=11,12,13}
};
static struct Record zeroes[3] = {0};
int main(void) {
    printf("%d %d %d %d %d %d\n", specs[0].flags, specs[0].begin.index.position,
        specs[0].kind, specs[0].find.range.last, specs[0].find.range.step, specs[0].find.range.limit);
    printf("%d %d %d %d %d\n", specs[1].flags, specs[1].begin.other.x,
        specs[1].begin.other.y, specs[1].kind, specs[1].find.other);
    for (int i=0; i<3; i++) printf("%d %d %d %d\n", records[i].before,
        records[i].pair.x, records[i].pair.y, records[i].after);
    printf("%d %d %d\n", zeroes[0].before, zeroes[1].pair.y, zeroes[2].after);
    return 0;
}
