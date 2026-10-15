/* RHS-before-LHS, single receiver capture, stored bitfield values, and lvalues.
   All source operations are defined for the selected GNU target. */
struct Bits { signed signed_part : 5; unsigned part : 4; };
static Bits bits;
static short cell;
static int events[32];
static int event_count;
static int values[11];

extern "C" {
static int mark(int event, int value) {
    events[event_count++] = event;
    return value;
}
static short *left(void) { mark(1, 0); return &cell; }
static int right(void) { mark(2, 0); cell = 20; return 3; }
static Bits *receiver(void) { mark(6, 0); return &bits; }
int sequence_count(void) { return event_count; }
int sequence_event(int index) { return events[index]; }
int sequence_value(int index) { return values[index]; }

int fixture(void) {
    event_count = 0;
    cell = 4;
    values[0] = (*left() += right());
    values[1] = (receiver()->signed_part = mark(5, 18));
    values[2] = ++receiver()->signed_part;
    values[3] = receiver()->signed_part++;
    values[4] = (receiver()->part = mark(7, 31));
    values[5] = ++receiver()->part;
    values[6] = receiver()->part++;
    int x = 4, y = 6;
    (mark(8, 0) ? x : y) = mark(9, 22);
    (mark(10, 0), x) += mark(11, 2);
    values[7] = x;
    values[8] = y;
    values[9] = bits.signed_part;
    values[10] = bits.part;
    int total = 0;
    for (int index = 0; index < 11; ++index) total += values[index];
    return total;
}
}
