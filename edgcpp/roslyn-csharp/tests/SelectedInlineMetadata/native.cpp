#include "../Fixtures/T5/selected-inline-metadata.hpp"
#include <array>
#include <cstddef>
#include <cstdio>
#include <cstdlib>

extern "C" int fixture();
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
extern "C" int lifetime_live();
extern "C" int lifetime_event(int);
extern "C" int lifetime_value(int);
extern "C" int metadata_created();
extern "C" int metadata_destroyed();

namespace {
constexpr int value_count = 9;
constexpr int event_capacity = 64;

struct Observation {
    int result;
    int created;
    int destroyed;
    int live;
    int overflow;
    int count;
    std::array<int, value_count> values;
    std::array<int, event_capacity> events;
};

Observation observe(int result) noexcept
{
    Observation value{};
    value.result = result;
    value.created = metadata_created();
    value.destroyed = metadata_destroyed();
    value.live = lifetime_live();
    value.overflow = lifetime_overflow();
    value.count = lifetime_count();
    if (value.count < 0 || value.count > event_capacity) std::abort();
    for (int index = 0; index < value_count; ++index) value.values[index] = lifetime_value(index);
    for (int index = 0; index < value.count; ++index) value.events[index] = lifetime_event(index);
    if (value.live != 0 || value.overflow != 0 || value.created != value.destroyed || value.values[8] != 0) std::abort();
    return value;
}

void print_observation(const Observation& value) noexcept
{
    std::printf("{\"result\":%d,\"created\":%d,\"destroyed\":%d,\"live\":%d,\"overflow\":%d,\"values\":[",
        value.result, value.created, value.destroyed, value.live, value.overflow);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::fputs("]}", stdout);
}
}

int main()
{
    const auto first = observe(fixture());
    const auto second = observe(fixture());
    // Source scope exit already performed both Whole and Member destruction.
    // No constructor/destructor is called manually or registered by this tool.
    std::fputs("{\"runs\":[", stdout); print_observation(first);
    std::fputc(',', stdout); print_observation(second);
    std::printf("],\"physical\":{\"wholeSize\":%zu,\"wholeAlignment\":%zu,\"prefixOffset\":%zu,\"memberOffset\":%zu,"
        "\"memberSize\":%zu,\"memberAlignment\":%zu,\"memberIdOffset\":%zu},\"counterUnit\":\"Member\"}\n",
        sizeof(Whole), alignof(Whole), offsetof(Whole, prefix), offsetof(Whole, member),
        sizeof(Member), alignof(Member), offsetof(Member, id));
    return std::fflush(stdout) == 0 ? 0 : 1;
}
