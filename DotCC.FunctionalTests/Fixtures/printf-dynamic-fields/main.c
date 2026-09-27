#include <stdio.h>
#include <string.h>
int main(void) {
    char directive[20];
    const char *source = "'use strict'; tail";
    int n = snprintf(directive, sizeof directive, "%.*s", 10, source + 1);
    if (n != 10 || strcmp(directive, "use strict")) return 1;
    printf("directive [%s] %d\n", directive, n);
    printf("fields [%*.*s] [%*d] [%.*s] [%.*f]\n", 7, 3, "abcdef", -5, 42, -1, "end", 2, 1.25);
    char small[5];
    n = snprintf(small, sizeof small, "%*.*s", 6, 3, "abcdef");
    printf("truncated [%s] %d\n", small, n);
    unsigned char bytes[3] = {0xc3, 0xa9, 0x80};
    n = snprintf(directive, sizeof directive, "%.*s", 3, (char *)bytes);
    if (n != 3 || (unsigned char)directive[0] != 0xc3 ||
        (unsigned char)directive[1] != 0xa9 || (unsigned char)directive[2] != 0x80 || directive[3]) return 2;
    printf("bounded bytes %d %u %u %u\n", n, (unsigned char)directive[0], (unsigned char)directive[1], (unsigned char)directive[2]);
    return 0;
}
