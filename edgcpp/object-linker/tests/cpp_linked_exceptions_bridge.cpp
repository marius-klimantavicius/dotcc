#include "cpp_linked_exceptions.h"

namespace {
struct Guard { ~Guard() { ++bridge_destructions; } };
}

void bridge_raise(int mode) {
    Guard guard;
    try {
        raise_value(mode);
    } catch (Right*& adjusted) {
        // Catch conversion owns a pointer copy. Mutating it must not overwrite
        // the original Payload* used by the rethrow in the caller's TU.
        adjusted = nullptr;
        throw;
    } catch (...) {
        throw;
    }
}
