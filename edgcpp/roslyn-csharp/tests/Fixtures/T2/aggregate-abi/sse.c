#include "aggregate-abi.h"
#define ABI_TYPE struct SsePair
#define ABI_CALLBACK sse_callback
#define ABI_FIRST -3.25
#define ABI_SECOND 7.5
#define ABI_FIRST_DELTA 0.5
#define ABI_SECOND_DELTA -1.5
#define ABI_SCALE 4
#define ABI_ECHO native_sse_echo
#define ABI_INVOKE native_sse_invoke
#define ABI_PROVIDER native_sse_provider
#define ABI_FIELD native_sse_field
#define ABI_ECHO_CALLBACK native_sse_echo_callback
#include "two-field-application.inc"
