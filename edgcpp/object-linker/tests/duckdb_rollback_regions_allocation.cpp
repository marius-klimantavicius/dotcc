// Real source-defined class allocators keep this language test independent
// of libc++'s heap implementation. Every acquired pool slot must be released.
static unsigned long long events;
static int failure;
static int occupied[4];
alignas(16) static unsigned char pool[4][64];
static unsigned long released_size;
static void event(int value) { events = events * 17 + (unsigned)value; }
static void *allocate(unsigned long size) {
    event(1);
    if (failure == 1 || size > 64) throw 1;
    for (int i = 0; i != 4; ++i) {
        if (!occupied[i]) { occupied[i] = 1; return pool[i]; }
    }
    throw 1;
}
static void release(void *pointer, unsigned long size, int marker) {
    event(marker); released_size = size;
    for (int i = 0; i != 4; ++i) {
        if (pointer == pool[i]) { occupied[i] = 0; return; }
    }
    event(15); // Observable incorrect target; no host allocator fallback.
}
struct Member {
    int value;
    Member() : value(12) { event(2); if (failure == 2) throw 2; }
    ~Member() { event(4); }
};
struct Ordinary {
    Member member;
    Ordinary() { event(3); if (failure == 3) throw 3; }
    ~Ordinary() { event(5); }
    static void *operator new(unsigned long size) { return allocate(size); }
    static void operator delete(void *pointer) noexcept { release(pointer, 0, 6); }
};
struct Sized {
    Member member;
    unsigned char payload[17];
    Sized() { event(3); if (failure == 3) throw 3; }
    ~Sized() { event(5); }
    static void *operator new(unsigned long size) { return allocate(size); }
    static void operator delete(void *pointer, unsigned long size) noexcept { release(pointer, size, 7); }
};
extern "C" unsigned long long duckdb_rollback_allocation(int scenario, int fail) {
    events = 0; failure = fail; released_size = 0;
    try {
        if (scenario == 0) { Ordinary *object = new Ordinary; delete object; }
        else if (scenario == 1) {
            // Keeps allocation rollback under a conditional source lifetime.
            Ordinary *object = fail ? new Ordinary : nullptr;
            if (object) delete object;
        } else { Sized *object = new Sized; delete object; }
    } catch (int) { event(9); }
    for (int i = 0; i != 4; ++i) if (occupied[i]) event(16);
    if (scenario == 2 && fail != 1 && released_size != sizeof(Sized)) event(14);
    return events;
}
