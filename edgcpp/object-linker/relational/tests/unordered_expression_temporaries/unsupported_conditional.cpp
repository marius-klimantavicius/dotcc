#include "program.h"
Result unsupported_conditional(bool selected) {
    return selected ? Receiver::combine(make_value(1), make_value(2))
                    : Receiver::combine(make_value(2), make_value(1));
}
