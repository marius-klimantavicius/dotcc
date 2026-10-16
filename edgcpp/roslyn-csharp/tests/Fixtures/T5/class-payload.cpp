// Capture-first class exception candidate. Retained operations and the
// independent native oracle establish copying, cleanup and payload ownership.
static int events[64];
static int event_count;
static int event_overflow;
static int live_count;
static int values[9];

// The positive signed LL literal is representable before unary negation.
// No out-of-range unsigned-to-signed conversion supplies the high-bit value.
static const long long payload_canary = -0x6123456789ABCDE0LL;

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Payload {
    long long canary;
    int value;

    explicit Payload(int selected) noexcept
        : canary(payload_canary), value(selected)
    {
        ++live_count;
        note(100 + value);
    }

    Payload(const Payload& source) noexcept
        : canary(source.canary), value(source.value)
    {
        ++live_count;
        note(200 + value);
    }

    ~Payload() noexcept
    {
        note(300 + value);
        note(canary == payload_canary ? 400 + value : -400 - value);
        --live_count;
    }
};

static void raise_selected(const Payload& source)
{
    Payload guard(3);
    note(500 + source.value);
    throw source;
}

static int class_path()
{
    Payload source(7);
    values[0] = source.value;
    try {
        raise_selected(source);
        note(600); // Observable only if the selected throw unexpectedly returns.
    } catch (const Payload& caught) {
        values[1] = caught.value;
        values[2] = caught.canary == payload_canary;
        values[3] = &caught != &source;
        values[4] = live_count;
        note(700 + caught.value);
    }
    values[5] = live_count;
    values[6] = source.canary == payload_canary && source.value == 7;
    note(800 + source.value);
    return source.value + values[1];
}

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = live_count; // Preserve prior-run evidence; never reset ownership.
    int result = class_path();
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
