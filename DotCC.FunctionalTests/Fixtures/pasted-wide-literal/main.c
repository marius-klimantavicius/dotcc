#include <stdio.h>
#include <wchar.h>
#define WIDE(x) L ## x
#define U16(x) u ## x
#define JOIN(a,b) a ## b
struct Holder { int null; };
int main(void) {
    wchar_t values[] = WIDE("é😀");
    wchar_t c = WIDE('\x1234');
    struct Holder h;
    struct Holder *p = &h;
    int null = 7;
    p->null = null;
    switch (c) {
        case 4660: break;
        case 65536: return 1;
        default: return 2;
    }
    switch (c) {
        case 0: if (null) { case 4660: h.null += 0; } break;
        case 65536: return 3;
        default: return 4;
    }
    printf("%u %u %u %u %u %d %d\n", values[0], values[1], values[2], values[3], c, U16('A'), JOIN(12,34) + h.null);
    return 0;
}
