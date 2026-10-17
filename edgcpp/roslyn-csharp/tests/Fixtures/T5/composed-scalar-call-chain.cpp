#include "composed-scalar-call-chain.hpp"

static int events[64];
static int event_count;
static int event_overflow;
static int values[24];
static int expect_count;
static int guard_count;
static int missing_count;

static void note(int event)
{
    if (event_count < 64) events[event_count++] = event;
    else event_overflow = 1;
}

ScalarCode ScalarInput::read() const { return kind; }

static long expect_argument(bool missing)
{
    ++expect_count;
    note(2000 + static_cast<int>(missing));
    return static_cast<long>(missing);
}

// Observable fixture behavior, not a replacement for any project guard.
static void record_guard(bool missing)
{
    ++guard_count;
    if (missing) ++missing_count;
    note(3000 + static_cast<int>(missing));
}

StoredScalarElement* ScalarStorage::get() const noexcept { return pointer; }
ScalarStorage::operator bool() const noexcept { return pointer != nullptr; }

ScalarElement* ScalarAccess::base_pointer() const noexcept
{
    return static_cast<const ScalarStorage*>(this)->get();
}

ScalarElement* ScalarAccess::operator->() const noexcept { return base_pointer(); }

ScalarProxy::operator bool() const noexcept { return static_cast<bool>(internal); }

ScalarInput* ScalarProxy::operator->() const
{
    if (use_direct) {
        note(4000);
        StoredScalarElement* const pointer = internal.get();
        record_guard(__builtin_expect(expect_argument(!pointer), 0L));
        return pointer;
    }
    note(4001);
    return static_cast<const ScalarAccess&>(internal).operator->();
}

const ScalarProxyAlias& ScalarOwner::view() const { return payload; }

ScalarCode ScalarOwner::composed() const
{
    const ScalarProxyAlias& borrowed = view();
    if (!borrowed) return ScalarCode::missing;
    return borrowed->read();
}

extern "C" int fixture(const ScalarOwner* first, const ScalarOwner* second,
    ScalarInput* first_input, ScalarInput* second_input)
{
    event_count = 0;
    event_overflow = 0;
    expect_count = 0;
    guard_count = 0;
    missing_count = 0;
    for (int index = 0; index < 24; ++index) values[index] = 0;
    note(100);

    values[0] = static_cast<int>(first->composed());
    note(200);
    values[1] = static_cast<int>(second->composed());
    note(300);
    values[2] = static_cast<int>(first->composed());
    note(400);
    values[3] = static_cast<int>(second->composed());
    note(500);

    const ScalarProxyAlias& first_borrowed = first->view();
    const ScalarProxyAlias& second_borrowed = second->view();
    ScalarInput* first_pointer = first_borrowed.internal.get();
    ScalarInput* second_pointer = second_borrowed.internal.get();
    values[4] = static_cast<bool>(first_borrowed);
    values[5] = static_cast<bool>(second_borrowed);
    values[6] = first_pointer == nullptr;
    values[7] = second_pointer == nullptr;
    values[8] = first_pointer == first_input;
    values[9] = first_pointer == second_input;
    values[10] = second_pointer == first_input;
    values[11] = second_pointer == second_input;
    values[12] = first_pointer != second_pointer;
    values[13] = &first_borrowed != &second_borrowed;
    values[14] = first_borrowed.internal.refcount.cookie == second_input;
    values[15] = second_borrowed.internal.refcount.cookie == first_input;
    values[16] = expect_count;
    values[17] = guard_count;
    values[18] = missing_count;
    values[19] = static_cast<int>(first_input->read());
    values[20] = static_cast<int>(second_input->read());
    values[21] = values[0] == values[2];
    values[22] = values[1] == values[3];
    values[23] = first_input != second_input;
    return values[0] + values[1];
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
    return index >= 0 && index < 24 ? values[index] : -1;
}
