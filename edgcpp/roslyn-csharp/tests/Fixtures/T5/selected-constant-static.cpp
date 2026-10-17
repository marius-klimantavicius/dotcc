#include "selected-constant-static.hpp"

static int events[64];
static int event_count;
static int event_overflow;
static int fixture_calls;
static int values[64];

extern "C" const char* selected_constant_a() noexcept { return selected_shared_constant(); }
extern "C" const char* selected_different_a() noexcept { return selected_different_constant(); }

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

extern "C" int fixture()
{
    ++fixture_calls;
    note(500 + fixture_calls);
    const char* from_a = selected_constant_a();
    const char* from_b = selected_constant_b();
    const char* different_a = selected_different_a();
    const char* different_b = selected_different_b();
    const char* again_a = selected_constant_a();
    const char* again_b = selected_constant_b();
    const char* again_different_a = selected_different_a();
    const char* again_different_b = selected_different_b();
    values[0] = from_a == from_b;
    values[1] = from_a == again_a;
    values[2] = from_b == again_b;
    values[3] = different_a == different_b;
    values[4] = different_a == again_different_a;
    values[5] = different_b == again_different_b;
    values[6] = from_a != different_a;
    values[7] = from_b != different_b;
    int equal = 1;
    for (int index = 0; index < 50; ++index) {
        const unsigned char byte = static_cast<unsigned char>(from_a[index]);
        values[14 + index] = byte;
        if (byte != static_cast<unsigned char>(from_b[index]) ||
            byte != static_cast<unsigned char>(different_a[index]) ||
            byte != static_cast<unsigned char>(different_b[index])) equal = 0;
    }
    values[8] = equal;
    values[9] = from_a[49] == 0 && from_b[49] == 0 && different_a[49] == 0 && different_b[49] == 0;
    values[10] = fixture_calls;
    values[11] = static_cast<unsigned char>(again_a[0]);
    values[12] = static_cast<unsigned char>(again_b[48]);
    values[13] = 0;
    note(600 + fixture_calls);
    return values[0] + values[1] + values[2] + values[3] + values[4] + values[5] +
        values[6] + values[7] + values[8] + values[9];
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
// Scalar harness compatibility only; this fixture has no class lifetime proof.
extern "C" int lifetime_live() { return 0; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 64 ? values[index] : -1;
}
