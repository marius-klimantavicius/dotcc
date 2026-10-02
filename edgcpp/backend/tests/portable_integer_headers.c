#include <stdint.h>
#include <stddef.h>
#include <limits.h>
#include <errno.h>
#ifdef __cplusplus
#define ASSERT static_assert
#else
#define ASSERT _Static_assert
#endif
ASSERT(sizeof(int_least8_t)==1 && sizeof(int_least16_t)==2 && sizeof(int_least32_t)==4 && sizeof(int_least64_t)==8,"least-width ABI");
ASSERT(sizeof(int_fast8_t)==1 && sizeof(int_fast16_t)==4 && sizeof(int_fast32_t)==4 && sizeof(int_fast64_t)==8,"fast-width ABI");
ASSERT(sizeof(intptr_t)==sizeof(void*) && sizeof(uintptr_t)==sizeof(void*),"pointer integer ABI");
ASSERT(sizeof(intmax_t)==8 && sizeof(uintmax_t)==8,"maximum standard integer ABI");
ASSERT(INT_LEAST16_MAX==32767 && UINT_LEAST16_MAX==65535,"least limits");
ASSERT(INT_FAST16_MAX==INT32_MAX && UINT_FAST16_MAX==UINT32_MAX,"fast limits");
ASSERT(INTMAX_MIN==(-INT64_MAX-1) && UINTMAX_MAX==UINT64_MAX,"maximum limits");
ASSERT(WCHAR_MIN<0 && WCHAR_MAX==INT32_MAX && sizeof(wchar_t)==4,"wide character ABI");
ASSERT(WINT_MIN==0 && WINT_MAX==UINT32_MAX,"wide input ABI");
ASSERT(SIG_ATOMIC_MIN==INT32_MIN && SIG_ATOMIC_MAX==INT32_MAX,"signal atomic limits");
ASSERT(SIZE_MAX==UINTPTR_MAX && PTRDIFF_MIN==INTPTR_MIN,"size and difference limits");
ASSERT(sizeof(INT64_C(1))==8 && sizeof(UINT64_C(1))==8,"literal suffix ABI");
ASSERT(EBADF==9 && EOVERFLOW==75 && EILSEQ==84 && ENOTSUP==95,"normalized runtime error ABI");
static int_least16_t clamp(int_fast16_t value) {
  if(value>INT_LEAST16_MAX)return INT_LEAST16_MAX;
  if(value<INT_LEAST16_MIN)return INT_LEAST16_MIN;
  return (int_least16_t)value;
}
int main(void) {
  uint_fast16_t widened=UINT_LEAST16_MAX;
  widened+=UINT16_C(1);
  uintptr_t pointer=(uintptr_t)&widened;
  return widened!=65536 || clamp(40000)!=32767 || clamp(-40000)!=-32768 ||
    (void*)pointer!=(void*)&widened || UINTMAX_C(18446744073709551615)!=UINTMAX_MAX;
}
