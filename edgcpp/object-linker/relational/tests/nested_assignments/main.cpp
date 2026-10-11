#include "program.h"

int main() {
    if (check_selected(false, 0x90, 0, 7, 0)) return 1;
    if (check_selected(true, 0x90, 1, 0x10, 1)) return 2;
    if (check_selected(true, 0xff, 1, 0x7f, 0)) return 3;
    if (check_selected(true, 0x80 + 0x101, 1, 1, 1)) return 4;
    int value = 11;
    effects = 0;
    int result = effects ? (value = selected_byte(29)) : (value = selected_byte(31));
    if (effects != 1 || value != 31 || result != 31) return 5;
    effects = 0;
    if (value != 0 || (value = selected_byte(47))) return effects == 0 && value == 31 ? 0 : 6;
    return 7;
}
