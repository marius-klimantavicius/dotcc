/* Genuine C17 input. Parameters prevent the producer from folding the grouping
   cases; the observation/event protocol is shared with the independent oracle. */
static long long observations[30];
static int events[8];
static int event_count;

int precedence_add(int a, int b, int c) { return a + b * c; }
int grouping_sub(int a, int b, int c) { return a - (b - c); }
int grouping_div(int a, int b, int c) { return a / (b / c); }
int grouping_remainder(int a, int b, int c) { return a % (b % c); }
int grouping_left(int a, int b, int c) { return (a - b) - c; }
unsigned bit_precedence(unsigned a, unsigned b, unsigned c, unsigned d) {
    return (a & b) ^ (c | d);
}
int comparison_grouping(int a, int b, int c) { return (a < b) == (b < c); }
int unary_negative(int a) { return - -a; }
int unary_positive(int a) { return + +a; }
int unary_truth(int a) { return ! !a; }
unsigned char narrowing(unsigned a) { return (unsigned char)(a + 1U); }
unsigned wrap_add(unsigned a, unsigned b) { return a + b; }
double floating_right(double a, double b, double c) { return a + (b + c); }
double floating_left(double a, double b, double c) { return (a + b) + c; }
int pointer_value(int *p, int offset) { return *(p + offset); }
int pointer_roundtrip(int *p) { return *(&(*p)); }
unsigned long long target_shift(unsigned long long a, unsigned b, unsigned c) {
    return a << (b + c);
}
int loop_sum(int count) {
    int result = 0;
    for (int index = 0; index < count; ++index) result += index;
    return result;
}
static int mark(int event, int value) {
    events[event_count++] = event;
    return value;
}

int fixture(void) {
    event_count = 0;
    observations[0] = precedence_add(2, 3, 4);
    observations[1] = grouping_sub(30, 12, 5);
    observations[2] = grouping_div(60, 12, 3);
    observations[3] = grouping_remainder(20, 9, 4);
    observations[4] = grouping_left(30, 12, 5);
    observations[5] = bit_precedence(15U, 6U, 8U, 1U);
    observations[6] = comparison_grouping(1, 2, 3);
    observations[7] = comparison_grouping(2, 1, 3);
    observations[8] = unary_negative(-7);
    observations[9] = unary_positive(7);
    observations[10] = unary_truth(9);
    observations[11] = unary_truth(0);
    observations[12] = narrowing(511U);
    observations[13] = wrap_add(0xffffffffU, 2U);
    observations[14] = (long long)floating_right(1e16, -1e16, 1.0);
    observations[15] = (long long)floating_left(1e16, -1e16, 1.0);
    int values[3];
    values[0] = 10; values[1] = 20; values[2] = 30;
    observations[16] = pointer_value(values, 2);
    observations[17] = pointer_roundtrip(values + 1);
    observations[18] = (long long)target_shift(3ULL, 2U, 3U);
    observations[19] = 30 - (12 - mark(1, 5));
    observations[20] = (mark(2, 1), mark(3, 17));
    observations[21] = mark(4, 0) && mark(9, 1);
    observations[22] = mark(5, 1) || mark(9, 0);
    observations[23] = mark(6, 1) ? 19 : mark(9, 99);
    int value = 4;
    observations[24] = value++ + 3;
    observations[25] = value;
    observations[26] = event_count;
    observations[27] = events[0] * 100000 + events[1] * 10000 + events[2] * 1000
        + events[3] * 100 + events[4] * 10 + events[5];
    observations[28] = (long long)target_shift(3ULL, 0xffffffffU, 6U);
    observations[29] = loop_sum(5);
    return 30;
}
int expression_observation_count(void) { return 30; }
long long expression_observe(int index) { return observations[index]; }
int expression_event_count(void) { return event_count; }
int expression_event(int index) { return events[index]; }
