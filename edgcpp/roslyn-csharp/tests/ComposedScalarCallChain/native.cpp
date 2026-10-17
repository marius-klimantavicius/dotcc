#include "../Fixtures/T5/composed-scalar-call-chain.hpp"

#include <array>
#include <cstddef>
#include <cstdint>
#include <cstdio>
#include <cstring>

// Only the native oracle constructs these objects. The generated checker uses
// independently aligned captured-layout bytes without executing a C++ lifetime.
struct ConcreteScalarInput final : ScalarInput {
    int unused() const override { return 17; }
};

int ScalarOwnerPrefix::unused() const { return 29; }

namespace {
constexpr int value_count = 24;
constexpr int event_capacity = 64;
using InputBytes = std::array<unsigned char, sizeof(ConcreteScalarInput)>;
using OwnerBytes = std::array<unsigned char, sizeof(ScalarOwner)>;

struct Observation {
    int result;
    std::array<int, value_count> values;
    std::array<int, event_capacity> events;
    int count;
    int overflow;
    int live;
    std::array<bool, 4> unchanged;
    std::array<bool, 2> borrowed_identity;
};

struct Phase {
    int first_target;
    int second_target;
    bool first_direct;
    bool second_direct;
    Observation first;
    Observation second;
};

struct Scenario {
    unsigned first_kind;
    unsigned second_kind;
    std::array<Phase, 4> phases;
};

template<class T>
std::array<unsigned char, sizeof(T)> snapshot(const T& input)
{
    std::array<unsigned char, sizeof(T)> bytes;
    std::memcpy(bytes.data(), &input, bytes.size());
    return bytes;
}

template<class T>
bool unchanged(const std::array<unsigned char, sizeof(T)>& before, const T& input)
{
    return std::memcmp(before.data(), &input, before.size()) == 0;
}

template<class T>
std::ptrdiff_t offset(const T& owner, const void* member)
{
    return static_cast<const unsigned char*>(member) -
        reinterpret_cast<const unsigned char*>(&owner);
}

ScalarInput* selected_pointer(int selected, ConcreteScalarInput& first,
    ConcreteScalarInput& second)
{
    return selected == 0 ? nullptr : selected == 1 ? &first : &second;
}

bool observe(const ScalarOwner& first, const ScalarOwner& second,
    ConcreteScalarInput& first_input, ConcreteScalarInput& second_input,
    const OwnerBytes& before_first, const OwnerBytes& before_second,
    const InputBytes& before_first_input, const InputBytes& before_second_input,
    Observation& output)
{
    output.result = fixture(&first, &second, &first_input, &second_input);
    output.count = lifetime_count();
    output.overflow = lifetime_overflow();
    output.live = lifetime_live();
    output.unchanged = {unchanged(before_first, first), unchanged(before_second, second),
        unchanged(before_first_input, first_input), unchanged(before_second_input, second_input)};
    output.borrowed_identity = {&first.view() == &first.payload, &second.view() == &second.payload};
    if (output.count < 0 || output.count > event_capacity) {
        std::fputs("Native events exceed the bounded observation buffer.\n", stderr);
        return false;
    }
    for (int index = 0; index < value_count; ++index) output.values[index] = lifetime_value(index);
    for (int index = 0; index < output.count; ++index) output.events[index] = lifetime_event(index);
    return true;
}

void print_bool(bool value) { std::fputs(value ? "true" : "false", stdout); }

template<std::size_t N>
void print_bools(const std::array<bool, N>& values)
{
    std::fputc('[', stdout);
    for (std::size_t index = 0; index < N; ++index) {
        if (index != 0) std::fputc(',', stdout);
        print_bool(values[index]);
    }
    std::fputc(']', stdout);
}

void print_observation(const Observation& value)
{
    std::printf("{\"result\":%d,\"values\":[", value.result);
    for (int index = 0; index < value_count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.values[index]);
    std::fputs("],\"events\":[", stdout);
    for (int index = 0; index < value.count; ++index)
        std::printf("%s%d", index == 0 ? "" : ",", value.events[index]);
    std::printf("],\"overflow\":%d,\"live\":%d,\"sameObjectByteChecks\":", value.overflow, value.live);
    print_bools(value.unchanged);
    std::fputs(",\"borrowedFieldIdentity\":", stdout);
    print_bools(value.borrowed_identity);
    std::fputc('}', stdout);
}

bool valid(const Observation& value)
{
    if (value.overflow != 0 || value.live != 0) return false;
    for (bool check : value.unchanged) if (!check) return false;
    for (bool check : value.borrowed_identity) if (!check) return false;
    return true;
}

bool same_logical_observation(const Observation& first, const Observation& second)
{
    if (first.result != second.result || first.values != second.values || first.count != second.count ||
        first.overflow != second.overflow || first.live != second.live) return false;
    for (int index = 0; index < first.count; ++index)
        if (first.events[index] != second.events[index]) return false;
    return true;
}
}

int main()
{
    ConcreteScalarInput first_input{};
    ConcreteScalarInput second_input{};
    ScalarOwner first{};
    ScalarOwner second{};
    first_input.first = -0x6123456789ABCDE0LL;
    first_input.second = 0x5123456789ABCDE0LL;
    first_input.tail[0] = 0x0102030405060708LL;
    first_input.tail[1] = -0x0203040506070809LL;
    first_input.tail[2] = 0x030405060708090ALL;
    second_input.first = -0x4123456789ABCDE0LL;
    second_input.second = 0x3123456789ABCDE0LL;
    second_input.tail[0] = 0x1112131415161718LL;
    second_input.tail[1] = -0x1213141516171819LL;
    second_input.tail[2] = 0x131415161718191ALL;
    first.prefix.first = -0x2123456789ABCDE0LL;
    first.prefix.second = 0x1123456789ABCDE0LL;
    first.suffix = 0x2132435465760718LL;
    second.prefix.first = -0x3123456789ABCDE0LL;
    second.prefix.second = 0x2123456789ABCDE0LL;
    second.suffix = -0x3142536475061728LL;
    first.payload.internal.refcount.cookie = &second_input;
    second.payload.internal.refcount.cookie = &first_input;

    const std::array<unsigned, 2> first_kinds{5, 128};
    const std::array<unsigned, 2> second_kinds{201, 255};
    std::array<Scenario, 2> scenarios{};
    bool checks_passed = true;
    for (std::size_t scenario_index = 0; scenario_index < scenarios.size(); ++scenario_index) {
        first_input.kind = static_cast<ScalarCode>(first_kinds[scenario_index]);
        second_input.kind = static_cast<ScalarCode>(second_kinds[scenario_index]);
        auto& scenario = scenarios[scenario_index];
        scenario.first_kind = first_kinds[scenario_index];
        scenario.second_kind = second_kinds[scenario_index];
        scenario.phases[0] = {0, 2, true, false, {}, {}};
        scenario.phases[1] = {1, 0, true, false, {}, {}};
        scenario.phases[2] = {2, 1, false, true, {}, {}};
        scenario.phases[3] = {1, 2, false, true, {}, {}};
        for (auto& phase : scenario.phases) {
            // These writes happen between invocations, outside captured code.
            // Reusing the same two owners makes each subsequent read observable.
            first.payload.internal.pointer = selected_pointer(phase.first_target, first_input, second_input);
            second.payload.internal.pointer = selected_pointer(phase.second_target, first_input, second_input);
            first.payload.use_direct = phase.first_direct;
            second.payload.use_direct = phase.second_direct;
            const auto before_first = snapshot(first);
            const auto before_second = snapshot(second);
            const auto before_first_input = snapshot(first_input);
            const auto before_second_input = snapshot(second_input);
            if (!observe(first, second, first_input, second_input, before_first, before_second,
                    before_first_input, before_second_input, phase.first) ||
                !observe(first, second, first_input, second_input, before_first, before_second,
                    before_first_input, before_second_input, phase.second)) return 1;
            checks_passed = checks_passed && valid(phase.first) && valid(phase.second) &&
                same_logical_observation(phase.first, phase.second);
        }
    }

    std::fputs("{\"scenarios\":[", stdout);
    for (std::size_t scenario_index = 0; scenario_index < scenarios.size(); ++scenario_index) {
        const auto& scenario = scenarios[scenario_index];
        std::printf("%s{\"inputKinds\":[%u,%u],\"phases\":[", scenario_index == 0 ? "" : ",",
            scenario.first_kind, scenario.second_kind);
        for (std::size_t phase_index = 0; phase_index < scenario.phases.size(); ++phase_index) {
            const auto& phase = scenario.phases[phase_index];
            std::printf("%s{\"pointerTargets\":[%d,%d],\"useDirect\":[", phase_index == 0 ? "" : ",",
                phase.first_target, phase.second_target);
            print_bool(phase.first_direct); std::fputc(',', stdout); print_bool(phase.second_direct);
            std::fputs("],\"first\":", stdout); print_observation(phase.first);
            std::fputs(",\"second\":", stdout); print_observation(phase.second);
            std::fputc('}', stdout);
        }
        std::fputs("]}", stdout);
    }
    const auto* storage = static_cast<const ScalarStorage*>(&first.payload.internal);
    const auto* access = static_cast<const ScalarAccess*>(storage);
    std::printf("],\"nativePhysical\":{\"representation\":\"live-native-cpp-owners-and-pointees\","
        "\"ownerSize\":%zu,\"ownerAlignment\":%zu,\"ownerPrefixSize\":%zu,\"ownerPrefixAlignment\":%zu,"
        "\"prefixOffset\":%td,\"prefixFirstOffset\":%td,\"prefixSecondOffset\":%td,\"payloadOffset\":%td,\"suffixOffset\":%td,"
        "\"proxySize\":%zu,\"proxyAlignment\":%zu,\"internalOffset\":%td,\"useDirectOffset\":%td,"
        "\"storageSize\":%zu,\"storageAlignment\":%zu,\"derivedStorageSize\":%zu,\"derivedStorageAlignment\":%zu,"
        "\"pointerOffset\":%td,\"refcountOffset\":%td,\"cookieOffset\":%td,\"accessSize\":%zu,\"accessAlignment\":%zu,"
        "\"storageBaseOffset\":%td,\"accessBaseOffset\":%td,"
        "\"inputSize\":%zu,\"inputAlignment\":%zu,\"concreteInputSize\":%zu,\"concreteInputAlignment\":%zu,"
        "\"inputPrefixBaseOffset\":%td,\"inputFirstOffset\":%td,\"inputSecondOffset\":%td,\"kindOffset\":%td,"
        "\"tailOffsets\":[%td,%td,%td],\"codeSize\":%zu,\"codeAlignment\":%zu,\"addressesAligned\":",
        sizeof(ScalarOwner), alignof(ScalarOwner), sizeof(ScalarOwnerPrefix), alignof(ScalarOwnerPrefix),
        offset(first, &first.prefix), offset(first, &first.prefix.first), offset(first, &first.prefix.second),
        offset(first, &first.payload), offset(first, &first.suffix),
        sizeof(ScalarProxy), alignof(ScalarProxy), offset(first.payload, &first.payload.internal), offset(first.payload, &first.payload.use_direct),
        sizeof(ScalarStorage), alignof(ScalarStorage), sizeof(ScalarDerivedStorage), alignof(ScalarDerivedStorage),
        offset(*storage, &storage->pointer), offset(*storage, &storage->refcount), offset(storage->refcount, &storage->refcount.cookie),
        sizeof(ScalarAccess), alignof(ScalarAccess), offset(first.payload.internal, storage), offset(first.payload.internal, access),
        sizeof(ScalarInput), alignof(ScalarInput), sizeof(ConcreteScalarInput), alignof(ConcreteScalarInput),
        offset(first_input, static_cast<const ScalarInputPrefix*>(&first_input)), offset(first_input, &first_input.first), offset(first_input, &first_input.second),
        offset(first_input, &first_input.kind), offset(first_input, &first_input.tail[0]), offset(first_input, &first_input.tail[1]), offset(first_input, &first_input.tail[2]),
        sizeof(ScalarCode), alignof(ScalarCode));
    print_bools(std::array<bool, 4>{reinterpret_cast<std::uintptr_t>(&first) % alignof(ScalarOwner) == 0,
        reinterpret_cast<std::uintptr_t>(&second) % alignof(ScalarOwner) == 0,
        reinterpret_cast<std::uintptr_t>(&first_input) % alignof(ScalarInput) == 0,
        reinterpret_cast<std::uintptr_t>(&second_input) % alignof(ScalarInput) == 0});
    std::fputs(",\"allSameObjectAndRepeatChecksPassed\":", stdout);
    print_bool(checks_passed);
    std::fputs("}}\n", stdout);
    return checks_passed ? 0 : 1;
}
