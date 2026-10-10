// No standard library dependencies: all throws use the owned provider.
static unsigned long long events;
static int next_id;
static int failure;
static int cleanup_failure;
static void event(int value) { events = events * 17 + (unsigned)value; }
struct Element {
    int id;
    Element() : id(++next_id) { event(id); if (failure == id) throw id; }
    Element(const Element &other) : id(++next_id) { event(id); if (failure == id) throw id; }
    ~Element() noexcept(false) { event(id + 8); if (cleanup_failure == id) throw id; if (failure == id + 8) { failure = 0; throw id; } }
};
struct MemberArray {
    Element values[3];
    MemberArray() { event(7); if (failure == 7) throw 7; }
    ~MemberArray() noexcept(false) { event(8); }
};
extern "C" unsigned long long array_cleanup_case(int scenario, int fail) {
    events = 0; next_id = 0; failure = fail;
    try {
        if (scenario == 0) { Element array[3]; event(6); if (fail == 6) throw 6; }
        else if (scenario == 1) { MemberArray array; event(6); if (fail == 6) throw 6; }
        else if (scenario == 2) { Element array[2][2]; event(6); if (fail == 6) throw 6; }
        else if (scenario == 3) { Element array[1]; event(6); if (fail == 6) throw 6; }
        else { MemberArray original; MemberArray copy = original; event(6); if (fail == 6) throw 6; }
    } catch (int error) { event(16); }
    return events;
}
struct VirtualRoot {
    int value;
    VirtualRoot() : value(3) { event(1); if (failure == 1) throw 1; }
    ~VirtualRoot() { event(value); }
};
struct VirtualChild : virtual VirtualRoot {
    Element member;
    VirtualChild() { value = 4; event(5); if (failure == 5) throw 5; }
    VirtualChild(int) : VirtualChild() { event(12); if (failure == 12) throw 12; }
    ~VirtualChild() noexcept(false) { event(6); }
};
struct VirtualMost : VirtualChild {
    int padding; // Moves VirtualRoot relative to the standalone VirtualChild.
    VirtualMost() : padding(99) { value = 7; event(8); if (failure == 8) throw 8; }
    ~VirtualMost() noexcept(false) { event(9); }
};
struct VirtualDelegated : VirtualChild {
    int padding;
    VirtualDelegated() : VirtualChild(0), padding(77) { event(14); if (failure == 14) throw 14; }
    ~VirtualDelegated() noexcept(false) { event(15); }
};
extern "C" unsigned long long vtt_cleanup_case(int scenario, int fail) {
    events = 0; next_id = 1; failure = fail;
    try {
        if (scenario == 0) { VirtualChild object; event(10); if (failure == 13) throw 13; }
        else if (scenario == 1) { VirtualMost object; event(10); if (failure == 13) throw 13; }
        else { VirtualDelegated object; event(10); if (failure == 13) throw 13; }
    } catch (int error) { event(16); }
    return events;
}

extern "C" void array_cleanup_terminate(int scenario) {
    events = 0; next_id = 0; cleanup_failure = 1;
    failure = scenario == 1 ? 3 : scenario == 2 ? 11 : 0;
    Element array[3];
    if (scenario == 0) throw 99;
}
