#
   # /* a null directive may contain whitespace and comments */
#include <stdio.h>
#include <stdint.h>
#define QUOTED_HEADER "selected.h"
#define HEADER_ALIAS QUOTED_HEADER
#include HEADER_ALIAS
#undef FIXTURE_SELECTED_H
#define STRINGIFY_INNER(x) #x
#define STRINGIFY(x) STRINGIFY_INNER(x)
#include STRINGIFY(selected.h)
#undef FIXTURE_SELECTED_H
#define ANGLED_HEADER <selected.h>
#include ANGLED_HEADER

#if !defined(__has_attribute) || !__has_attribute(packed) || !__has_attribute(aligned) || !__has_attribute(format)
#error Required attribute capabilities
#endif
#if __has_attribute(dotcc_fixture_unknown_attribute)
#error Unknown attributes must be unavailable
#endif

#if UINTPTR_MAX != 0xffffffffffffffffULL || SIZE_MAX != 18446744073709551615UL
#error LP64 pointer and size limits must match
#endif
#if !(UINT64_MAX > 0 && UINT64_MAX / 3 == 6148914691236517205ULL && UINT64_MAX % 10 == 5)
#error Unsigned comparison division and remainder
#endif
#if UINT64_MAX + 1 != 0 || (0x8000000000000000ULL >> 63) != 1 || ~0U != UINT64_MAX
#error Unsigned wrapping shifting and complement
#endif
#if -1 < 1U || !((1 ? -1 : 0U) > 0) || (0 ? -1 : 0U) != 0
#error Usual unsigned conversions include conditional alternatives
#endif
#if 012 != 10 || -5 / 2 != -2 || (-8 >> 2) != -2
#error Signed arithmetic and octal constants
#endif
#if 0 && 1 / 0
#error Unevaluated logical operand
#elif 1 || 1 / 0
#define SHORT_CIRCUIT 7
#endif
#if !(1 ? 1 : 1 / 0)
#error Unevaluated conditional alternative
#endif
#if 0
#if 1 / 0
#error Inactive nested expression
#endif
#endif
int main(void) {
    printf("include=%d short=%d\n", INCLUDE_RESULT, SHORT_CIRCUIT);
    return 0;
}
