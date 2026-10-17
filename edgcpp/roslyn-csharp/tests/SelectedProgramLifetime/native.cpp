#include <array>
#include <cstdio>
#include <cstdlib>

extern "C" int fixture();
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
extern "C" int lifetime_live();
extern "C" int lifetime_event(int);
extern "C" int lifetime_value(int);
extern "C" int startup_constructed();
extern "C" int startup_destroyed();
extern "C" int startup_calls();

namespace {
constexpr int value_count = 9;
constexpr int event_capacity = 64;

struct Observation {
    bool has_result;
    int result;
    int count;
    int overflow;
    int live;
    int constructed;
    int destroyed;
    int calls;
    std::array<int, value_count> values;
    std::array<int, event_capacity> events;
};

// These native tooling records have no dynamic construction or destructor.
Observation startup;
Observation first_call;
Observation second_call;
bool observer_installed;
bool observer_preceded_fixture_startup;
bool main_completed;

Observation observe(bool has_result, int result) noexcept
{
    Observation value{};
    value.has_result = has_result;
    value.result = result;
    value.count = lifetime_count();
    value.overflow = lifetime_overflow();
    value.live = lifetime_live();
    value.constructed = startup_constructed();
    value.destroyed = startup_destroyed();
    value.calls = startup_calls();
    if (value.count < 0 || value.count > event_capacity) {
        std::fputs("Native global startup event capacity exceeded.\n", stderr);
        std::abort();
    }
    for (int index = 0; index < value_count; ++index) value.values[index] = lifetime_value(index);
    for (int index = 0; index < value.count; ++index) value.events[index] = lifetime_event(index);
    return value;
}

void print_observation(const Observation& value) noexcept
{
    std::fputs("{\"result\":", stdout);
    if (value.has_result) std::printf("%d", value.result);
    else std::fputs("null", stdout);
    std::printf(",\"live\":%d,\"constructed\":%d,\"destroyed\":%d,\"calls\":%d,\"overflow\":%d,\"values\":[",
        value.live, value.constructed, value.destroyed, value.calls, value.overflow);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::fputs("]}", stdout);
}

void report_after_actual_destruction() noexcept
{
    if (!observer_installed || !observer_preceded_fixture_startup || !main_completed) {
        std::fputs("Native global observer did not bracket the real fixture lifetime.\n", stderr);
        std::abort();
    }
    const auto shutdown = observe(false, 0);
    if (startup.overflow != 0 || first_call.overflow != 0 || second_call.overflow != 0 || shutdown.overflow != 0 ||
        startup.calls != 0 || first_call.calls != 1 || second_call.calls != 2 || shutdown.calls != 2 ||
        startup.constructed != 2 || startup.destroyed != 0 || startup.live != 2 ||
        shutdown.constructed != startup.constructed || shutdown.destroyed != startup.constructed || shutdown.live != 0) {
        std::fputs("Native actual global construction/destruction protocol failed.\n", stderr);
        std::abort();
    }
    std::fputs("{\"startup\":", stdout); print_observation(startup);
    std::fputs(",\"first\":", stdout); print_observation(first_call);
    std::fputs(",\"second\":", stdout); print_observation(second_call);
    std::fputs(",\"shutdown\":", stdout); print_observation(shutdown);
    std::fputs("}\n", stdout);
    if (std::fflush(stdout) != 0) std::abort();
}

// Native-only observer registration. Default-priority fixture global C++
// construction follows this priority101 routine. Its actual destructor
// registrations therefore precede this observer in reverse shutdown order.
// No fixture destructor is called manually by this harness.
__attribute__((constructor(101))) void install_native_report_observer() noexcept
{
    observer_preceded_fixture_startup = lifetime_count() == 0 && lifetime_overflow() == 0 && lifetime_live() == 0 &&
        startup_constructed() == 0 && startup_destroyed() == 0 && startup_calls() == 0;
    if (std::atexit(report_after_actual_destruction) != 0) {
        std::fputs("Native report observer registration failed.\n", stderr);
        std::abort();
    }
    observer_installed = true;
}
}

int main()
{
    if (!observer_installed || !observer_preceded_fixture_startup) return 1;
    startup = observe(false, 0);
    first_call = observe(true, fixture());
    second_call = observe(true, fixture());
    main_completed = true;
    // Normal return triggers the actual native C++ destructor registrations.
    // The earlier native tooling observer emits the final report afterwards.
    return 0;
}
