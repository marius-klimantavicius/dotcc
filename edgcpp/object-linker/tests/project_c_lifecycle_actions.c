void abort(void);
static int trace;
static int seeded = 9;
__attribute__((constructor(0))) static void platform_start(void) {
    if (trace || seeded != 9) abort();
    trace = 1;
}
__attribute__((constructor(101))) static void library_start(void) {
    if (trace != 1) abort();
    trace = 12;
}
__attribute__((constructor)) static void user_start(void) {
    if (trace != 12) abort();
    trace = 123;
}
__attribute__((destructor)) static void user_stop(void) {
    if (trace != 123) abort();
    trace = 1234;
}
__attribute__((destructor(101))) static void library_stop(void) {
    if (trace != 1234) abort();
    trace = 12345;
}
__attribute__((destructor(0))) static void platform_stop(void) {
    if (trace != 12345) abort();
    trace = 123456;
}
int read_trace(void) { return trace; }
int main(void) { return trace != 123; }
