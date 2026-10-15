/* One argument keeps native observations independent of unspecified ordering
   among distinct arguments. C++20 sequences the callee before that argument;
   the translator must obtain its actual plan from captured operand flags. */
static int events[16];
static int event_count;
extern "C" {
static int mark(int event, int value) { events[event_count++] = event; return value; }
static int target(int value) { mark(3, 0); return value + 1; }
static int (*get_target(void))(int) { mark(1, 0); return &target; }
int sequence_count(void) { return event_count; }
int sequence_event(int index) { return events[index]; }
int sequence_value(int index) { return index == 0 ? event_count : -1; }
int fixture(void) {
    event_count = 0;
    int first = get_target()(mark(2, 7));
    int second = get_target()(mark(2, 9));
    return first + second;
}
}
