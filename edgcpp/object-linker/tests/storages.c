#include "storage_fixture.h"

#define SAVED_VALUE 17
#pragma push_macro("SAVED_VALUE")
#undef SAVED_VALUE
#define SAVED_VALUE 5
#pragma push_macro("SAVED_VALUE")
#undef SAVED_VALUE
#pragma pop_macro("SAVED_VALUE")
#if SAVED_VALUE != 5
#error inner macro restore failed
#endif
#pragma pop_macro("SAVED_VALUE")
#pragma push_macro("PREVIOUSLY_UNDEFINED")
#define PREVIOUSLY_UNDEFINED 1
#pragma pop_macro("PREVIOUSLY_UNDEFINED")
#ifdef PREVIOUSLY_UNDEFINED
#error undefined macro restore failed
#endif

int main(void) {
    if (counter != 7 || counter_pointer != &counter) return 1;
    if (run_operation(0, 4) != 22) return 2;
    counter = 9;
    if (operations[1].apply(3) != 12) return 3;
    if (operations[0].name[0] != 'i' || operations[1].name[4] != 'n') return 4;
    if (word.bytes[0] != 1 || word.bytes[3] != 4) return 5;
    if (flags.count != 19 || flags.delta != -7 || flags.ready != 1) return 6;
    flags.delta = -11;
    flags.count += 3;
    if (flags.delta != -11 || flags.count != 22 || flags.ready != 1) return 7;
    if (static_counter() != 3 || static_counter() != 4) return 8;
    if (opaque_read(opaque_instance()) != 31) return 9;
    if (thread_counter != 11) return 10;
    thread_counter += 5;
    if (thread_counter != 16 || SAVED_VALUE != 17) return 11;
    return 0;
}
