#include <stddef.h>
#include <stdint.h>
#include <limits.h>

#if !defined(DOT_TARGET_NATIVE_ORACLE)
#if !defined(__EDG_DOT_TARGET__) || __EDG_DOT_TARGET_VERSION__ != 1
#error Missing portable target identity
#endif
#if defined(__linux__) || defined(__linux) || defined(linux) || \
    defined(__unix__) || defined(unix) || defined(_WIN32) || defined(_WIN64) || \
    defined(__APPLE__) || defined(__MACH__) || defined(__x86_64__) || \
    defined(__x86_64) || defined(__i386__) || defined(__aarch64__) || \
    defined(__arm__) || defined(__SSE__) || defined(__AVX__) || defined(_GNU_SOURCE)
#error Host OS or CPU leaked into the portable translated program
#endif
#endif

#ifdef __cplusplus
#define CHECK(condition) static_assert(condition, #condition)
#define ALIGNOF(type) alignof(type)
CHECK(sizeof(bool) == 1);
CHECK(sizeof(char16_t) == 2 && sizeof(char32_t) == 4);
#else
#define CHECK(condition) _Static_assert(condition, #condition)
#define ALIGNOF(type) _Alignof(type)
CHECK(sizeof(_Bool) == 1);
#endif

CHECK(CHAR_BIT == 8 && (char)-1 < 0);
CHECK(sizeof(short) == 2 && ALIGNOF(short) == 2);
CHECK(sizeof(int) == 4 && ALIGNOF(int) == 4);
CHECK(sizeof(long) == 8 && ALIGNOF(long) == 8);
CHECK(sizeof(long long) == 8 && ALIGNOF(long long) == 8);
CHECK(sizeof(__int128) == 16 && ALIGNOF(__int128) == 16);
CHECK(sizeof(void*) == 8 && ALIGNOF(void*) == 8);
CHECK(sizeof(size_t) == 8 && sizeof(ptrdiff_t) == 8);
CHECK(sizeof(wchar_t) == 4);
CHECK(sizeof(float) == 4 && ALIGNOF(float) == 4);
CHECK(sizeof(double) == 8 && ALIGNOF(double) == 8);
CHECK(sizeof(long double) == 16 && ALIGNOF(long double) == 16);
CHECK(ALIGNOF(max_align_t) == 16);
CHECK(__LDBL_MANT_DIG__ == 64 && __LDBL_MAX_EXP__ == 16384);
CHECK(__LDBL_MIN_EXP__ == -16381);
CHECK(__LDBL_EPSILON__ == 0x1p-63L);
CHECK(__FLT_EPSILON__ == 0x1p-23F && __DBL_EPSILON__ == 0x1p-52);
CHECK(__SIZEOF_LONG__ == sizeof(long));
CHECK(__SIZEOF_LONG_DOUBLE__ == sizeof(long double));
CHECK(sizeof(int64_t) == 8 && sizeof(uintptr_t) == 8);
CHECK(SIZE_MAX == (size_t)-1);

struct TargetLayout { char c; long n; void* pointer; long double extended; };
CHECK(offsetof(struct TargetLayout, n) == 8);
CHECK(offsetof(struct TargetLayout, pointer) == 16);
CHECK(offsetof(struct TargetLayout, extended) == 32);
CHECK(sizeof(struct TargetLayout) == 48);

int main(void) {
  union { unsigned long word; unsigned char bytes[8]; } endian;
  union { long double value; unsigned short words[8]; } extended;
  endian.word = 0x0807060504030201UL;
  for (int i = 0; i != 8; ++i) if (endian.bytes[i] != i + 1) return 1;
  extended.value = 1.0L + __LDBL_EPSILON__;
  if (extended.words[0] != 1 || extended.words[1] != 0 ||
      extended.words[2] != 0 || extended.words[3] != 0x8000 ||
      extended.words[4] != 0x3fff) return 2;
  if ((__LDBL_MAX__ / 2.0L) * 2.0L != __LDBL_MAX__) return 3;
  if (__LDBL_DENORM_MIN__ * 2.0L != 0x1p-16444L) return 4;
  if (__FLT_MIN__ != 0x1p-126F || __DBL_MIN__ != 0x1p-1022) return 5;
  return 0;
}
