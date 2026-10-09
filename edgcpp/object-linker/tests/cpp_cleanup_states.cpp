// Ordinary C++ destruction and constructor rollback; the host supplies throws.
extern "C" void cleanup_throw_point(int point);
static unsigned long long trace_value;
static void record(int digit) { trace_value = trace_value * 10 + digit; }
extern "C" void cleanup_reset() { trace_value = 0; }
extern "C" unsigned long long cleanup_trace() { return trace_value; }
struct Item {
    int digit;
    Item(int value) : digit(value) { record(digit); cleanup_throw_point(digit); }
    ~Item() noexcept(false) { record(digit); }
};
extern "C" void cleanup_locals() {
    Item first(1);
    { Item second(2); cleanup_throw_point(3); record(7); }
}
struct Base {
    Base(const Item &) { record(4); cleanup_throw_point(4); }
    ~Base() noexcept(false) { record(5); }
};
struct Derived : Base {
    Derived() : Base(Item(3)) { record(6); cleanup_throw_point(6); }
    ~Derived() noexcept(false) { record(7); }
};
extern "C" void cleanup_constructor() {
    Derived value;
    record(8);
    cleanup_throw_point(8);
}

struct PayloadBase {
    int payload;
    PayloadBase() : payload(9) { record(4); cleanup_throw_point(4); }
    ~PayloadBase() noexcept(false) { record(5); }
};
struct MemberOwner : PayloadBase {
    Item member;
    MemberOwner() : member(2) { record(6); cleanup_throw_point(6); }
    ~MemberOwner() noexcept(false) { record(7); }
};
extern "C" void cleanup_members() {
    MemberOwner value;
    record(8);
    cleanup_throw_point(8);
}

static int read_item(const Item &value) { return value.digit; }
static int return_item_digit() { return read_item(Item(3)); }
extern "C" void cleanup_temporary_result() {
    int result = return_item_digit();
    record(result);
}

// The destructor definition follows the delegating constructor. Its signature
// must be captured at its own finalized pre-lowering boundary.
struct Delegating {
    Delegating();
    Delegating(int);
    ~Delegating() noexcept(false);
};
Delegating::Delegating() : Delegating(1) { record(2); cleanup_throw_point(2); }
Delegating::Delegating(int) { record(1); cleanup_throw_point(1); }
Delegating::~Delegating() noexcept(false) { record(3); }
extern "C" void cleanup_delegating() {
    Delegating value;
    record(8);
    cleanup_throw_point(8);
}
