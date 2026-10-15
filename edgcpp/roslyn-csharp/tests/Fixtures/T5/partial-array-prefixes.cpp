// Additional capture candidate: every incomplete prefix of the fixed array.
// Element definitions and array_path match the preserved partial-array.cpp;
// the fixture records success and failures at elements one, two and three.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int next_id;
static int fail_at;
static int values[9];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct ArrayElement {
    int id;
    ArrayElement() : id(++next_id)
    {
        note(100 + id);
        if (id == fail_at) {
            note(500 + id);
            throw 7;
        }
        ++live_count;
        note(200 + id);
    }
    ~ArrayElement() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

static int array_path(int selected_failure)
{
    next_id = 0;
    fail_at = selected_failure;
    try {
        ArrayElement items[3];
        note(600 + items[0].id);
        note(610 + items[1].id);
        note(620 + items[2].id);
        return items[0].id * 100 + items[1].id * 10 + items[2].id;
    } catch (int value) {
        note(700 + value);
        return value;
    }
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count; // Preserve evidence from repeated invocations.
    values[0] = array_path(0);
    values[1] = live_count;
    note(900);
    values[2] = array_path(1);
    values[3] = live_count;
    note(901);
    values[4] = array_path(2);
    values[5] = live_count;
    note(902);
    values[6] = array_path(3);
    values[7] = live_count;
    return values[0] + values[2] + values[4] + values[6];
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
extern "C" int lifetime_live() { return live_count; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
