/* Re-inclusion observes the current NDEBUG, as required by ISO C. */
#undef assert
#ifdef NDEBUG
#define assert(expression) ((void)0)
#else
#define assert(expression) ((expression) ? (void)0 : __assert_fail(#expression, __FILE__, __LINE__, __func__))
#endif
#if defined(__STDC_VERSION__) && __STDC_VERSION__ >= 201112L && !defined(__cplusplus)
#ifndef static_assert
#define static_assert _Static_assert
#endif
#endif
#ifndef __EDG_LIBC_ASSERT_DECLARED
#define __EDG_LIBC_ASSERT_DECLARED
#ifdef __cplusplus
extern "C" { [[noreturn]] void __assert_fail(const char *, const char *, int, const char *); }
#else
_Noreturn void __assert_fail(const char *, const char *, int, const char *);
#endif
#endif
