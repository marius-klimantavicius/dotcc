#include "selected-template-provider.hpp"

extern "C" {
int selected_template_live;
int selected_template_constructed;
int selected_template_destroyed;
}

static int values[9];
static int events[64];
static int event_count;
static int event_overflow;

extern "C" void selected_template_note(int value) noexcept {
    if (event_count < 64) events[event_count++] = value;
    else ++event_overflow;
}

extern "C" int fixture() noexcept {
    event_count = 0;
    event_overflow = 0;
    selected_template_constructed = 0;
    selected_template_destroyed = 0;
    values[8] = selected_template_live;
    int result;
    result = 0;
    {
        SelectedTemplateCell<int> first(7);
        values[0] = first.value;
        values[1] = selected_template_live;
        {
            SelectedTemplateCell<int> second(11);
            values[2] = second.value;
            values[3] = selected_template_live;
            first.value = 19;
            values[4] = first.value;
            result = first.value + second.value;
            selected_template_note(500 + result);
        }
        values[5] = selected_template_live;
    }
    values[6] = selected_template_constructed;
    values[7] = selected_template_destroyed;
    return result;
}

extern "C" int lifetime_value_count() noexcept { return 9; }
extern "C" int lifetime_value(int index) noexcept { return values[index]; }
extern "C" int lifetime_count() noexcept { return event_count; }
extern "C" int lifetime_event(int index) noexcept { return events[index]; }
extern "C" int lifetime_overflow() noexcept { return event_overflow; }
extern "C" int lifetime_live() noexcept { return selected_template_live; }
extern "C" int lifetime_constructed() noexcept { return selected_template_constructed; }
extern "C" int lifetime_destroyed() noexcept { return selected_template_destroyed; }
