#include "program.h"
int effects = 0;
int selected_byte(int value) { ++effects; return value; }

int check_selected(bool active, int input, int expected_effects, int expected_value, int expected_result) {
    effects = 0;
    unsigned char value = 7;
    int result = active && ((value = selected_byte(input) - 0x80) <= 0x3f);
    return effects == expected_effects && value == expected_value && result == expected_result ? 0 : 1;
}
