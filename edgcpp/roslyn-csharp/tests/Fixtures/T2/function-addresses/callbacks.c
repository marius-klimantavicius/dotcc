/* Application fixture: native adapters remain explicit unresolved imports. */
#include "callbacks.h"

struct CallbackTable {
    fixture_callback first;
    fixture_callback second;
};

static int increment(int value) {
    native_mark_event(11);
    return value + 3;
}

static int reenter(int value) {
    native_mark_event(31);
    return native_invoke_callback(increment, value + 1) + 2;
}

static struct CallbackTable table = {increment, reenter};
static fixture_callback callback_array[2] = {increment, reenter};

int fixture(void) {
    native_reset_events();
    fixture_callback echoed = native_echo_callback(table.first);
    fixture_erased_callback erased = (fixture_erased_callback)increment;
    fixture_callback restored = (fixture_callback)erased;
    fixture_callback supplied = native_callback_provider();
    int identities = 0;
    if (echoed == increment) identities += 1;
    if (restored == table.first) identities += 2;
    if (table.first != table.second) identities += 4;
    if (callback_array[1] == table.second) identities += 8;
    if (native_echo_callback(supplied) == supplied) identities += 16;

    int first_result = native_invoke_callback(echoed, 7);
    int nested_result = native_invoke_callback(table.second, 5);
    int supplied_result = supplied(11);
    int order = 0;
    for (int index = 0; index < native_event_count(); ++index) {
        order += (index + 1) * native_event_at(index);
    }
    return first_result + nested_result + supplied_result + identities + order;
}
