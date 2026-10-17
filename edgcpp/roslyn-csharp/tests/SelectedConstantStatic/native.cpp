#include "../Fixtures/T5/selected-constant-static.hpp"
#include <array>
#include <cstdio>
#include <cstdlib>
#include <cstring>

extern "C" int fixture();
extern "C" int lifetime_count();
extern "C" int lifetime_overflow();
extern "C" int lifetime_live();
extern "C" int lifetime_event(int);
extern "C" int lifetime_value(int);

namespace {
constexpr int value_count = 64;
constexpr int event_capacity = 64;
constexpr int byte_count = 50;

struct Observation {
    int result;
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
    value.live = lifetime_live();
    value.overflow = lifetime_overflow();
    value.count = lifetime_count();
    if (value.count < 0 || value.count > event_capacity) std::abort();
    for (int index = 0; index < value_count; ++index) value.values[index] = lifetime_value(index);
    for (int index = 0; index < value.count; ++index) value.events[index] = lifetime_event(index);
    if (value.live != 0 || value.overflow != 0) std::abort();
    return value;
}

void print_observation(const Observation& value) noexcept
{
    std::printf("{\"result\":%d,\"live\":%d,\"overflow\":%d,\"values\":[", value.result, value.live, value.overflow);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::fputs("]}", stdout);
}

void print_bytes(const std::array<unsigned char, byte_count>& bytes) noexcept
{
    std::fputc('[', stdout);
    for (int index = 0; index < byte_count; ++index)
        std::printf("%s%u", index == 0 ? "" : ",", static_cast<unsigned int>(bytes[index]));
    std::fputc(']', stdout);
}
}

int main()
{
    const char* shared_a = selected_constant_a();
    const char* shared_b = selected_constant_b();
    const char* different_a = selected_different_a();
    const char* different_b = selected_different_b();
    std::array<unsigned char, byte_count> shared_bytes;
    std::array<unsigned char, byte_count> different_bytes;
    std::memcpy(shared_bytes.data(), shared_a, byte_count);
    std::memcpy(different_bytes.data(), different_a, byte_count);
    const auto first = observe(fixture());
    const auto second = observe(fixture());
    const bool shared_across_tus = shared_a == shared_b;
    const bool different_across_tus = different_a == different_b;
    const bool shared_stable = shared_a == selected_constant_a() && shared_b == selected_constant_b();
    const bool different_stable = different_a == selected_different_a() && different_b == selected_different_b();
    const bool named_arrays_distinct = shared_a != different_a && shared_b != different_b;
    const bool unchanged = std::memcmp(shared_bytes.data(), selected_constant_a(), byte_count) == 0 &&
        std::memcmp(shared_bytes.data(), selected_constant_b(), byte_count) == 0 &&
        std::memcmp(different_bytes.data(), selected_different_a(), byte_count) == 0 &&
        std::memcmp(different_bytes.data(), selected_different_b(), byte_count) == 0;
    const bool contents_equal = std::memcmp(shared_a, shared_b, byte_count) == 0 &&
        std::memcmp(shared_a, different_a, byte_count) == 0 && std::memcmp(shared_a, different_b, byte_count) == 0;
    const bool terminated = shared_a[49] == 0 && shared_b[49] == 0 && different_a[49] == 0 && different_b[49] == 0;
    if (!shared_across_tus || !different_across_tus || !shared_stable || !different_stable ||
        !named_arrays_distinct || !unchanged || !contents_equal || !terminated) std::abort();
    std::fputs("{\"runs\":[", stdout); print_observation(first);
    std::fputc(',', stdout); print_observation(second);
    std::printf("],\"storage\":{\"byteCount\":%d,\"sharedAcrossTus\":%s,\"differentAcrossTus\":%s,"
        "\"sharedStableAcrossCalls\":%s,\"differentStableAcrossCalls\":%s,\"namedArraysDistinct\":%s,"
        "\"allBytesUnchanged\":%s,\"allFourContentsEqual\":%s,\"terminatedAt49\":%s,\"sharedBytes\":",
        byte_count, shared_across_tus ? "true" : "false", different_across_tus ? "true" : "false",
        shared_stable ? "true" : "false", different_stable ? "true" : "false", named_arrays_distinct ? "true" : "false",
        unchanged ? "true" : "false", contents_equal ? "true" : "false", terminated ? "true" : "false");
    print_bytes(shared_bytes);
    std::fputs(",\"differentBytes\":", stdout); print_bytes(different_bytes);
    std::fputs("}}\n", stdout);
    return std::fflush(stdout) == 0 ? 0 : 1;
}
