#include "record_transport_shared.h"
// This unit defines/uses only custom assignment of existing shared objects.
static int assignments;
CrossValue &CrossValue::operator=(const CrossValue &other) {
    ++assignments;
    value = other.value + 100;
    return *this;
}
extern "C" int cross_assign(CrossValue *target, const CrossValue *source) {
    *target = *source;
    return target->value;
}
extern "C" int cross_assignment_count() { return assignments; }
