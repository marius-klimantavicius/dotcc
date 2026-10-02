//type: fp
//options: 
# 0 "./analyzer/atomic-builtins-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./analyzer/atomic-builtins-1.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
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
# 36 "/usr/include/stdint.h" 3 4
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
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 7 "./analyzer/atomic-builtins-1.c" 2

# 1 "./analyzer/analyzer-decls.h" 1
# 20 "./analyzer/analyzer-decls.h"

# 20 "./analyzer/analyzer-decls.h"
extern void __analyzer_break (void);




extern void __analyzer_describe (int verbosity, ...);


extern void __analyzer_dump (void);


extern void __analyzer_dump_capacity (const void *ptr);


extern void __analyzer_dump_escaped (void);
# 44 "./analyzer/analyzer-decls.h"
extern void __analyzer_dump_exploded_nodes (int);


extern void __analyzer_dump_named_constant (const char *name);



extern void __analyzer_dump_path (void);


extern void __analyzer_dump_region_model (void);




extern void __analyzer_dump_state (const char *name, ...);



extern void __analyzer_eval (int);


extern void *__analyzer_get_unknown_ptr (void);




extern long unsigned int __analyzer_get_strlen (const char *ptr);
# 9 "./analyzer/atomic-builtins-1.c" 2



void test__atomic_exchange_on_int8 (int8_t i, int8_t j)
{
  int8_t orig_i = i;
  int8_t orig_j = j;
  int8_t ret;
  __atomic_exchange (&i, &j, &ret, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_on_int16 (int16_t i, int16_t j)
{
  int16_t orig_i = i;
  int16_t orig_j = j;
  int16_t ret;
  __atomic_exchange (&i, &j, &ret, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_on_int32 (int32_t i, int32_t j)
{
  int32_t orig_i = i;
  int32_t orig_j = j;
  int32_t ret;
  __atomic_exchange (&i, &j, &ret, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_on_int64 (int64_t i, int64_t j)
{
  int64_t orig_i = i;
  int64_t orig_j = j;
  int64_t ret;
  __atomic_exchange (&i, &j, &ret, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_on_int128 (__int128 i, __int128 j)
{
  __int128 orig_i = i;
  __int128 orig_j = j;
  __int128 ret;
  __atomic_exchange (&i, &j, &ret, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}



void test__atomic_exchange_n_on_int8 (int8_t i, int8_t j)
{
  int8_t orig_i = i;
  int8_t orig_j = j;
  int8_t ret;
  ret = __atomic_exchange_n (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_n_on_int16 (int16_t i, int16_t j)
{
  int16_t orig_i = i;
  int16_t orig_j = j;
  int16_t ret;
  ret = __atomic_exchange_n (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_n_on_int32 (int32_t i, int32_t j)
{
  int32_t orig_i = i;
  int32_t orig_j = j;
  int32_t ret;
  ret = __atomic_exchange_n (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_n_on_int64 (int64_t i, int64_t j)
{
  int64_t orig_i = i;
  int64_t orig_j = j;
  int64_t ret;
  ret = __atomic_exchange_n (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_n_on_int128 (__int128 i, __int128 j)
{
  __int128 orig_i = i;
  __int128 orig_j = j;
  __int128 ret;
  ret = __atomic_exchange_n (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}



void test__atomic_exchange_1 (int8_t i, int8_t j)
{
  int8_t orig_i = i;
  int8_t orig_j = j;
  int8_t ret;
  ret = __atomic_exchange_1 (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_2 (int16_t i, int16_t j)
{
  int16_t orig_i = i;
  int16_t orig_j = j;
  int16_t ret;
  ret = __atomic_exchange_2 (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_4 (int32_t i, int32_t j)
{
  int32_t orig_i = i;
  int32_t orig_j = j;
  int32_t ret;
  ret = __atomic_exchange_4 (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_8 (int64_t i, int64_t j)
{
  int64_t orig_i = i;
  int64_t orig_j = j;
  int64_t ret;
  ret = __atomic_exchange_8 (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}

void test__atomic_exchange_16 (__int128 i, __int128 j)
{
  __int128 orig_i = i;
  __int128 orig_j = j;
  __int128 ret;
  ret = __atomic_exchange_16 (&i, j, 0);
  __analyzer_eval (ret == orig_i);
  __analyzer_eval (i == orig_j);
}



void test__atomic_load_from_int8 (int8_t i)
{
  int8_t orig_i = i;
  int8_t ret;
  __atomic_load (&i, &ret, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_from_int16 (int16_t i)
{
  int16_t orig_i = i;
  int16_t ret;
  __atomic_load (&i, &ret, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_from_int32 (int32_t i)
{
  int32_t orig_i = i;
  int32_t ret;
  __atomic_load (&i, &ret, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_from_int64 (int64_t i)
{
  int64_t orig_i = i;
  int64_t ret;
  __atomic_load (&i, &ret, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_from_int1288 (__int128 i)
{
  __int128 orig_i = i;
  __int128 ret;
  __atomic_load (&i, &ret, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}



void test__atomic_load_n_from_int8 (int8_t i)
{
  int8_t orig_i = i;
  int8_t ret;
  ret = __atomic_load_n (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_n_from_int16 (int16_t i)
{
  int16_t orig_i = i;
  int16_t ret;
  ret = __atomic_load_n (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_n_from_int32 (int32_t i)
{
  int32_t orig_i = i;
  int32_t ret;
  ret = __atomic_load_n (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_n_from_int64 (int64_t i)
{
  int64_t orig_i = i;
  int64_t ret;
  ret = __atomic_load_n (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_n_from_int128 (__int128 i)
{
  __int128 orig_i = i;
  __int128 ret;
  ret = __atomic_load_n (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}



void test__atomic_load_1 (int8_t i)
{
  int8_t orig_i = i;
  int8_t ret;
  ret = __atomic_load_1 (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_2 (int16_t i)
{
  int16_t orig_i = i;
  int16_t ret;
  ret = __atomic_load_2 (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_4 (int32_t i)
{
  int32_t orig_i = i;
  int32_t ret;
  ret = __atomic_load_4 (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_8 (int64_t i)
{
  int64_t orig_i = i;
  int64_t ret;
  ret = __atomic_load_8 (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_load_16 (__int128 i)
{
  __int128 orig_i = i;
  __int128 ret;
  ret = __atomic_load_16 (&i, 0);
  __analyzer_eval (i == orig_i);
  __analyzer_eval (ret == orig_i);
}

void test__atomic_store_n_on_uint8 (uint8_t i)
{
  uint8_t tmp;
  __atomic_store_n (&tmp, i, 0);
  __analyzer_eval (tmp == i);
}

void test__atomic_store_n_on_uint16 (uint16_t i)
{
  uint16_t tmp;
  __atomic_store_n (&tmp, i, 0);
  __analyzer_eval (tmp == i);
}

void test__atomic_store_n_on_uint32 (uint32_t i)
{
  uint32_t tmp;
  __atomic_store_n (&tmp, i, 0);
  __analyzer_eval (tmp == i);
}

void test__atomic_store_n_on_uint64 (uint64_t i)
{
  uint64_t tmp;
  __atomic_store_n (&tmp, i, 0);
  __analyzer_eval (tmp == i);
}

void test__atomic_store_n_on_int128 (__int128 i)
{
  __int128 tmp;
  __atomic_store_n (&tmp, i, 0);
  __analyzer_eval (tmp == i);
}





void test__atomic_add_fetch_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_add_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i + j));
  __analyzer_eval (ret == i);
}

void test__atomic_add_fetch_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_add_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i + j));
  __analyzer_eval (ret == i);
}



void test__atomic_sub_fetch_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_sub_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i - j));
  __analyzer_eval (ret == i);
}

void test__atomic_sub_fetch_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_sub_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i - j));
  __analyzer_eval (ret == i);
}



void test__atomic_and_fetch_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_and_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i & j));
  __analyzer_eval (ret == i);
}

void test__atomic_and_fetch_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_and_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i & j));
  __analyzer_eval (ret == i);
}



void test__atomic_xor_fetch_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_xor_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i ^ j));
  __analyzer_eval (ret == i);
}

void test__atomic_xor_fetch_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_xor_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i ^ j));
  __analyzer_eval (ret == i);
}



void test__atomic_or_fetch_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_or_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i | j));
  __analyzer_eval (ret == i);
}

void test__atomic_or_fetch_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_or_fetch (&i, j, 0);
  __analyzer_eval (i == (orig_i | j));
  __analyzer_eval (ret == i);
}





void test__atomic_fetch_add_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_fetch_add (&i, j, 0);
  __analyzer_eval (i == (orig_i + j));
  __analyzer_eval (ret == orig_i);
}

void test__atomic_fetch_add_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_fetch_add (&i, j, 0);
  __analyzer_eval (i == (orig_i + j));
  __analyzer_eval (ret == orig_i);
}



void test__atomic_fetch_sub_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_fetch_sub (&i, j, 0);
  __analyzer_eval (i == (orig_i - j));
  __analyzer_eval (ret == orig_i);
}

void test__atomic_fetch_sub_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_fetch_sub (&i, j, 0);
  __analyzer_eval (i == (orig_i - j));
  __analyzer_eval (ret == orig_i);
}



void test__atomic_fetch_and_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_fetch_and (&i, j, 0);
  __analyzer_eval (i == (orig_i & j));
  __analyzer_eval (ret == orig_i);
}

void test__atomic_fetch_and_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_fetch_and (&i, j, 0);
  __analyzer_eval (i == (orig_i & j));
  __analyzer_eval (ret == orig_i);
}



void test__atomic_fetch_xor_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_fetch_xor (&i, j, 0);
  __analyzer_eval (i == (orig_i ^ j));
  __analyzer_eval (ret == orig_i);
}

void test__atomic_fetch_xor_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_fetch_xor (&i, j, 0);
  __analyzer_eval (i == (orig_i ^ j));
  __analyzer_eval (ret == orig_i);
}



void test__atomic_fetch_or_on_uint32_t (uint32_t i, uint32_t j)
{
  uint32_t orig_i = i;
  uint32_t ret;
  ret = __atomic_fetch_or (&i, j, 0);
  __analyzer_eval (i == (orig_i | j));
  __analyzer_eval (ret == orig_i);
}

void test__atomic_fetch_or_on_uint64_t (uint64_t i, uint64_t j)
{
  uint64_t orig_i = i;
  uint64_t ret;
  ret = __atomic_fetch_or (&i, j, 0);
  __analyzer_eval (i == (orig_i | j));
  __analyzer_eval (ret == orig_i);
}
