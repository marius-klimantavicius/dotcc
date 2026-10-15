// Genuine capture candidate: rethrow the same integer exception through two
// selected handlers. Native observations are separate comparison evidence;
// retained operations and stock contracts establish cleanup and ownership.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int values[5];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct RethrowTracked {
    int id;
    explicit RethrowTracked(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }
    ~RethrowTracked() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

static void inner_path()
{
    try {
        RethrowTracked before(2);
        note(507);
        throw 7;
    } catch (int value) {
        values[0] = value;
        value = 99;
        values[1] = value;
        note(600 + value);
        RethrowTracked in_handler(3);
        throw;
    }
}

static int outer_path()
{
    RethrowTracked outer(1);
    try {
        inner_path();
        return -1;
    } catch (int value) {
        values[2] = value;
        note(700 + value);
        return value;
    }
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 5; ++index) values[index] = 0;
    values[4] = live_count; // Preserve evidence from repeated invocations.
    int result = outer_path();
    values[3] = live_count;
    return result;
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
    return index >= 0 && index < 5 ? values[index] : -1;
}
