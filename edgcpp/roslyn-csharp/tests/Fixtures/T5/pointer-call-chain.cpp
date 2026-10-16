#include "pointer-call-chain.hpp"

static int events[64];
static int event_count;
static int event_overflow;
static int values[32];
static int expect_count;
static int guard_count;
static int missing_count;

static void note(int event)
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

FieldCode CellInput::read() const { return kind; }

static long expect_argument(bool missing)
{
    ++expect_count;
    note(2000 + static_cast<int>(missing));
    return static_cast<long>(missing);
}

// This is an observable, defined fixture guard. It is not an implementation of
// any unresolved project guard and does not throw or claim a noreturn contract.
static void record_guard(bool missing)
{
    ++guard_count;
    if (missing) ++missing_count;
    note(3000 + static_cast<int>(missing));
}

struct PointerStorage;

struct PointerAccess {
    CellInput* base_pointer() const noexcept;
    CellInput* operator->() const noexcept;
};

struct RefCountCarrier {
    CellInput* cookie;
};

struct PointerStorage : PointerAccess {
    CellInput* pointer;
    RefCountCarrier refcount;

    CellInput* get() const noexcept { return pointer; }
    explicit operator bool() const noexcept { return pointer != nullptr; }
};

CellInput* PointerAccess::base_pointer() const noexcept
{
    return static_cast<const PointerStorage*>(this)->get();
}

CellInput* PointerAccess::operator->() const noexcept
{
    return base_pointer();
}

struct DerivedStorage : PointerStorage {};
using DerivedAlias = DerivedStorage;

struct PointerProxy {
    DerivedAlias internal;
    bool use_direct;

    explicit operator bool() const noexcept { return static_cast<bool>(internal); }

    CellInput* operator->() const
    {
        if (use_direct) {
            note(4000);
            CellInput* const pointer = internal.get();
            record_guard(__builtin_expect(expect_argument(!pointer), 0L));
            return pointer;
        }
        note(4001);
        return static_cast<const PointerAccess&>(internal).operator->();
    }
};

using ProxyAlias = PointerProxy;

struct Owner {
    long long canary;
    mutable ProxyAlias payload;

    const ProxyAlias& view() const { return payload; }
};

extern "C" int fixture(CellInput* first, CellInput* second)
{
    event_count = 0;
    event_overflow = 0;
    expect_count = 0;
    guard_count = 0;
    missing_count = 0;
    for (int index = 0; index < 32; ++index) values[index] = 0;

    Owner owner;
    Owner other;
    owner.canary = -0x6123456789ABCDE0LL;
    other.canary = 0x5123456789ABCDE0LL;
    owner.payload.internal.pointer = nullptr;
    owner.payload.internal.refcount.cookie = second;
    owner.payload.use_direct = true;
    other.payload.internal.pointer = second;
    other.payload.internal.refcount.cookie = first;
    other.payload.use_direct = false;
    note(100);

    const ProxyAlias& borrowed = owner.view();
    const ProxyAlias& other_borrowed = other.view();
    CellInput* selected = borrowed.operator->();
    values[0] = selected == nullptr;
    values[1] = !borrowed;
    note(200);

    owner.payload.internal.pointer = first;
    selected = borrowed.operator->();
    values[2] = selected == first;
    values[3] = static_cast<int>(borrowed->read());
    values[4] = static_cast<bool>(borrowed);
    note(300);

    owner.payload.use_direct = false;
    selected = borrowed.operator->();
    values[5] = selected == first;
    values[6] = static_cast<int>(selected->read());
    note(400);

    owner.payload.internal.pointer = second;
    selected = borrowed.operator->();
    values[7] = selected == second;
    values[8] = static_cast<int>(borrowed->read());
    note(700);

    owner.payload.use_direct = true;
    selected = borrowed.operator->();
    values[9] = selected == second;
    values[10] = static_cast<int>(selected->read());
    note(800);

    owner.payload.use_direct = false;
    owner.payload.internal.pointer = nullptr;
    selected = borrowed.operator->();
    values[11] = selected == nullptr;
    values[12] = !borrowed;
    note(900);

    selected = other_borrowed.operator->();
    values[13] = selected == second;
    values[14] = static_cast<int>(selected->read());
    note(1000);

    values[15] = &borrowed == &owner.payload;
    values[16] = &borrowed != &other.payload;
    values[17] = &borrowed.internal == &owner.payload.internal;
    values[18] = static_cast<const void*>(static_cast<const PointerAccess*>(&borrowed.internal))
        == static_cast<const void*>(&borrowed.internal);
    values[19] = borrowed.internal.get() == nullptr;
    values[20] = owner.payload.internal.refcount.cookie == second;
    values[21] = other.payload.internal.refcount.cookie == first;
    values[22] = owner.canary == -0x6123456789ABCDE0LL;
    values[23] = other.canary == 0x5123456789ABCDE0LL;
    values[24] = expect_count;
    values[25] = guard_count;
    values[26] = missing_count;
    values[27] = first != second;
    values[28] = static_cast<int>(first->read());
    values[29] = static_cast<int>(second->read());
    values[30] = expect_count == guard_count;
    values[31] = values[28] == values[3] && values[29] == values[8];
    return values[3] + values[8] + values[10] + values[14];
}

extern "C" int lifetime_count() { return event_count; }
extern "C" int lifetime_overflow() { return event_overflow; }
extern "C" int lifetime_live() { return 0; }
extern "C" int lifetime_event(int index)
{
    return index >= 0 && index < event_count ? events[index] : -1;
}
extern "C" int lifetime_value(int index)
{
    return index >= 0 && index < 32 ? values[index] : -1;
}
