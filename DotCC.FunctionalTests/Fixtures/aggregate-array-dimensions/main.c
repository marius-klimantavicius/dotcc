#include <stdio.h>

struct Entry { int first; int second; };
static const struct Entry table[2][3] = {{{1,2}}, {{3,4}, {5,6}}};
static struct Entry cube[2][2][2] = {
    {{{7,8}}, {{9,10},{11,12}}},
    {{{13,14}}}
};

int main(void) {
    struct Entry value = {25,26};
    struct Entry copies[2][2] = {value,value,value};
    struct Entry local[2][2] = {{{15,16}}, {{17,18},{19,20}}};
    static struct Entry saved[2][2] = {{{21,22}}, {{23,24}}};
    printf("table %d %d %d %d\n", table[0][0].second, table[0][1].first,
           table[1][1].second, table[1][2].second);
    printf("cube %d %d %d %d\n", cube[0][1][1].second, cube[0][0][1].first,
           cube[1][0][0].second, cube[1][1][1].second);
    printf("local %d %d %d saved %d %d\n", local[0][0].first,
           local[0][1].second, local[1][1].second,
           saved[1][0].second, saved[1][1].first);
    printf("copies %d %d %d\n", copies[0][1].first, copies[1][0].second, copies[1][1].first);
    return 0;
}
