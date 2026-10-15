/* Explicit named-local snapshots keep copy timing observable without relying on
   the original aggregate.cpp source expression's producer/native disagreement. */
struct Aggregate { int first; int second; };
static Aggregate source;
static Aggregate destination;
static int events[16];
static int event_count;
static int values[6];

extern "C" {
static int mark(int event, int value) { events[event_count++] = event; return value; }
static Aggregate *select_destination(void) {
    mark(2, 0);
    source.first = 99;
    source.second = 101;
    return &destination;
}
static const Aggregate *select_source(void) { mark(1, 0); return &source; }
int sequence_count(void) { return event_count; }
int sequence_event(int index) { return events[index]; }
int sequence_value(int index) { return values[index]; }
int fixture(void) {
    event_count = 0;
    source.first = 7; source.second = 11;
    Aggregate snapshot = *select_source();
    *select_destination() = snapshot;
    values[0] = destination.first; values[1] = destination.second;
    source.first = 13; source.second = 17;
    snapshot = *select_source();
    *select_destination() = snapshot;
    values[2] = destination.first; values[3] = destination.second;
    values[4] = source.first; values[5] = source.second;
    return values[0] + values[1] + values[2] + values[3] + values[4] + values[5];
}
}
