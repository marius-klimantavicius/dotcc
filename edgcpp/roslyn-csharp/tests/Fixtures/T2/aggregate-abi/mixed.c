#include "aggregate-abi.h"
#define ABI_TYPE struct MixedPair
#define ABI_CALLBACK mixed_callback
#define ABI_FIRST -7
#define ABI_SECOND 7.5
#define ABI_FIRST_DELTA 3
#define ABI_SECOND_DELTA 0.5
#define ABI_SCALE 4
#define ABI_ECHO native_mixed_echo
#define ABI_INVOKE native_mixed_invoke
#define ABI_PROVIDER native_mixed_provider
#define ABI_FIELD native_mixed_field
#define ABI_ECHO_CALLBACK native_mixed_echo_callback
#include "two-field-application.inc"
