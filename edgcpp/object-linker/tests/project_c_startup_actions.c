void abort(void);
static int trace;
static int seeded = 9;
static int *seed_pointer = &seeded;

__attribute__((constructor(0))) static void platform_start(void) {
    if (trace || seeded != 9 || seed_pointer != &seeded || *seed_pointer != 9)
        abort();
    trace = 1;
}
__attribute__((constructor(101))) static void first_library_start(void) {
    if (trace != 1) abort();
    trace = 12;
}
__attribute__((constructor(101))) static void second_library_start(void) {
    if (trace != 12) abort();
    trace = 123;
}
__attribute__((constructor)) static void user_start(void) {
    if (trace != 123) abort();
    trace = 1234;
}
int read_startup_trace(void) { return trace; }
int main(void) { return trace != 1234; }
