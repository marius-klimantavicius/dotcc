#include "selected-constructor-boundary.hpp"

static int events[64];
static int event_count;
static int event_overflow;
static int fixture_calls;
static int values[9];

BoundaryCell first;
BoundaryCell second;

static void note(int event)
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

extern "C" int fixture()
{
    ++fixture_calls;
    note(500 + fixture_calls);
    values[0] = first.serial;
    values[1] = second.serial;
    values[2] = first.kind;
    values[3] = second.kind;
    values[4] = first.canary == -0x6123456789ABCDE0LL;
    values[5] = second.canary == -0x6123456789ABCDE0LL;
    values[6] = &first != &second;
    values[7] = fixture_calls;
    values[8] = 0;
    return first.serial * 10 + second.serial + fixture_calls;
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
// This is a scalar harness protocol, not a class ownership/lifetime counter.
extern "C" int lifetime_live() { return 0; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
extern "C" int startup_calls() { return fixture_calls; }
