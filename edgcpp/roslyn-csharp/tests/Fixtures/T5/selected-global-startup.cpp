// Header-free capture input. Only actual global initialization constructs the
// two objects, and only actual registered shutdown destroys them.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int constructed_count;
static int destroyed_count;
static int fixture_calls;
static int values[9];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct StartupItem {
    int id;

    explicit StartupItem(int selected) noexcept : id(selected)
    {
        ++constructed_count;
        ++live_count;
        note(100 + id);
    }

    ~StartupItem() noexcept
    {
        note(300 + id);
        ++destroyed_count;
        --live_count;
    }
};

StartupItem first(1);
StartupItem second(2);

extern "C" int fixture()
{
    ++fixture_calls;
    note(500 + fixture_calls);
    values[0] = first.id;
    values[1] = second.id;
    values[2] = live_count;
    values[3] = constructed_count;
    values[4] = fixture_calls;
    values[5] = destroyed_count;
    values[6] = &first != &second;
    values[7] = live_count == constructed_count - destroyed_count;
    // Shared scalar protocol, not an automatic-local prior-live observation.
    values[8] = 0;
    return first.id * 10 + second.id + fixture_calls;
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
extern "C" int startup_constructed() { return constructed_count; }
extern "C" int startup_destroyed() { return destroyed_count; }
extern "C" int startup_calls() { return fixture_calls; }
