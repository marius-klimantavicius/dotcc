#include <stdio.h>
#include <stdint.h>
unsigned char first[8] = {255, 0, 128, 4};
unsigned char second[8] = {255, 0, 128, 4};
char exact[3] = "abc";
char embedded[] = "a\0b\0";
_Alignas(64) unsigned char aligned[] = {255, 0, 128, 4};
unsigned char *links[] = {second + 2, first, aligned};
unsigned char conversions[] = {(unsigned char)-1, (0xffffffffu / 2u) >> 24,
    sizeof(void *), (unsigned char)0x1ffULL, 0 ? 97 : 65};
int main(void) {
    first[0] = 7;
    if (conversions[0] != 255 || conversions[1] != 127 || conversions[2] != 8
        || conversions[3] != 255 || conversions[4] != 65) return 2;
    printf("%d %d %d %d %d %d %d %d %d\n", first != second, first[0], second[0],
        first[7], exact[2], (int)sizeof(embedded), embedded[4],
        (int)((uintptr_t)aligned % 64), *links[0] + *links[1] + *links[2]);
    return 0;
}
