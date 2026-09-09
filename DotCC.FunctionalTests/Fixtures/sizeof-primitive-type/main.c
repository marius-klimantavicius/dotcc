#include <stdio.h>
#include <stddef.h>
typedef long Word;
#define IS_SIZE_T(value) _Generic((value), unsigned long: 1, default: 0)
int main(void) {
    printf("%d %d %d %d\n", IS_SIZE_T(sizeof(char)), IS_SIZE_T(sizeof(int)),
           IS_SIZE_T(sizeof(long)), IS_SIZE_T(sizeof(Word)));
    printf("%d %d %d %d\n", sizeof(char) < -1, sizeof(int) < -1,
           sizeof(long) < -1, sizeof(Word) < -1);
    printf("%llu %llu\n", (unsigned long long)(-sizeof(long) / 2),
           (unsigned long long)(-sizeof(Word) / 2));
    return 0;
}
