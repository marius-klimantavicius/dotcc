#include "aggregate-abi.h"
static struct MemoryTriple transform(struct MemoryTriple value) {
    value.first += 3;
    value.second -= 2;
    value.third += 5;
    return value;
}
static memory_callback callback = transform;
static int observations[29];
int aggregate_observation_count(void) { return 29; }
int aggregate_observe(int selector) { return observations[selector]; }
int fixture(void) {
    struct MemoryTriple original = {-7, 11, 17};
    struct MemoryTriple direct = transform(original);
    struct MemoryTriple indirect = callback(original);
    struct MemoryTriple echoed = native_memory_echo(original);
    struct MemoryTriple reentered = native_memory_invoke(callback, original);
    memory_callback supplied = native_memory_provider();
    struct MemoryTriple unknown = supplied(original);
    observations[0] = (int)direct.first; observations[1] = (int)direct.second; observations[2] = (int)direct.third;
    observations[3] = (int)indirect.first; observations[4] = (int)indirect.second; observations[5] = (int)indirect.third;
    observations[6] = (int)echoed.first; observations[7] = (int)echoed.second; observations[8] = (int)echoed.third;
    observations[9] = (int)reentered.first; observations[10] = (int)reentered.second; observations[11] = (int)reentered.third;
    observations[12] = (int)unknown.first; observations[13] = (int)unknown.second; observations[14] = (int)unknown.third;
    for (int index = 0; index < 3; ++index) observations[15 + index] = (int)native_memory_field(&reentered, index);
    observations[18] = native_memory_echo_callback(callback) == callback;
    observations[19] = native_memory_provider() == supplied;
    observations[20] = native_memory_field(&reentered, 0) == -4;
    observations[21] = native_memory_field(&reentered, 1) == 9;
    observations[22] = native_memory_field(&reentered, 2) == 22;
    observations[23] = echoed.first == -7LL;
    observations[24] = echoed.second == 11LL;
    observations[25] = echoed.third == 17LL;
    observations[26] = unknown.first == -7LL;
    observations[27] = unknown.second == 11LL;
    observations[28] = unknown.third == 17LL;
    int result = 0;
    for (int index = 0; index < 29; ++index) result += observations[index];
    return result;
}
