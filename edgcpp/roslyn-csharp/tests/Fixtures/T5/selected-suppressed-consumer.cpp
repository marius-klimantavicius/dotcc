#include "selected-suppressed-provider.hpp"

extern "C" {
int selected_suppressed_live;
int selected_suppressed_constructed;
int selected_suppressed_destroyed;
}

static int values[9];
static int events[64];
static int event_count;
static int event_overflow;

extern "C" void selected_suppressed_note(int value) noexcept {
    if (event_count < 64) events[event_count++] = value;
    else ++event_overflow;
}

extern "C" int fixture() noexcept {
    event_count = 0;
    event_overflow = 0;
    selected_suppressed_constructed = 0;
    selected_suppressed_destroyed = 0;
    values[8] = selected_suppressed_live;
    int result;
    result = 0;
    {
        SelectedProviderCell<int> first(7);
        values[0] = first.value;
        values[1] = selected_suppressed_live;
        {
            SelectedProviderCell<int> second(11);
            values[2] = second.value;
            values[3] = selected_suppressed_live;
            first.value = 19;
            values[4] = first.value;
            result = first.value + second.value;
            selected_suppressed_note(500 + result);
        }
        values[5] = selected_suppressed_live;
    }
    values[6] = selected_suppressed_constructed;
    values[7] = selected_suppressed_destroyed;
    return result;
}

extern "C" int lifetime_value_count() noexcept { return 9; }
extern "C" int lifetime_value(int index) noexcept { return values[index]; }
extern "C" int lifetime_count() noexcept { return event_count; }
extern "C" int lifetime_event(int index) noexcept { return events[index]; }
extern "C" int lifetime_overflow() noexcept { return event_overflow; }
extern "C" int lifetime_live() noexcept { return selected_suppressed_live; }
extern "C" int lifetime_constructed() noexcept { return selected_suppressed_constructed; }
extern "C" int lifetime_destroyed() noexcept { return selected_suppressed_destroyed; }
