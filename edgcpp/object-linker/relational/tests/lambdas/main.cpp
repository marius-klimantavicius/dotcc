#include "program.h"

int main() {
    if (captureless_call(10) != 13 || body_effects != 1) return 1;
    State state = {17};
    int& selected = captureless_reference(state);
    selected = 19;
    if (state.value != 19 || body_effects != 2) return 2;
    if (captured_values(23) != 28 || body_effects != 3) return 3;
    int value = 31;
    if (captured_reference(value) != 38 || value != 38 || body_effects != 4) return 4;
    if (capture_once(41) != 43 || capture_effects != 1 || body_effects != 5) return 5;
    if (converted_call(47) != 58 || body_effects != 6) return 6;
    return 0;
}
