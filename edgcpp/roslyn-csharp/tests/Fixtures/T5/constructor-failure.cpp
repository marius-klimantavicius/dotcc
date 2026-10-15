// Genuine capture candidate: constructor failure after one member completes.
// Capture/native observations, rather than this source, establish the retained
// cleanup states, member storage offset and selected destructor entries.
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

struct ConstructorMember {
    int id;
    explicit ConstructorMember(int value) noexcept : id(value)
    {
        ++live_count;
        note(100 + id);
    }
    ~ConstructorMember() noexcept
    {
        note(300 + id);
        --live_count;
    }
};

struct ConstructorWhole {
    int canary;
    ConstructorMember member;

    explicit ConstructorWhole(int fail) : canary(0x13579), member(1)
    {
        note(200);
        note(canary);
        if (fail != 0) {
            note(507);
            throw 7;
        }
        note(201);
    }

    ~ConstructorWhole() noexcept
    {
        note(400);
        note(canary + 1);
    }
};

static int constructor_path(int fail)
{
    try {
        ConstructorWhole object(fail);
        note(500);
        note(object.canary + 2);
        return object.canary + object.member.id;
    } catch (int value) {
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
    values[0] = constructor_path(0);
    values[1] = live_count;
    note(900);
    values[2] = constructor_path(1);
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
