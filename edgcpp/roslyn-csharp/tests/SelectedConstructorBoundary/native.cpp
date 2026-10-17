#include "../Fixtures/T5/selected-constructor-boundary.hpp"
#include <array>
#include <cstddef>
#include <cstdio>
#include <cstdlib>
#include <cstring>

extern "C" int fixture();
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
extern "C" int lifetime_live();
extern "C" int lifetime_event(int);
extern "C" int lifetime_value(int);
extern "C" int startup_calls();

namespace {
constexpr int value_count = 9;
constexpr int event_capacity = 64;

struct Observation {
    bool has_result;
    int result;
    int calls;
    int live;
    int overflow;
    int count;
    std::array<int, value_count> values;
    std::array<int, event_capacity> events;
};

Observation startup;
Observation first_call;
Observation second_call;
bool observer_installed;
bool observer_preceded_fixture_startup;
bool main_completed;
bool global_objects_distinct;
bool global_storage_aligned;
bool global_bytes_unchanged;
int first_serial;
int second_serial;

Observation observe(bool has_result, int result) noexcept
{
    Observation value{};
    value.has_result = has_result;
    value.result = result;
    value.calls = startup_calls();
    value.live = lifetime_live();
    value.overflow = lifetime_overflow();
    value.count = lifetime_count();
    if (value.count < 0 || value.count > event_capacity) std::abort();
    for (int index = 0; index < value_count; ++index) value.values[index] = lifetime_value(index);
    for (int index = 0; index < value.count; ++index) value.events[index] = lifetime_event(index);
    return value;
}

void print_observation(const Observation& value) noexcept
{
    std::fputs("{\"result\":", stdout);
    if (value.has_result) std::printf("%d", value.result);
    else std::fputs("null", stdout);
    std::printf(",\"calls\":%d,\"live\":%d,\"overflow\":%d,\"values\":[", value.calls, value.live, value.overflow);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::fputs("]}", stdout);
}

void report_after_actual_shutdown() noexcept
{
    if (!observer_installed || !observer_preceded_fixture_startup || !main_completed) std::abort();
    // No class member is read after global shutdown. These are independent
    // scalar/event/value objects and physical observations saved by main.
    const auto shutdown = observe(false, 0);
    if (startup.calls != 0 || first_call.calls != 1 || second_call.calls != 2 || shutdown.calls != 2 ||
        startup.overflow != 0 || first_call.overflow != 0 || second_call.overflow != 0 || shutdown.overflow != 0 ||
        startup.live != 0 || first_call.live != 0 || second_call.live != 0 || shutdown.live != 0 ||
        !global_objects_distinct || !global_storage_aligned || !global_bytes_unchanged) std::abort();
    std::fputs("{\"startup\":", stdout); print_observation(startup);
    std::fputs(",\"first\":", stdout); print_observation(first_call);
    std::fputs(",\"second\":", stdout); print_observation(second_call);
    std::fputs(",\"shutdown\":", stdout); print_observation(shutdown);
    std::printf(",\"physical\":{\"cellSize\":%zu,\"cellAlignment\":%zu,\"canaryOffset\":%zu,\"serialOffset\":%zu,\"kindOffset\":%zu,"
        "\"globalObjectsDistinct\":%s,\"globalStorageAligned\":%s,\"globalBytesUnchanged\":%s,\"firstSerial\":%d,\"secondSerial\":%d}}\n",
        sizeof(BoundaryCell), alignof(BoundaryCell), offsetof(BoundaryCell, canary), offsetof(BoundaryCell, serial), offsetof(BoundaryCell, kind),
        global_objects_distinct ? "true" : "false", global_storage_aligned ? "true" : "false", global_bytes_unchanged ? "true" : "false",
        first_serial, second_serial);
    if (std::fflush(stdout) != 0) std::abort();
}

__attribute__((constructor(101))) void install_native_report_observer() noexcept
{
    observer_preceded_fixture_startup = lifetime_count() == 0 && lifetime_overflow() == 0 &&
        lifetime_live() == 0 && startup_calls() == 0;
    if (std::atexit(report_after_actual_shutdown) != 0) std::abort();
    observer_installed = true;
}
}

int main()
{
    if (!observer_installed || !observer_preceded_fixture_startup) return 1;
    global_objects_distinct = &first != &second;
    global_storage_aligned = reinterpret_cast<std::size_t>(&first) % alignof(BoundaryCell) == 0 &&
        reinterpret_cast<std::size_t>(&second) % alignof(BoundaryCell) == 0;
    first_serial = first.serial;
    second_serial = second.serial;
    std::array<unsigned char, sizeof(BoundaryCell)> first_bytes;
    std::array<unsigned char, sizeof(BoundaryCell)> second_bytes;
    std::memcpy(first_bytes.data(), &first, sizeof(first));
    std::memcpy(second_bytes.data(), &second, sizeof(second));
    startup = observe(false, 0);
    first_call = observe(true, fixture());
    global_bytes_unchanged = std::memcmp(first_bytes.data(), &first, sizeof(first)) == 0 &&
        std::memcmp(second_bytes.data(), &second, sizeof(second)) == 0;
    second_call = observe(true, fixture());
    global_bytes_unchanged = global_bytes_unchanged && std::memcmp(first_bytes.data(), &first, sizeof(first)) == 0 &&
        std::memcmp(second_bytes.data(), &second, sizeof(second)) == 0;
    main_completed = true;
    // Both BoundaryCell objects have trivial destruction. No constructor or
    // destructor is called manually, and no ownership cleanup is synthesized.
    return 0;
}
