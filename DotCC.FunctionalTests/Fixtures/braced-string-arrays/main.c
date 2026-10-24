#include <stdio.h>
#include <stddef.h>
#include <uchar.h>

char global[] = { "NONE" };
static unsigned char static_global[] = { "a\0b", };

static int next(void) {
    static char persistent[] = { "xy" };
    return ++persistent[0];
}

int main(void) {
    char no_curve[] = { "NONE" };
    char empty[] = { "" }, tail[] = { "a\0" "b" };
    signed char signed_bytes[] = { "\xff" };
    unsigned char utf8[] = { "λ" };
    char16_t u16[] = { u"😀" };
    char32_t u32[] = { U"😀" };
    wchar_t wide[] = { L"ab" };
    char padded[7] = { "NONE" }, exact[4] = { "NONE" };
    const char *pointers[] = { "NONE", "other" };
    no_curve[0] = 'D';
    if (sizeof(global) != 5 || sizeof(no_curve) != 5 || global[0] != 'N'
        || no_curve[0] != 'D' || no_curve[4] != 0 || sizeof(empty) != 1 || empty[0] != 0
        || sizeof(tail) != 4 || tail[1] != 0 || tail[2] != 'b' || tail[3] != 0
        || sizeof(signed_bytes) != 2 || (unsigned char)signed_bytes[0] != 255
        || sizeof(utf8) != 3 || utf8[0] != 206 || utf8[1] != 187 || utf8[2] != 0
        || sizeof(u16)/sizeof(*u16) != 3 || u16[0] != 55357 || u16[1] != 56832 || u16[2] != 0
        || sizeof(u32)/sizeof(*u32) != 2 || u32[0] != 128512 || u32[1] != 0
        || sizeof(wide)/sizeof(*wide) != 3 || wide[2] != 0
        || padded[4] != 0 || padded[6] != 0 || sizeof(exact) != 4 || exact[3] != 'E'
        || sizeof(pointers)/sizeof(*pointers) != 2 || pointers[1][0] != 'o'
        || sizeof(static_global) != 4 || static_global[2] != 'b') return 1;
    int first = next(), second = next();
    if (first != 121 || second != 122) return 2;
    printf("braced strings: bytes, extents, ownership, persistence, pointer arrays\n");
    return 0;
}
