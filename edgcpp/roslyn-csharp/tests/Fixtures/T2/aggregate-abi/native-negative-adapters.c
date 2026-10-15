/* Identity-only ABI test adapters for explicitly unsupported admission shapes. */
#include "aggregate-abi.h"
struct Overaligned native_overaligned_echo(struct Overaligned value) { return value; }
struct BitFields native_bitfields_echo(struct BitFields value) { return value; }
