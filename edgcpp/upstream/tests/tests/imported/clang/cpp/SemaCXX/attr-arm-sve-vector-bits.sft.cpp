//type: fp
//options:  --c++11 --target linux_aarch64
# 1 "SemaCXX/attr-arm-sve-vector-bits.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/attr-arm-sve-vector-bits.cpp" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 1
# 56 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 345 "/usr/include/features.h" 3 4
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 346 "/usr/include/features.h" 2 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4








typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 57 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 2
# 5 "SemaCXX/attr-arm-sve-vector-bits.cpp" 2



typedef __SVInt8_t svint8_t;
typedef svint8_t fixed_int8_t __attribute__((arm_sve_vector_bits(__ARM_FEATURE_SVE_BITS)));
typedef int8_t gnu_int8_t __attribute__((vector_size(__ARM_FEATURE_SVE_BITS / 8)));

typedef __SVBool_t svbool_t;
typedef svbool_t fixed_bool_t __attribute__((arm_sve_vector_bits(__ARM_FEATURE_SVE_BITS)));
typedef int8_t gnu_bool_t __attribute__((vector_size(__ARM_FEATURE_SVE_BITS / 64)));

template<typename T> struct S { T var; };

S<fixed_int8_t> s;


svint8_t to_svint8_t(fixed_int8_t x) { return x; }
fixed_int8_t from_svint8_t(svint8_t x) { return x; }


svint8_t to_svint8_t__from_gnu_int8_t(gnu_int8_t x) { return x; }
gnu_int8_t from_svint8_t__to_gnu_int8_t(svint8_t x) { return x; }


fixed_int8_t to_fixed_int8_t__from_gnu_int8_t(gnu_int8_t x) { return x; }
gnu_int8_t from_fixed_int8_t__to_gnu_int8_t(fixed_int8_t x) { return x; }


svbool_t to_svbool_t(fixed_bool_t x) { return x; }
fixed_bool_t from_svbool_t(svbool_t x) { return x; }


svbool_t to_svbool_t__from_gnu_bool_t(gnu_bool_t x) { return x; }
gnu_bool_t from_svbool_t__to_gnu_bool_t(svbool_t x) { return x; }
