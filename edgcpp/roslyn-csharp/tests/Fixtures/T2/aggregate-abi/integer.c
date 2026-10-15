#include "aggregate-abi.h"
#define ABI_TYPE struct IntegerPair
#define ABI_CALLBACK integer_callback
#define ABI_FIRST -7
#define ABI_SECOND 11
#define ABI_FIRST_DELTA 3
#define ABI_SECOND_DELTA -2
#define ABI_SCALE 1
#define ABI_ECHO native_integer_echo
#define ABI_INVOKE native_integer_invoke
#define ABI_PROVIDER native_integer_provider
#define ABI_FIELD native_integer_field
#define ABI_ECHO_CALLBACK native_integer_echo_callback
#include "two-field-application.inc"
