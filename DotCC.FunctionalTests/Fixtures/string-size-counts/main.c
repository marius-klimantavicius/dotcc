#include <string.h>
#include <stdio.h>

int main(void) {
    const char *source = "abcdef";
    char data[32];
    size_t n = 7;
    void *(*copy)(void *, const void *, size_t) = &memcpy;
    void *(*move)(void *, const void *, size_t) = &memmove;
    void *(*fill)(void *, int, size_t) = &memset;
    int (*compare)(const void *, const void *, size_t) = &memcmp;
    fill(data, 0, sizeof(data));
    copy(data, source, n);
    move(data + 1, data, n);
    printf("right=%s\n", data);
    move(data, data + 1, n);
    printf("left=%s same=%d found=%d\n", data, compare(data, source, n) == 0, (int)((char *)memchr(data, 'd', n) - data));
    size_t wide = (size_t)4294967296ULL;
    printf("wide=%d\n", strncmp(source, "abcdef", wide) == 0);
    strncat(data, "xy", wide);
    printf("append=%s\n", data);
    strncpy(data, "hi", n);
    printf("pad=%d,%d,%d\n", (int)data[2], (int)data[5], (int)data[6]);
    return 0;
}
