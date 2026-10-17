#include "selected-inline-comdat.hpp"

int selected_inline_created;
int selected_inline_destroyed;
int selected_inline_live;

static int events[64];
static int event_count;
static int event_overflow;
static int fixture_calls;
static int values[9];

InlineTracked selected_inline_first;

void selected_inline_note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

static int selected_inline_scope_a() noexcept
{
    InlineTracked local;
    selected_inline_note(600 + local.id);
    return local.id;
}

extern "C" int fixture()
{
    ++fixture_calls;
    selected_inline_note(500 + fixture_calls);
    const int first_local = selected_inline_scope_a();
    const int second_local = selected_inline_scope_b();
    values[0] = selected_inline_first.id;
    values[1] = selected_inline_second.id;
    values[2] = first_local;
    values[3] = second_local;
    values[4] = selected_inline_live;
    values[5] = selected_inline_created;
    values[6] = selected_inline_destroyed;
    values[7] = fixture_calls;
    values[8] = &selected_inline_first != &selected_inline_second;
    return first_local + second_local + fixture_calls;
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
extern "C" int lifetime_live() { return selected_inline_live; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
extern "C" int startup_created() { return selected_inline_created; }
extern "C" int startup_destroyed() { return selected_inline_destroyed; }
extern "C" int startup_calls() { return fixture_calls; }
