#include "../Fixtures/T5/pointer-call-chain.hpp"

#include <array>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstring>

// Native-oracle inputs are real, live C++ objects. Generated aligned-byte inputs
// are a separate test representation; no vptr or padding bytes cross that edge.
struct ConcreteInput final : CellInput {
    int unused() const override { return 17; }
};

namespace {
constexpr int value_count = 32;
constexpr int event_capacity = 64;
using InputBytes = std::array<unsigned char, sizeof(ConcreteInput)>;

struct Observation {
    int result;
    std::array<int, value_count> values;
    std::array<int, event_capacity> events;
    int count;
    int overflow;
    int live;
    bool first_unchanged;
    bool second_unchanged;
};

struct Scenario {
    unsigned first_kind;
    unsigned second_kind;
    Observation first;
    Observation second;
};

InputBytes snapshot(const ConcreteInput& input) {
    InputBytes bytes;
    std::memcpy(bytes.data(), &input, bytes.size());
    return bytes;
}

bool unchanged(const InputBytes& before, const ConcreteInput& after) {
    return std::memcmp(before.data(), &after, before.size()) == 0;
}

// Each member address is compared with its own complete native object's address.
// CellInput is polymorphic; offsetof would not be a portable layout query here.
std::ptrdiff_t offset(const ConcreteInput& owner, const void* member) {
    return static_cast<const unsigned char*>(member) -
        reinterpret_cast<const unsigned char*>(&owner);
}

bool observe(ConcreteInput& first, ConcreteInput& second,
             const InputBytes& before_first, const InputBytes& before_second,
             Observation& output) {
    output.result = fixture(static_cast<CellInput*>(&first), static_cast<CellInput*>(&second));
    output.count = lifetime_count();
    output.overflow = lifetime_overflow();
    output.live = lifetime_live();
    output.first_unchanged = unchanged(before_first, first);
    output.second_unchanged = unchanged(before_second, second);
    if (output.count < 0 || output.count > event_capacity) {
        std::fputs("Native event count exceeds the bounded observation buffer.\n", stderr);
        return false;
    }
    for (int index = 0; index < value_count; ++index) output.values[index] = lifetime_value(index);
    for (int index = 0; index < output.count; ++index) output.events[index] = lifetime_event(index);
    return true;
}

void print_bool(bool value) { std::fputs(value ? "true" : "false", stdout); }

void print_observation(const Observation& value) {
    std::printf("{\"result\":%d,\"values\":[", value.result);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::printf("],\"overflow\":%d,\"live\":%d}", value.overflow, value.live);
}

void print_byte_checks(const Observation& value) {
    std::fputc('[', stdout);
    print_bool(value.first_unchanged);
    std::fputc(',', stdout);
    print_bool(value.second_unchanged);
    std::fputc(']', stdout);
}

void print_input_fields(const ConcreteInput& input) {
    // Signed-to-unsigned conversion preserves all 64 bits by the C++ rule. This
    // is deterministic field data, not an ASLR address or object-padding dump.
    std::printf("{\"prefixHex\":[\"%016llx\",\"%016llx\"],\"tailHex\":[\"%016llx\",\"%016llx\",\"%016llx\"]}",
        static_cast<unsigned long long>(input.first), static_cast<unsigned long long>(input.second),
        static_cast<unsigned long long>(input.tail[0]), static_cast<unsigned long long>(input.tail[1]),
        static_cast<unsigned long long>(input.tail[2]));
}

bool valid(const Observation& value) {
    // Zero live is the shared scalar harness protocol, not an ownership proof.
    return value.overflow == 0 && value.live == 0 && value.first_unchanged && value.second_unchanged;
}
}

int main() {
    ConcreteInput first{};
    ConcreteInput second{};
    first.first = -0x6123456789ABCDE0LL;
    first.second = 0x5123456789ABCDE0LL;
    first.tail[0] = 0x0102030405060708LL;
    first.tail[1] = -0x0203040506070809LL;
    first.tail[2] = 0x030405060708090ALL;
    second.first = -0x4123456789ABCDE0LL;
    second.second = 0x3123456789ABCDE0LL;
    second.tail[0] = 0x1112131415161718LL;
    second.tail[1] = -0x1213141516171819LL;
    second.tail[2] = 0x131415161718191ALL;

    const std::array<unsigned, 2> first_kinds{5, 37};
    const std::array<unsigned, 2> second_kinds{201, 19};
    std::array<Scenario, 2> scenarios{};
    bool checks_passed = true;
    for (std::size_t index = 0; index < scenarios.size(); ++index) {
        first.kind = static_cast<FieldCode>(first_kinds[index]);
        second.kind = static_cast<FieldCode>(second_kinds[index]);
        auto& scenario = scenarios[index];
        scenario.first_kind = first_kinds[index];
        scenario.second_kind = second_kinds[index];
        // Snapshots are taken after construction and every field initialization.
        // Each comparison is against the same live object in the same process.
        const auto before_first = snapshot(first);
        const auto before_second = snapshot(second);
        if (!observe(first, second, before_first, before_second, scenario.first) ||
            !observe(first, second, before_first, before_second, scenario.second)) return 1;
        checks_passed = checks_passed && valid(scenario.first) && valid(scenario.second);
    }

    std::fputs("{\"scenarios\":[", stdout);
    for (std::size_t index = 0; index < scenarios.size(); ++index) {
        const auto& scenario = scenarios[index];
        std::printf("%s{\"inputKinds\":[%u,%u],\"first\":", index == 0 ? "" : ",", scenario.first_kind, scenario.second_kind);
        print_observation(scenario.first);
        std::fputs(",\"second\":", stdout);
        print_observation(scenario.second);
        std::fputs(",\"inputByteChecks\":{\"firstRun\":", stdout);
        print_byte_checks(scenario.first);
        std::fputs(",\"secondRun\":", stdout);
        print_byte_checks(scenario.second);
        std::fputs("}}", stdout);
    }
    const auto* first_base = static_cast<const CellInput*>(&first);
    const auto* second_base = static_cast<const CellInput*>(&second);
    std::printf("],\"nativeInputPhysical\":{\"representation\":\"two-live-native-cpp-objects\","
        "\"cellInputSize\":%zu,\"cellInputAlignment\":%zu,\"concreteInputSize\":%zu,\"concreteInputAlignment\":%zu,"
        "\"inputPrefixSize\":%zu,\"inputPrefixAlignment\":%zu,\"fieldCodeSize\":%zu,\"fieldCodeAlignment\":%zu,"
        "\"cellBaseOffset\":%td,\"prefixBaseOffset\":%td,\"firstOffset\":%td,\"secondOffset\":%td,"
        "\"kindOffset\":%td,\"tailOffsets\":[%td,%td,%td],\"inputAddressesAligned\":[",
        sizeof(CellInput), alignof(CellInput), sizeof(ConcreteInput), alignof(ConcreteInput),
        sizeof(InputPrefix), alignof(InputPrefix), sizeof(FieldCode), alignof(FieldCode),
        offset(first, first_base), offset(first, static_cast<const InputPrefix*>(first_base)),
        offset(first, &first.first), offset(first, &first.second), offset(first, &first.kind),
        offset(first, &first.tail[0]), offset(first, &first.tail[1]), offset(first, &first.tail[2]));
    print_bool(reinterpret_cast<std::uintptr_t>(first_base) % alignof(CellInput) == 0);
    std::fputc(',', stdout);
    print_bool(reinterpret_cast<std::uintptr_t>(second_base) % alignof(CellInput) == 0);
    std::fputs("],\"distinctInputPointers\":", stdout);
    print_bool(first_base != second_base);
    std::fputs(",\"deterministicFirstFields\":", stdout);
    print_input_fields(first);
    std::fputs(",\"deterministicSecondFields\":", stdout);
    print_input_fields(second);
    std::fputs(",\"allSameObjectByteChecksPassed\":", stdout);
    print_bool(checks_passed);
    std::fputs("}}\n", stdout);
    return checks_passed ? 0 : 1;
}
