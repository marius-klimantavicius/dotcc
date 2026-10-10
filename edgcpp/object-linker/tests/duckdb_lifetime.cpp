// Reduced forms of DuckDB's placement construction and allocator delegation.
// No C++ library declarations are needed to exercise the lowering contracts.
static unsigned long long events;
static int failure;
static void event(int value) { events = events * 17 + (unsigned)value; }

void *operator new(unsigned long, void *storage, int) { event(10); if (failure == 10) throw 10; return storage; }
void operator delete(void *, void *, int) noexcept { event(8); }

struct Argument {
    Argument() { event(1); if (failure == 1) throw 1; }
    ~Argument() { event(7); }
};
struct Member {
    Member() { event(2); if (failure == 2) throw 2; }
    ~Member() { event(6); }
};
struct Constructed {
    Member member;
    explicit Constructed(const Argument &) { event(3); if (failure == 3) throw 3; }
    ~Constructed() { event(5); }
};
struct Wrapped {
    Constructed *pointer;
    explicit Wrapped(Constructed *value) : pointer(value) { event(12); if (failure == 12) throw 12; }
    ~Wrapped() { event(13); pointer->~Constructed(); }
};
struct Aggregate {
    int value;
    ~Aggregate() { event(value); }
};
struct NullableOwner {
    int *pointer;
    constexpr NullableOwner(decltype(nullptr)) : pointer(nullptr) {}
    ~NullableOwner() { event(5); }
};
struct Delegated {
    Delegated() : Delegated(NullableOwner(nullptr)) { event(3); }
    explicit Delegated(NullableOwner) { event(2); if (failure == 2) throw 2; }
    ~Delegated() { event(4); }
};

extern "C" unsigned long long duckdb_lifetime_case(int scenario, int fail) {
    events = 0; failure = fail;
    try {
        Aggregate outer = {11};
        if (scenario == 0) {
            alignas(Constructed) unsigned char storage[sizeof(Constructed)];
            Constructed *value = new (storage, 0) Constructed(Argument());
            event(4);
            value->~Constructed();
        } else if (scenario == 1) {
            Aggregate initialized = {12};
            Aggregate uninitialized;
            uninitialized.value = 13;
            event(1);
            if (fail) throw 1;
        } else if (scenario == 2) {
            Delegated value;
            event(1);
            if (fail == 1) throw 1;
        } else {
            alignas(Constructed) unsigned char inner[sizeof(Constructed)];
            alignas(Wrapped) unsigned char outer[sizeof(Wrapped)];
            Wrapped *value = new (outer, 0) Wrapped(new (inner, 0) Constructed(Argument()));
            event(4);
            value->~Wrapped();
        }
    } catch (int) { event(9); }
    return events;
}
