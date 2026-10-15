/* Separate union class proof; overlap/merging is not inferred from struct tests. */
#include "aggregate-abi.h"
static union UnionWord transform(union UnionWord value) {
    value.integer ^= 1ULL;
    return value;
}
static unionword_callback callback = transform;
static int observations[13];
int aggregate_observation_count(void) { return 13; }
int aggregate_observe(int selector) { return observations[selector]; }
int fixture(void) {
    union UnionWord original = {.integer = 0x3ff0000000000000ULL};
    union UnionWord direct = transform(original);
    union UnionWord indirect = callback(original);
    union UnionWord echoed = native_unionword_echo(original);
    union UnionWord reentered = native_unionword_invoke(callback, original);
    unionword_callback supplied = native_unionword_provider();
    union UnionWord unknown = supplied(original);
    observations[0] = (int)(direct.integer & 255ULL);
    observations[1] = (int)(indirect.integer & 255ULL);
    observations[2] = (int)(echoed.integer & 255ULL);
    observations[3] = (int)(reentered.integer & 255ULL);
    observations[4] = (int)(unknown.integer & 255ULL);
    observations[5] = (int)(native_unionword_bits(&reentered) & 255ULL);
    observations[6] = native_unionword_echo_callback(callback) == callback;
    observations[7] = native_unionword_provider() == supplied;
    observations[8] = direct.integer == 0x3ff0000000000001ULL;
    observations[9] = indirect.integer == 0x3ff0000000000001ULL;
    observations[10] = echoed.integer == 0x3ff0000000000000ULL;
    observations[11] = native_unionword_bits(&reentered) == 0x3ff0000000000001ULL;
    observations[12] = unknown.integer == 0x3ff0000000000000ULL;
    int result = 0;
    for (int index = 0; index < 13; ++index) result += observations[index];
    return result;
}
