extern void host_startup_step(int);
int startup_trace;
int read_startup_trace(void) { return startup_trace; }
__attribute__((constructor(0))) static void first(void) {
    startup_trace = 1;
    host_startup_step(1);
}
__attribute__((constructor(101))) static void second(void) {
    startup_trace = startup_trace * 10 + 2;
    host_startup_step(2);
}
__attribute__((constructor)) static void third(void) {
    startup_trace = startup_trace * 10 + 3;
    host_startup_step(3);
}
int main(void) { return startup_trace == 123 ? 0 : 1; }
