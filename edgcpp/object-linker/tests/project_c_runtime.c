typedef __builtin_va_list arguments;

struct State { volatile int value; volatile unsigned long wide; };
static struct State state = { 4, 9 };
struct Plain { int value; };
static volatile struct Plain qualified_state = { 6 };

static int volatile_read(volatile int *pointer) { return *pointer; }

static int read_pair(arguments source) {
    arguments copy;
    __builtin_va_copy(copy, source);
    int first = __builtin_va_arg(copy, int);
    int second = __builtin_va_arg(copy, int);
    __builtin_va_end(copy);
    return first + second;
}

static int sum(int count, ...) {
    arguments list;
    __builtin_va_start(list, count);
    int pair = read_pair(list);
    int total = 0;
    for (int i = 0; i < count; ++i) total += __builtin_va_arg(list, int);
    __builtin_va_end(list);
    return total + pair;
}

static int mixed(int marker, ...) {
    arguments list;
    __builtin_va_start(list, marker);
    double number = __builtin_va_arg(list, double);
    unsigned long wide = __builtin_va_arg(list, unsigned long);
    int *pointer = __builtin_va_arg(list, int *);
    int result = number == 2.5 && wide == 123456789UL && *pointer == 19;
    __builtin_va_end(list);
    return result;
}

int main(void) {
    volatile int local = 3;
    state.value = state.value + local;
    state.wide += 4;
    if (state.value != 7 || state.wide != 13) return 1;
    if (volatile_read(&local) != 3 || qualified_state.value != 6) return 5;
    if (sum(4, 1, 2, 3, 4) != 13) return 2;
    int value = 19;
    int *volatile pointer = &value;
    if (*pointer != 19) return 6;
    if (!mixed(0, 2.5, 123456789UL, &value)) return 3;
    int index = 0;
again:
    index += 1;
    if (index < 3) goto again;
    if ((index = 5, index += 2, index) != 7) return 4;
    return 0;
}
