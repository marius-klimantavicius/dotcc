/* Typed prvalue va_arg and a selected volatile binary64 formal. */
typedef __builtin_va_list Cursor;
static int slot = 23;
static long long observations[13];
static int events[8];
static int event_count;
static int cursor_selections;
static int mark(int event) { events[event_count++] = event; return 0; }
static Cursor *selected_cursor(Cursor *cursor) { ++cursor_selections; return cursor; }
static double qualified_formal(volatile double value) { return value + 0.5; }
static double ordinary_caller(double value) { return qualified_formal(value); }

static int consume(int selected, ...) {
    Cursor cursor;
    __builtin_va_start(cursor, selected);
    volatile int first = 0;
    first = (mark(1), __builtin_va_arg(*selected_cursor(&cursor), int));
    unsigned int second = (mark(2), __builtin_va_arg(cursor, unsigned int));
    volatile double fraction = (mark(3), __builtin_va_arg(cursor, double));
    int third = selected ? (mark(4), __builtin_va_arg(cursor, int))
                         : (mark(9), __builtin_va_arg(cursor, int));
    int *pointer = selected ? (mark(5), __builtin_va_arg(cursor, int *))
                            : (mark(9), __builtin_va_arg(cursor, int *));
    int last = (mark(6), __builtin_va_arg(cursor, int));
    __builtin_va_end(cursor);
    observations[0] = first;
    observations[1] = second;
    observations[2] = (long long)(fraction * 4.0);
    observations[3] = third;
    observations[4] = *pointer;
    observations[5] = last;
    observations[6] = cursor_selections;
    observations[7] = event_count;
    observations[8] = events[0] * 100000 + events[1] * 10000 + events[2] * 1000
                      + events[3] * 100 + events[4] * 10 + events[5];
    observations[11] = pointer == &slot;
    observations[12] = sizeof(Cursor);
    return first + (int)fraction + third + *pointer + last;
}
int fixture(void) {
    event_count = 0; cursor_selections = 0;
    observations[9] = consume(1, 7, 0x80000001U, 1.5, 11, &slot, 13);
    observations[10] = (long long)(ordinary_caller(1.5) * 4.0);
    return 13;
}
int last_four_count(void) { return 13; }
long long last_four_observe(int index) { return observations[index]; }
int last_four_event_count(void) { return event_count; }
int last_four_event(int index) { return events[index]; }
