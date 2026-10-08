/* Equal-priority ties intentionally follow EDG's emitted routine order.
   Compare this fixture with stock EDG C output, not an unrelated compiler's
   chosen declaration ordering: GCC on the original source gives 12, while
   EDG's C backend declares second before first and therefore gives 21. */
static int trace;
static void first(void) __attribute__((constructor(101)));
__attribute__((constructor(101))) static void second(void) {
    trace = trace * 10 + 2;
}
static void first(void) {
    trace = trace * 10 + 1;
}
int read_startup_order(void) { return trace; }
int main(void) { return trace != 21; }
