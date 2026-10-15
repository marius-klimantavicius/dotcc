/* Storage bitfields are exercised elsewhere; byvalue ABI stays separately gated. */
#include "aggregate-abi.h"
int fixture(void) {
    struct BitFields input = {5, -7};
    struct BitFields output = native_bitfields_echo(input);
    return (int)output.first + output.second;
}
int aggregate_observation_count(void) { return 0; }
int aggregate_observe(int selector) { return selector; }
