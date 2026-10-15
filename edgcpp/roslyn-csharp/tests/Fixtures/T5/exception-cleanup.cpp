// Preparation only: run after the bounded normal-lifetime slice is established.
// All throw/catch execution stays inside this generated program's call graph.
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

struct ExceptionTracked {
    int id;
    explicit ExceptionTracked(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }
    ~ExceptionTracked() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

static void raise_selected(int value)
{
    ExceptionTracked nested(3);
    note(500 + value);
    throw value;
}

static int exceptional_path(int raising)
{
    ExceptionTracked outer(1);
    try {
        ExceptionTracked inner(2);
        if (raising != 0) raise_selected(7);
        note(600);
        return outer.id + inner.id;
    } catch (int value) {
        note(700 + value);
        return outer.id + value;
    }
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 5; ++index) values[index] = 0;
    values[4] = live_count; // Preserve evidence from repeated invocations.
    values[0] = exceptional_path(0);
    values[1] = live_count;
    note(900);
    values[2] = exceptional_path(1);
    values[3] = live_count;
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
    return index >= 0 && index < 5 ? values[index] : -1;
}
