#include "program.h"
Value *conditional_placement(bool selected) {
    return selected ? new (pointer_argument(false)) Value(input_value(false), false) : nullptr;
}
