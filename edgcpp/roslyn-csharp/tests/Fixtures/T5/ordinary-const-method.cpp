// Capture-first ordinary const method. Zero live/unused observation slots are
// harness protocol only; this trivial Cell owns no counted resources.
static int events[64];
static int event_count;
static int event_overflow;
static int values[9];

static void note(int event) noexcept
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Cell {
    unsigned char kind;
    unsigned char read() const { return kind; }
};

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 9; ++index) values[index] = 0;

    Cell cell;
    cell.kind = 5;
    note(10 + cell.kind);
    values[0] = cell.read();
    note(100 + values[0]);
    cell.kind = 201;
    note(20 + cell.kind);
    values[1] = cell.read();
    note(200 + values[1]);
    values[2] = cell.kind;
    return values[0] + values[1];
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
