/* Separate packed/unaligned shape; never admitted by an ordinary-shape proof. */
#include "aggregate-abi.h"
#define ABI_TYPE struct PackedPair
#define ABI_CALLBACK packed_callback
#define ABI_FIRST 5
#define ABI_SECOND 7.5
#define ABI_FIRST_DELTA 3
#define ABI_SECOND_DELTA 0.5
#define ABI_SCALE 4
#define ABI_ECHO native_packed_echo
#define ABI_INVOKE native_packed_invoke
#define ABI_PROVIDER native_packed_provider
#define ABI_FIELD native_packed_field
#define ABI_ECHO_CALLBACK native_packed_echo_callback
#include "two-field-application.inc"
