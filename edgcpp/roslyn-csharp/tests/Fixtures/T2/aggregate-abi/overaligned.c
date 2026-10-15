/* Unsupported admission candidate until explicit over-aligned storage/ABI proof. */
#include "aggregate-abi.h"
int fixture(void) {
    struct Overaligned input = {7};
    struct Overaligned output = native_overaligned_echo(input);
    return (int)output.value;
}
int aggregate_observation_count(void) { return 0; }
int aggregate_observe(int selector) { return selector; }
