#include "program.h"
bool unsupported_shortcircuit(bool active) {
    return active && (get_accumulator() += make_input()).value > 0;
}
