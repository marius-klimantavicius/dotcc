#define _GNU_SOURCE 1
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
int main(void) {
    char *value = NULL;
    int length = asprintf(&value, "ž:%s:%lld:%c", "猫", 4294967297LL, 255);
    const unsigned char expected[] = {0xc5, 0xbe, ':', 0xe7, 0x8c, 0xab, ':', '4', '2', '9', '4', '9', '6', '7', '2', '9', '7', ':', 255, 0};
    printf("%d %d\n", length, memcmp(value, expected, sizeof(expected)) == 0);
    free(value);
    length = asprintf(&value, "");
    printf("empty=%d,%d\n", length, value != NULL && value[0] == 0);
    free(value);
    return 0;
}
