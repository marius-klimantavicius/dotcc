// Preparation only: capture and native/generated observations are not yet run.
// C++17 avoids optional copies when a returned prvalue initializes its result.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int values[9];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct NormalTracked {
    int id;
    explicit NormalTracked(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }
    NormalTracked(const NormalTracked& source) noexcept : id(source.id + 10)
    {
        ++live_count;
        note(200 + id);
    }
    ~NormalTracked() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

// Returning a const-reference parameter requires the selected copy; this is
// deliberately not a named-local return whose copy could depend on NRVO.
static NormalTracked copy_for_return(const NormalTracked& source) noexcept
{
    return source;
}

static int read_temporary(const NormalTracked& value) noexcept
{
    return value.id;
}

static int ordinary_path(int early) noexcept
{
    NormalTracked outer(1);
    {
        NormalTracked copied(outer);
        values[0] = copied.id;
        if (early != 0) return copied.id + outer.id;
    }
    NormalTracked returned = copy_for_return(outer);
    values[1] = returned.id;
    values[2] = read_temporary(NormalTracked(2));
    values[3] = live_count;
    return outer.id + returned.id + values[2];
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count; // Do not conceal a leak on a repeated invocation.
    values[4] = ordinary_path(0);
    values[5] = live_count;
    note(900);
    values[6] = ordinary_path(1);
    values[7] = live_count;
    return values[4] + values[6];
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
