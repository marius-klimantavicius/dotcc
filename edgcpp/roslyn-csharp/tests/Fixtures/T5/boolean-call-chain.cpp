// Capture-first Boolean/member/base/reference composition. Pointer sentinels
// carry address bits only; no Opaque object is constructed or dereferenced.
// The zero-live slot is harness protocol, not a resource ownership proof.
static int events[64];
static int event_count;
static int event_overflow;
static int values[19];

static void note(int event)
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

struct Opaque {
    virtual int unused() const = 0;
    int payload;
};

struct EmptyTag {};

struct RefCountCarrier {
    Opaque* control;
};

struct Storage : EmptyTag {
    Opaque* pointer;
    RefCountCarrier refcount;

    explicit operator bool() const noexcept { return pointer != nullptr; }
};

struct DerivedStorage : Storage {};

using DerivedAlias = DerivedStorage;

struct Wrapper {
    DerivedAlias internal;

    explicit operator bool() const noexcept { return static_cast<bool>(internal); }
};

using WrapperAlias = Wrapper;

struct Owner {
    long long canary;
    mutable WrapperAlias payload;

    const WrapperAlias& view() const { return payload; }
};

extern "C" int fixture()
{
    event_count = 0;
    event_overflow = 0;
    for (int index = 0; index < 19; ++index) values[index] = 0;

    Owner first;
    Owner other;
    first.canary = -0x6123456789ABCDE0LL;
    other.canary = 0x5123456789ABCDE0LL;
    first.payload.internal.pointer = nullptr;
    first.payload.internal.refcount.control = reinterpret_cast<Opaque*>(&other.canary);
    other.payload.internal.pointer = reinterpret_cast<Opaque*>(&other.canary);
    other.payload.internal.refcount.control = reinterpret_cast<Opaque*>(&first.canary);
    note(100);

    const WrapperAlias& borrowed = first.view();
    const WrapperAlias& other_borrowed = other.view();
    values[0] = static_cast<bool>(borrowed);
    if (!borrowed) {
        values[1] = 1;
        note(201);
    } else {
        values[1] = 0;
        note(200);
    }
    values[2] = static_cast<bool>(other_borrowed);
    note(300 + values[2]);

    first.payload.internal.pointer = reinterpret_cast<Opaque*>(&first.canary);
    note(400);
    values[3] = static_cast<bool>(borrowed);
    if (!borrowed) {
        values[4] = 1;
        note(501);
    } else {
        values[4] = 0;
        note(500);
    }
    values[5] = &borrowed == &first.payload;
    values[6] = &borrowed != &other.payload;
    values[7] = &borrowed.internal == &first.payload.internal;
    values[8] = static_cast<const void*>(static_cast<const Storage*>(&borrowed.internal))
        == static_cast<const void*>(&borrowed.internal);
    values[9] = borrowed.internal.pointer == reinterpret_cast<Opaque*>(&first.canary);
    note(600 + values[3]);

    first.payload.internal.pointer = nullptr;
    note(700);
    values[10] = static_cast<bool>(borrowed);
    if (!borrowed) {
        values[11] = 1;
        note(801);
    } else {
        values[11] = 0;
        note(800);
    }
    values[12] = static_cast<bool>(other_borrowed);
    values[13] = other_borrowed.internal.pointer == reinterpret_cast<Opaque*>(&other.canary);
    values[14] = first.payload.internal.refcount.control == reinterpret_cast<Opaque*>(&other.canary);
    values[15] = other.payload.internal.refcount.control == reinterpret_cast<Opaque*>(&first.canary);
    values[16] = first.canary == -0x6123456789ABCDE0LL;
    values[17] = other.canary == 0x5123456789ABCDE0LL;
    note(900 + values[12]);
    return values[0] + values[2] + values[3] + values[10] + values[12];
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
    return index >= 0 && index < 19 ? values[index] : -1;
}
