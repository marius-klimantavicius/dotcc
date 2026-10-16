// Capture-first candidate: public facts and execution observations are pending.
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

struct Base {
    int id;

    explicit Base(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }

    virtual int evaluate(int argument) noexcept
    {
        note(600 + id);
        return id + argument;
    }

    // Deliberately nonvirtual: this candidate has no deleting destructor call.
    ~Base() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

struct Derived : Base {
    int bonus;

    Derived(int value, int extra) noexcept : Base(value), bonus(extra)
    {
        ++live_count;
        note(200 + id);
    }

    int evaluate(int argument) noexcept override
    {
        note(700 + id);
        return id + bonus + argument;
    }

    ~Derived() noexcept
    {
        note(400 + id);
        --live_count;
    }
};

// The receiver's dynamic type is supplied by the caller. Its actual lowered
// call representation must come from capture, not source-level rebinding.
static int dispatch(Base* receiver, int argument) noexcept
{
    note(500);
    return receiver->evaluate(argument);
}

static int ordinary_path() noexcept
{
    Base base(2);
    Derived derived(5, 11);
    values[0] = base.id;
    values[1] = derived.id;
    values[2] = derived.bonus;
    values[3] = live_count;
    values[4] = dispatch(&base, 3);
    values[5] = dispatch(&derived, 7);
    values[6] = live_count;
    return values[4] + values[5];
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count;
    int result = ordinary_path();
    values[7] = live_count;
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
    return index >= 0 && index < 9 ? values[index] : -1;
}
