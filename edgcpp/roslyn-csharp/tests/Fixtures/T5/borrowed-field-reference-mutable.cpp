// Capture-first borrowed field reference. Trivial records have no counted
// lifetime resources; zero live/unused slots are only harness protocol.
static int events[64];
static int event_count;
static int event_overflow;
static int values[9];

static void note(int event)
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Member {
    int first;
    int second;
};

using FieldAlias = Member;

struct Owner {
    long long canary;
    mutable FieldAlias payload;

    const FieldAlias& view() const { return payload; }
};

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;

    Owner first;
    Owner other;
    first.canary = -0x6123456789ABCDE0LL;
    first.payload.first = 5;
    first.payload.second = 7;
    other.canary = 0x5123456789ABCDE0LL;
    other.payload.first = 101;
    other.payload.second = 103;
    note(100);

    const FieldAlias& borrowed = first.view();
    values[0] = borrowed.first;
    values[1] = borrowed.second;
    note(200 + values[0]);
    note(300 + values[1]);

    first.payload.first = 11;
    first.payload.second = 13;
    note(400);
    values[2] = borrowed.first;
    values[3] = borrowed.second;
    note(500 + values[2]);
    note(600 + values[3]);
    values[4] = &borrowed == &first.payload;
    values[5] = &borrowed != &other.payload;
    values[6] = first.canary == -0x6123456789ABCDE0LL;
    values[7] = other.canary == 0x5123456789ABCDE0LL;
    note(700 + values[4] + values[5] + values[6] + values[7]);
    return values[0] + values[1] + values[2] + values[3];
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
extern "C" int lifetime_live() { return 0; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 9 ? values[index] : -1;
}
