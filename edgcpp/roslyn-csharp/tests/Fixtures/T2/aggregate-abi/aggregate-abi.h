#ifndef ROSLYN_CSHARP_T2_AGGREGATE_ABI_H
#define ROSLYN_CSHARP_T2_AGGREGATE_ABI_H

struct IntegerPair { int first; int second; };
struct SsePair { double first; double second; };
struct MixedPair { int first; double second; };
struct MemoryTriple { long long first; long long second; long long third; };
struct __attribute__((packed)) PackedPair { unsigned char first; double second; };
union UnionWord { unsigned long long integer; double floating; };
struct __attribute__((aligned(32))) Overaligned { long long value; };
struct BitFields { unsigned first : 3; signed second : 5; };

#define DECLARE_ABI_TEST(prefix, type) \
    typedef type (*prefix##_callback)(type); \
    type native_##prefix##_echo(type value); \
    type native_##prefix##_invoke(prefix##_callback callback, type value); \
    prefix##_callback native_##prefix##_provider(void); \
    prefix##_callback native_##prefix##_echo_callback(prefix##_callback callback)

DECLARE_ABI_TEST(integer, struct IntegerPair);
DECLARE_ABI_TEST(sse, struct SsePair);
DECLARE_ABI_TEST(mixed, struct MixedPair);
DECLARE_ABI_TEST(memory, struct MemoryTriple);
DECLARE_ABI_TEST(packed, struct PackedPair);
DECLARE_ABI_TEST(unionword, union UnionWord);
#undef DECLARE_ABI_TEST

double native_integer_field(const struct IntegerPair *value, int index);
double native_sse_field(const struct SsePair *value, int index);
double native_mixed_field(const struct MixedPair *value, int index);
double native_memory_field(const struct MemoryTriple *value, int index);
double native_packed_field(const struct PackedPair *value, int index);
unsigned long long native_unionword_bits(const union UnionWord *value);
struct Overaligned native_overaligned_echo(struct Overaligned value);
struct BitFields native_bitfields_echo(struct BitFields value);

int fixture(void);
int aggregate_observation_count(void);
int aggregate_observe(int selector);
#endif
