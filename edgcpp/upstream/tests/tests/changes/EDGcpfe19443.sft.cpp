//type:fp
//options_all:--clang_v 50000
//remark:[5.0] Clang compatibility: const-qualified argument to __builtin_nontemporal_load
// 3/15/18  [EDGcpfe/19443]
//
// Clang compatibility: const-qualified argument to __builtin_nontemporal_load
//
// A call to __builtin_nontemporal_load with a pointer to a const-qualified type
// had resulted in a spurious error and is now fixed.
// --clang_version 50000):
typedef long long __m128i __attribute__((__vector_size__(16)));
typedef long long __v2di __attribute__ ((__vector_size__ (16)));
__m128i foo (__m128i const *__V) {
  return (__m128i) __builtin_nontemporal_load ((const __v2di *) __V);
}
