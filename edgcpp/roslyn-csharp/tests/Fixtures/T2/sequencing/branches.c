/* Genuine application input. The independent oracle is native-oracle.c. */
static int events[32];
static int event_count;

static int mark(int event, int value) {
    events[event_count++] = event;
    return value;
}

int sequence_count(void) { return event_count; }
int sequence_event(int index) { return events[index]; }
int sequence_value(int index) { return index == 0 ? event_count : -1; }

int fixture(void) {
    event_count = 0;
    int a = (mark(1, 2), mark(2, 7));
    int b = mark(3, 0) && mark(90, 1);
    int c = mark(4, 1) || mark(91, 0);
    int d = mark(5, 1) ? mark(6, 11) : mark(92, 99);
    int e = mark(7, 0) ? mark(93, 99) : (mark(8, 13), mark(9, 17));
    int f = mark(10, 1) && mark(11, 3);
    int g = mark(12, 0) || mark(13, 4);
    int sum = 0;
    for (int i = 0; mark(20 + i, i < 3); (mark(30 + i, i), ++i)) {
        if (i == 1) continue;
        sum += i;
    }
    do { ++sum; } while (mark(40, 0));
    return a + b + c + d + e + f + g + sum;
}
