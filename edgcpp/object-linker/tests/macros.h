#ifndef DOTOBJ_MACROS_H
#define DOTOBJ_MACROS_H
#define API_VALUE 17
#define API_ALIAS API_VALUE
#define API_EMPTY
#define API_ADD(a, b) ((a) + (b))
#define API_CAT(a, b) a ## b
#define API_STRING(x) #x
#define API_VARIADIC(first, ...) ((first) + (__VA_ARGS__))
#define API_MULTI(x) \
    ((x) + 4)
#define API_COMMENT 5 /* Canonical capture deliberately omits this trivia. */
#define API_UTF8 u8"café"
#define API_OPTIONAL_TEXT "__VA_OPT__(17)"
#define API_SAME 9
#define API_SAME 9
#if 0
#define API_INACTIVE 99
#undef API_VALUE
#pragma push_macro("API_VALUE")
#endif
#endif
