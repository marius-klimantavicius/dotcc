#include "program.h"
char greeting[8] = "hi";
const char *provider_shared() { return shared_pointer; }
const char *embedded() { return "A\0B"; }
constexpr const char *offset_text = "offset" + 2;
const char *offset_literal() { return offset_text; }
const wchar_t *wide_literal() { return L"A\u03a9"; }
const char16_t *utf16_literal() { return u"A\U0001f600"; }
const char32_t *utf32_literal() { return U"A\U0001f600"; }
const char8_t *utf8_literal() { return u8"A\U0001f600"; }
int local_literals() {
    char padded[6] = "ab";
    if (padded[0] != 'a' || padded[1] != 'b' || padded[2] || padded[5]) return 1;
    padded[0] = 'z';
    char exact[3] = "hi";
    if (exact[0] != 'h' || exact[1] != 'i' || exact[2]) return 2;
    return padded[0] == 'z' ? 0 : 3;
}
