#include <stdio.h>
#include <stddef.h>
#include <uchar.h>
extern char exported[5];
extern char *exported_ptr;
static int step(void) {
    static char persistent[] = (("p" "q"));
    return ++persistent[0];
}
int main(void) {
    char local[] = (("a\0" "b"));
    char twin[] = ("a\0b");
    char padded[6] = (("xy"));
    char exact[2] = ("xy");
    char16_t u16[] = ((u"a" u"b"));
    char32_t u32[] = ((U"a" U"b"));
    wchar_t wide[] = ((L"a" L"b"));
    const char *expression = (("ok"));
    local[0] = 99;
    exported_ptr[2] = 74;
    if (local[0] != 99 || twin[0] != 97 || local == twin || exported[2] != 74
        || local[1] != 0 || local[3] != 0 || padded[2] != 0 || padded[5] != 0
        || u16[2] != 0 || u32[2] != 0 || wide[2] != 0
        || sizeof(u16)/sizeof(*u16) != 3 || sizeof(u32)/sizeof(*u32) != 3
        || sizeof(wide)/sizeof(*wide) != 3) return 2;
    int persisted = step() + step();
    printf("%d %d %d %d %d %d %d %d %d %d %d %d\n", (int)sizeof(exported),
        exported_ptr == exported, (int)sizeof(local), local[2], twin[0], padded[5], exact[1],
        persisted, u16[1], (int)u32[1], (int)wide[1], expression[1]);
    return 0;
}
