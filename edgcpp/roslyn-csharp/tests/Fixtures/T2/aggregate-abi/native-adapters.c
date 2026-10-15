/* Narrow native ABI test adapters ONLY: identity values/addresses, callback
   invocation and physical field observations. No application algorithm/body. */
#include "aggregate-abi.h"
#define DEFINE_ABI_TEST(prefix, type) \
    type native_##prefix##_echo(type value) { return value; } \
    type native_##prefix##_invoke(prefix##_callback callback, type value) { return callback(value); } \
    prefix##_callback native_##prefix##_provider(void) { return native_##prefix##_echo; } \
    prefix##_callback native_##prefix##_echo_callback(prefix##_callback callback) { return callback; }
DEFINE_ABI_TEST(integer, struct IntegerPair)
DEFINE_ABI_TEST(sse, struct SsePair)
DEFINE_ABI_TEST(mixed, struct MixedPair)
DEFINE_ABI_TEST(memory, struct MemoryTriple)
DEFINE_ABI_TEST(packed, struct PackedPair)
DEFINE_ABI_TEST(unionword, union UnionWord)
#undef DEFINE_ABI_TEST
double native_integer_field(const struct IntegerPair *value, int index) { return index == 0 ? value->first : value->second; }
double native_sse_field(const struct SsePair *value, int index) { return index == 0 ? value->first : value->second; }
double native_mixed_field(const struct MixedPair *value, int index) { return index == 0 ? value->first : value->second; }
double native_memory_field(const struct MemoryTriple *value, int index) { return index == 0 ? value->first : index == 1 ? value->second : value->third; }
double native_packed_field(const struct PackedPair *value, int index) { return index == 0 ? value->first : value->second; }
unsigned long long native_unionword_bits(const union UnionWord *value) { return value->integer; }
