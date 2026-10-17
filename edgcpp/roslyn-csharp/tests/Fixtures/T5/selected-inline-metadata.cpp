#include "selected-inline-metadata.hpp"

int selected_metadata_created;
int selected_metadata_destroyed;
int selected_metadata_live;
int selected_metadata_argument_count;

static int events[64];
static int event_count;
static int event_overflow;
static int fixture_calls;
static int values[9];

void selected_metadata_note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

int selected_metadata_argument() noexcept
{
    ++selected_metadata_argument_count;
    selected_metadata_note(800 + selected_metadata_argument_count);
    return 0x13579;
}

static int selected_metadata_scope_a() noexcept
{
    Whole local(selected_metadata_argument());
    selected_metadata_note(600 + local.member.id);
    return local.prefix == 0x13579 ? local.member.id : -local.member.id;
}

extern "C" int fixture()
{
    const int prior_live = selected_metadata_live;
    ++fixture_calls;
    selected_metadata_note(500 + fixture_calls);
    const int first = selected_metadata_scope_a();
    const int second = selected_metadata_scope_b();
    values[0] = first;
    values[1] = second;
    values[2] = selected_metadata_live;
    values[3] = selected_metadata_created;
    values[4] = selected_metadata_destroyed;
    values[5] = fixture_calls;
    values[6] = selected_metadata_created == selected_metadata_destroyed;
    values[7] = first > 0 && second > 0;
    values[8] = prior_live;
    return first + second + fixture_calls;
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
// Counters refer to actual Member subobjects; Whole adds no second live unit.
extern "C" int lifetime_live() { return selected_metadata_live; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
extern "C" int metadata_created() { return selected_metadata_created; }
extern "C" int metadata_destroyed() { return selected_metadata_destroyed; }
