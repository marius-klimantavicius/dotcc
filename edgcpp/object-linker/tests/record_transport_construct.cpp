#include "record_transport_shared.h"
// This unit constructs and transports the shared type; it never assigns it.
extern "C" CrossValue cross_make(int value) { return {value}; }
extern "C" int cross_read() {
    CrossValue first = cross_make(7);
    CrossValue second = first;
    return second.value;
}
