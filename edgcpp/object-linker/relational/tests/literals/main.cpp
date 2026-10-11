#include "program.h"
int main() {
    const char *value = embedded();
    if (value[0] != 'A' || value[1] || value[2] != 'B' || value[3]) return 1;
    if (offset_literal()[0] != 'f' || offset_literal()[4]) return 2;
    if (provider_shared()[0] != 's' || shared_pointer[0] != 's') return 3;
    if (shared_literal()[6] || shared_literal()[7] != 't') return 4;
    if (greeting[0] != 'h' || greeting[1] != 'i' || greeting[2] || greeting[7]) return 5;
    greeting[0] = 'H';
    if (greeting[0] != 'H' || local_literals()) return 6;
    if (wide_literal()[0] != 'A' || wide_literal()[1] != 0x3a9 || wide_literal()[2]) return 7;
    if (utf16_literal()[0] != 'A' || utf16_literal()[1] != 0xd83d || utf16_literal()[2] != 0xde00 || utf16_literal()[3]) return 8;
    if (utf32_literal()[0] != 'A' || utf32_literal()[1] != 0x1f600 || utf32_literal()[2]) return 9;
    if (utf8_literal()[0] != 'A' || utf8_literal()[1] != 0xf0 || utf8_literal()[2] != 0x9f || utf8_literal()[3] != 0x98 || utf8_literal()[4] != 0x80 || utf8_literal()[5]) return 10;
    return 0;
}
