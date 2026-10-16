// Capture-first mutable-reference candidate. The live count is protocol-only:
// Cell is trivial and this fixture owns no constructor/destructor resources.
static int events[64];
static int event_count;
static int event_overflow;
static int values[9];
static int last_alias;

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Cell {
    int first;
    int second;
};

static void mutate(Cell& dst, const Cell& src) noexcept
{
    dst.first = src.first + 3;
    note(100 + dst.first);
    dst.second = src.first + src.second;
    note(200 + dst.second);
    last_alias = &dst == &src;
    note(300 + last_alias);
}

extern "C" int fixture() noexcept
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;
    values[8] = 0; // Protocol only; there are no live resource units to count.

    Cell source = {5, 7};
    Cell destination = {11, 13};
    mutate(destination, source);
    values[0] = destination.first;
    values[1] = destination.second;
    values[4] = last_alias;

    mutate(destination, destination);
    values[2] = source.first;
    values[3] = source.second;
    values[5] = destination.first;
    values[6] = destination.second;
    values[7] = last_alias;
    return values[0] + values[1] + values[5] + values[6];
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
