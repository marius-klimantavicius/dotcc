extern "C" int putchar(int);
extern "C" int atexit(void (*)());
static void before_object() { putchar('b'); }
static void after_object() { putchar('l'); }
static void registered_during_drain() { putchar('n'); }
static void register_during_drain() {
    putchar('r');
    atexit(registered_during_drain);
}
struct Interleaved {
    Interleaved() { putchar('C'); atexit(before_object); }
    ~Interleaved() { putchar('d'); }
};
static Interleaved object;
extern "C" int library_body() {
    putchar('M');
    atexit(after_object);
    atexit(register_during_drain);
    return 0;
}
int main() { return library_body(); }
