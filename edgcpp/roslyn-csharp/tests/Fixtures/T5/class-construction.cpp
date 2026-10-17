// Capture-first direct exception construction, including failed construction.
// Retained EDG operations and the separate native oracle establish all ordering.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int temporary_live;
static int payload_started;
static int payload_completed;
static int payload_destroyed;
static int handler_checks;
static int values[9];
static const long long payload_canary = -0x6123456789ABCDE0LL;

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct ConstructionTemporary {
    int id;
    explicit ConstructionTemporary(int selected) noexcept : id(selected)
    {
        ++temporary_live;
        ++live_count;
        note(100 + id);
    }
    ~ConstructionTemporary() noexcept
    {
        note(300 + id);
        --live_count;
        --temporary_live;
    }
};

struct ConstructedPayload {
    long long canary;
    int value;

    ConstructedPayload(int selected, int fail, const ConstructionTemporary& argument)
        : canary(payload_canary), value(selected)
    {
        ++payload_started;
        ConstructionTemporary body_guard(30 + value);
        note(400 + argument.id);
        note(500 + value);
        if (fail != 0) {
            note(600 + value);
            // A distinct pending owner is constructed directly. Both throws
            // have the same payload type; no copy or integer exception is used.
            throw ConstructedPayload(17, 0, argument);
        }
        ++payload_completed;
        ++live_count;
        note(200 + value);
    }

    ~ConstructedPayload() noexcept
    {
        note(800 + value);
        note(canary == payload_canary ? 900 + value : -900 - value);
        ++payload_destroyed;
        --live_count;
    }
};

static void raise_constructed(int fail)
{
    ConstructionTemporary local(1);
    throw ConstructedPayload(7, fail, ConstructionTemporary(2));
}

static int construction_path(int fail)
{
    try {
        raise_constructed(fail);
        note(700); // Observable only if the selected throw unexpectedly returns.
        return -1;
    } catch (const ConstructedPayload& caught) {
        if (caught.canary != payload_canary || live_count != 1 || temporary_live != 0)
            handler_checks = 0;
        note(1000 + caught.value);
        return caught.value;
    }
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    payload_started = 0;
    payload_completed = 0;
    payload_destroyed = 0;
    handler_checks = 1;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count; // Preserve prior-run evidence; never reset ownership.
    values[0] = construction_path(0);
    values[1] = live_count;
    note(1100);
    values[2] = construction_path(1);
    values[3] = live_count;
    values[4] = payload_started;
    values[5] = payload_completed;
    values[6] = payload_destroyed;
    values[7] = handler_checks && temporary_live == 0;
    return values[0] + values[2];
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
