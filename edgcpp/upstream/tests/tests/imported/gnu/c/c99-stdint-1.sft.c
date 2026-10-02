//type: fp
//options: --c99 --strict_gnu
# 0 "./c99-stdint-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c99-stdint-1.c"
# 15 "./c99-stdint-1.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
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
# 27 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 16 "./c99-stdint-1.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 26 "/usr/include/stdint.h" 3 4
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
# 17 "./c99-stdint-1.c" 2



# 1 "/usr/include/signal.h" 1 3 4
# 30 "/usr/include/signal.h" 3 4


# 1 "/usr/include/bits/sigset.h" 1 3 4
# 23 "/usr/include/bits/sigset.h" 3 4
typedef int __sig_atomic_t;




typedef struct
  {
    unsigned long int __val[(1024 / (8 * sizeof (unsigned long int)))];
  } __sigset_t;
# 103 "/usr/include/bits/sigset.h" 3 4
extern int __sigismember (const __sigset_t *, int);
extern int __sigaddset (__sigset_t *, int);
extern int __sigdelset (__sigset_t *, int);
# 33 "/usr/include/signal.h" 2 3 4







typedef __sig_atomic_t sig_atomic_t;

# 56 "/usr/include/signal.h" 3 4
# 1 "/usr/include/bits/types.h" 1 3 4
# 27 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/bits/types.h" 2 3 4


typedef unsigned char __u_char;
typedef unsigned short int __u_short;
typedef unsigned int __u_int;
typedef unsigned long int __u_long;


typedef signed char __int8_t;
typedef unsigned char __uint8_t;
typedef signed short int __int16_t;
typedef unsigned short int __uint16_t;
typedef signed int __int32_t;
typedef unsigned int __uint32_t;

typedef signed long int __int64_t;
typedef unsigned long int __uint64_t;







typedef long int __quad_t;
typedef unsigned long int __u_quad_t;
# 130 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/typesizes.h" 1 3 4
# 131 "/usr/include/bits/types.h" 2 3 4


typedef unsigned long int __dev_t;
typedef unsigned int __uid_t;
typedef unsigned int __gid_t;
typedef unsigned long int __ino_t;
typedef unsigned long int __ino64_t;
typedef unsigned int __mode_t;
typedef unsigned long int __nlink_t;
typedef long int __off_t;
typedef long int __off64_t;
typedef int __pid_t;
typedef struct { int __val[2]; } __fsid_t;
typedef long int __clock_t;
typedef unsigned long int __rlim_t;
typedef unsigned long int __rlim64_t;
typedef unsigned int __id_t;
typedef long int __time_t;
typedef unsigned int __useconds_t;
typedef long int __suseconds_t;

typedef int __daddr_t;
typedef int __key_t;


typedef int __clockid_t;


typedef void * __timer_t;


typedef long int __blksize_t;




typedef long int __blkcnt_t;
typedef long int __blkcnt64_t;


typedef unsigned long int __fsblkcnt_t;
typedef unsigned long int __fsblkcnt64_t;


typedef unsigned long int __fsfilcnt_t;
typedef unsigned long int __fsfilcnt64_t;


typedef long int __fsword_t;

typedef long int __ssize_t;


typedef long int __syscall_slong_t;

typedef unsigned long int __syscall_ulong_t;



typedef __off64_t __loff_t;
typedef __quad_t *__qaddr_t;
typedef char *__caddr_t;


typedef long int __intptr_t;


typedef unsigned int __socklen_t;
# 57 "/usr/include/signal.h" 2 3 4
# 1 "/usr/include/bits/signum.h" 1 3 4
# 58 "/usr/include/signal.h" 2 3 4
# 85 "/usr/include/signal.h" 3 4
typedef void (*__sighandler_t) (int);




extern __sighandler_t __sysv_signal (int __sig, __sighandler_t __handler)
     __attribute__ ((__nothrow__ , __leaf__));
# 100 "/usr/include/signal.h" 3 4







extern __sighandler_t signal (int __sig, __sighandler_t __handler) __asm__ ("" "__sysv_signal") __attribute__ ((__nothrow__ , __leaf__))

                        ;





# 137 "/usr/include/signal.h" 3 4


extern int raise (int __sig) __attribute__ ((__nothrow__ , __leaf__));

# 169 "/usr/include/signal.h" 3 4
extern int __sigpause (int __sig_or_mask, int __is_sig);
# 403 "/usr/include/signal.h" 3 4
extern int __libc_current_sigrtmin (void) __attribute__ ((__nothrow__ , __leaf__));

extern int __libc_current_sigrtmax (void) __attribute__ ((__nothrow__ , __leaf__));




# 21 "./c99-stdint-1.c" 2
# 83 "./c99-stdint-1.c"

# 83 "./c99-stdint-1.c"
void
test_exact (void)
{

  do { int a[sizeof(int8_t) * 8 
# 87 "./c99-stdint-1.c"
 == (8) ? 1 : -1]; } while (0);
  do { int8_t a; int b[(int8_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 88 "./c99-stdint-1.c" 3 4
 (-128)
# 88 "./c99-stdint-1.c"
 )) a; __typeof__((int8_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 88 "./c99-stdint-1.c" 3 4
 (127)
# 88 "./c99-stdint-1.c"
 )) a; __typeof__((int8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 88 "./c99-stdint-1.c" 3 4
 (-128)
# 88 "./c99-stdint-1.c"
 )) == -((
# 88 "./c99-stdint-1.c" 3 4
 (127)
# 88 "./c99-stdint-1.c"
 ))-1 && (((
# 88 "./c99-stdint-1.c" 3 4
 (127)
# 88 "./c99-stdint-1.c"
 )) & 1) && (((((
# 88 "./c99-stdint-1.c" 3 4
 (127)
# 88 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int8_t) * 8 
# 88 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);




  do { int a[sizeof(int16_t) * 8 
# 93 "./c99-stdint-1.c"
 == (16) ? 1 : -1]; } while (0);
  do { int16_t a; int b[(int16_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 94 "./c99-stdint-1.c" 3 4
 (-32767-1)
# 94 "./c99-stdint-1.c"
 )) a; __typeof__((int16_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 94 "./c99-stdint-1.c" 3 4
 (32767)
# 94 "./c99-stdint-1.c"
 )) a; __typeof__((int16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 94 "./c99-stdint-1.c" 3 4
 (-32767-1)
# 94 "./c99-stdint-1.c"
 )) == -((
# 94 "./c99-stdint-1.c" 3 4
 (32767)
# 94 "./c99-stdint-1.c"
 ))-1 && (((
# 94 "./c99-stdint-1.c" 3 4
 (32767)
# 94 "./c99-stdint-1.c"
 )) & 1) && (((((
# 94 "./c99-stdint-1.c" 3 4
 (32767)
# 94 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int16_t) * 8 
# 94 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);




  do { int a[sizeof(int32_t) * 8 
# 99 "./c99-stdint-1.c"
 == (32) ? 1 : -1]; } while (0);
  do { int32_t a; int b[(int32_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 100 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 100 "./c99-stdint-1.c"
 )) a; __typeof__((int32_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 100 "./c99-stdint-1.c" 3 4
 (2147483647)
# 100 "./c99-stdint-1.c"
 )) a; __typeof__((int32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 100 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 100 "./c99-stdint-1.c"
 )) == -((
# 100 "./c99-stdint-1.c" 3 4
 (2147483647)
# 100 "./c99-stdint-1.c"
 ))-1 && (((
# 100 "./c99-stdint-1.c" 3 4
 (2147483647)
# 100 "./c99-stdint-1.c"
 )) & 1) && (((((
# 100 "./c99-stdint-1.c" 3 4
 (2147483647)
# 100 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int32_t) * 8 
# 100 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);




  do { int a[sizeof(int64_t) * 8 
# 105 "./c99-stdint-1.c"
 == (64) ? 1 : -1]; } while (0);
  do { int64_t a; int b[(int64_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 106 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 106 "./c99-stdint-1.c"
 )) a; __typeof__((int64_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 106 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 106 "./c99-stdint-1.c"
 )) a; __typeof__((int64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 106 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 106 "./c99-stdint-1.c"
 )) == -((
# 106 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 106 "./c99-stdint-1.c"
 ))-1 && (((
# 106 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 106 "./c99-stdint-1.c"
 )) & 1) && (((((
# 106 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 106 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int64_t) * 8 
# 106 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);




  do { int a[sizeof(uint8_t) * 8 
# 111 "./c99-stdint-1.c"
 == (8) ? 1 : -1]; } while (0);
  do { uint8_t a; int b[(uint8_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 112 "./c99-stdint-1.c" 3 4
 (255)
# 112 "./c99-stdint-1.c"
 )) a; __typeof__((uint8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 112 "./c99-stdint-1.c" 3 4
 (255)
# 112 "./c99-stdint-1.c"
 )) == (uint8_t)-1) ? 1 : -1]; } while (0);




  do { int a[sizeof(uint16_t) * 8 
# 117 "./c99-stdint-1.c"
 == (16) ? 1 : -1]; } while (0);
  do { uint16_t a; int b[(uint16_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 118 "./c99-stdint-1.c" 3 4
 (65535)
# 118 "./c99-stdint-1.c"
 )) a; __typeof__((uint16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 118 "./c99-stdint-1.c" 3 4
 (65535)
# 118 "./c99-stdint-1.c"
 )) == (uint16_t)-1) ? 1 : -1]; } while (0);




  do { int a[sizeof(uint32_t) * 8 
# 123 "./c99-stdint-1.c"
 == (32) ? 1 : -1]; } while (0);
  do { uint32_t a; int b[(uint32_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 124 "./c99-stdint-1.c" 3 4
 (4294967295U)
# 124 "./c99-stdint-1.c"
 )) a; __typeof__((uint32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 124 "./c99-stdint-1.c" 3 4
 (4294967295U)
# 124 "./c99-stdint-1.c"
 )) == (uint32_t)-1) ? 1 : -1]; } while (0);




  do { int a[sizeof(uint64_t) * 8 
# 129 "./c99-stdint-1.c"
 == (64) ? 1 : -1]; } while (0);
  do { uint64_t a; int b[(uint64_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 130 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 130 "./c99-stdint-1.c"
 )) a; __typeof__((uint64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 130 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 130 "./c99-stdint-1.c"
 )) == (uint64_t)-1) ? 1 : -1]; } while (0);



}

void
test_least (void)
{
  do { int a[sizeof(int_least8_t) * 8 
# 139 "./c99-stdint-1.c"
 >= (8) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast8_t) >= sizeof(int_least8_t) ? 1 : -1]; } while (0);
  do { int_least8_t a; int b[(int_least8_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 141 "./c99-stdint-1.c" 3 4
 (-128)
# 141 "./c99-stdint-1.c"
 )) a; __typeof__((int_least8_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 141 "./c99-stdint-1.c" 3 4
 (127)
# 141 "./c99-stdint-1.c"
 )) a; __typeof__((int_least8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 141 "./c99-stdint-1.c" 3 4
 (-128)
# 141 "./c99-stdint-1.c"
 )) == -((
# 141 "./c99-stdint-1.c" 3 4
 (127)
# 141 "./c99-stdint-1.c"
 ))-1 && (((
# 141 "./c99-stdint-1.c" 3 4
 (127)
# 141 "./c99-stdint-1.c"
 )) & 1) && (((((
# 141 "./c99-stdint-1.c" 3 4
 (127)
# 141 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_least8_t) * 8 
# 141 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_least16_t) * 8 
# 142 "./c99-stdint-1.c"
 >= (16) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast16_t) >= sizeof(int_least16_t) ? 1 : -1]; } while (0);
  do { int_least16_t a; int b[(int_least16_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 144 "./c99-stdint-1.c" 3 4
 (-32767-1)
# 144 "./c99-stdint-1.c"
 )) a; __typeof__((int_least16_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 144 "./c99-stdint-1.c" 3 4
 (32767)
# 144 "./c99-stdint-1.c"
 )) a; __typeof__((int_least16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 144 "./c99-stdint-1.c" 3 4
 (-32767-1)
# 144 "./c99-stdint-1.c"
 )) == -((
# 144 "./c99-stdint-1.c" 3 4
 (32767)
# 144 "./c99-stdint-1.c"
 ))-1 && (((
# 144 "./c99-stdint-1.c" 3 4
 (32767)
# 144 "./c99-stdint-1.c"
 )) & 1) && (((((
# 144 "./c99-stdint-1.c" 3 4
 (32767)
# 144 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_least16_t) * 8 
# 144 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_least32_t) * 8 
# 145 "./c99-stdint-1.c"
 >= (32) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast32_t) >= sizeof(int_least32_t) ? 1 : -1]; } while (0);
  do { int_least32_t a; int b[(int_least32_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 147 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 147 "./c99-stdint-1.c"
 )) a; __typeof__((int_least32_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 147 "./c99-stdint-1.c" 3 4
 (2147483647)
# 147 "./c99-stdint-1.c"
 )) a; __typeof__((int_least32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 147 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 147 "./c99-stdint-1.c"
 )) == -((
# 147 "./c99-stdint-1.c" 3 4
 (2147483647)
# 147 "./c99-stdint-1.c"
 ))-1 && (((
# 147 "./c99-stdint-1.c" 3 4
 (2147483647)
# 147 "./c99-stdint-1.c"
 )) & 1) && (((((
# 147 "./c99-stdint-1.c" 3 4
 (2147483647)
# 147 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_least32_t) * 8 
# 147 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_least64_t) * 8 
# 148 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast64_t) >= sizeof(int_least64_t) ? 1 : -1]; } while (0);
  do { int_least64_t a; int b[(int_least64_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 150 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 150 "./c99-stdint-1.c"
 )) a; __typeof__((int_least64_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 150 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 150 "./c99-stdint-1.c"
 )) a; __typeof__((int_least64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 150 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 150 "./c99-stdint-1.c"
 )) == -((
# 150 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 150 "./c99-stdint-1.c"
 ))-1 && (((
# 150 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 150 "./c99-stdint-1.c"
 )) & 1) && (((((
# 150 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 150 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_least64_t) * 8 
# 150 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_least8_t) * 8 
# 151 "./c99-stdint-1.c"
 >= (8) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast8_t) >= sizeof(uint_least8_t) ? 1 : -1]; } while (0);
  do { uint_least8_t a; int b[(uint_least8_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 153 "./c99-stdint-1.c" 3 4
 (255)
# 153 "./c99-stdint-1.c"
 )) a; __typeof__((uint_least8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 153 "./c99-stdint-1.c" 3 4
 (255)
# 153 "./c99-stdint-1.c"
 )) == (uint_least8_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_least16_t) * 8 
# 154 "./c99-stdint-1.c"
 >= (16) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast16_t) >= sizeof(uint_least16_t) ? 1 : -1]; } while (0);
  do { uint_least16_t a; int b[(uint_least16_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 156 "./c99-stdint-1.c" 3 4
 (65535)
# 156 "./c99-stdint-1.c"
 )) a; __typeof__((uint_least16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 156 "./c99-stdint-1.c" 3 4
 (65535)
# 156 "./c99-stdint-1.c"
 )) == (uint_least16_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_least32_t) * 8 
# 157 "./c99-stdint-1.c"
 >= (32) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast32_t) >= sizeof(uint_least32_t) ? 1 : -1]; } while (0);
  do { uint_least32_t a; int b[(uint_least32_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 159 "./c99-stdint-1.c" 3 4
 (4294967295U)
# 159 "./c99-stdint-1.c"
 )) a; __typeof__((uint_least32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 159 "./c99-stdint-1.c" 3 4
 (4294967295U)
# 159 "./c99-stdint-1.c"
 )) == (uint_least32_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_least64_t) * 8 
# 160 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast64_t) >= sizeof(uint_least64_t) ? 1 : -1]; } while (0);
  do { uint_least64_t a; int b[(uint_least64_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 162 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 162 "./c99-stdint-1.c"
 )) a; __typeof__((uint_least64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 162 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 162 "./c99-stdint-1.c"
 )) == (uint_least64_t)-1) ? 1 : -1]; } while (0);
}

void
test_fast (void)
{
  do { int a[sizeof(int_fast8_t) * 8 
# 168 "./c99-stdint-1.c"
 >= (8) ? 1 : -1]; } while (0);
  do { int_fast8_t a; int b[(int_fast8_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 169 "./c99-stdint-1.c" 3 4
 (-128)
# 169 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast8_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 169 "./c99-stdint-1.c" 3 4
 (127)
# 169 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 169 "./c99-stdint-1.c" 3 4
 (-128)
# 169 "./c99-stdint-1.c"
 )) == -((
# 169 "./c99-stdint-1.c" 3 4
 (127)
# 169 "./c99-stdint-1.c"
 ))-1 && (((
# 169 "./c99-stdint-1.c" 3 4
 (127)
# 169 "./c99-stdint-1.c"
 )) & 1) && (((((
# 169 "./c99-stdint-1.c" 3 4
 (127)
# 169 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_fast8_t) * 8 
# 169 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast16_t) * 8 
# 170 "./c99-stdint-1.c"
 >= (16) ? 1 : -1]; } while (0);
  do { int_fast16_t a; int b[(int_fast16_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 171 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 171 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast16_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 171 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 171 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 171 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 171 "./c99-stdint-1.c"
 )) == -((
# 171 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 171 "./c99-stdint-1.c"
 ))-1 && (((
# 171 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 171 "./c99-stdint-1.c"
 )) & 1) && (((((
# 171 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 171 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_fast16_t) * 8 
# 171 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast32_t) * 8 
# 172 "./c99-stdint-1.c"
 >= (32) ? 1 : -1]; } while (0);
  do { int_fast32_t a; int b[(int_fast32_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 173 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 173 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast32_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 173 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 173 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 173 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 173 "./c99-stdint-1.c"
 )) == -((
# 173 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 173 "./c99-stdint-1.c"
 ))-1 && (((
# 173 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 173 "./c99-stdint-1.c"
 )) & 1) && (((((
# 173 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 173 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_fast32_t) * 8 
# 173 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(int_fast64_t) * 8 
# 174 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { int_fast64_t a; int b[(int_fast64_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 175 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 175 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast64_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 175 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 175 "./c99-stdint-1.c"
 )) a; __typeof__((int_fast64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 175 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 175 "./c99-stdint-1.c"
 )) == -((
# 175 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 175 "./c99-stdint-1.c"
 ))-1 && (((
# 175 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 175 "./c99-stdint-1.c"
 )) & 1) && (((((
# 175 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 175 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(int_fast64_t) * 8 
# 175 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast8_t) * 8 
# 176 "./c99-stdint-1.c"
 >= (8) ? 1 : -1]; } while (0);
  do { uint_fast8_t a; int b[(uint_fast8_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 177 "./c99-stdint-1.c" 3 4
 (255)
# 177 "./c99-stdint-1.c"
 )) a; __typeof__((uint_fast8_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 177 "./c99-stdint-1.c" 3 4
 (255)
# 177 "./c99-stdint-1.c"
 )) == (uint_fast8_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast16_t) * 8 
# 178 "./c99-stdint-1.c"
 >= (16) ? 1 : -1]; } while (0);
  do { uint_fast16_t a; int b[(uint_fast16_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 179 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 179 "./c99-stdint-1.c"
 )) a; __typeof__((uint_fast16_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 179 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 179 "./c99-stdint-1.c"
 )) == (uint_fast16_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast32_t) * 8 
# 180 "./c99-stdint-1.c"
 >= (32) ? 1 : -1]; } while (0);
  do { uint_fast32_t a; int b[(uint_fast32_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 181 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 181 "./c99-stdint-1.c"
 )) a; __typeof__((uint_fast32_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 181 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 181 "./c99-stdint-1.c"
 )) == (uint_fast32_t)-1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uint_fast64_t) * 8 
# 182 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { uint_fast64_t a; int b[(uint_fast64_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 183 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 183 "./c99-stdint-1.c"
 )) a; __typeof__((uint_fast64_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 183 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 183 "./c99-stdint-1.c"
 )) == (uint_fast64_t)-1) ? 1 : -1]; } while (0);
}

void
test_ptr (void)
{

  do { intptr_t a; int b[(intptr_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 190 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 190 "./c99-stdint-1.c"
 )) a; __typeof__((intptr_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 190 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 190 "./c99-stdint-1.c"
 )) a; __typeof__((intptr_t)0 + 0) *b = &a; } while (0); do { int a[((((
# 190 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 190 "./c99-stdint-1.c"
 )) == -((
# 190 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 190 "./c99-stdint-1.c"
 ))-1 && (((
# 190 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 190 "./c99-stdint-1.c"
 )) & 1) && (((((
# 190 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 190 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(intptr_t) * 8 
# 190 "./c99-stdint-1.c"
 - 2)) == 1) && (
# 190 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 190 "./c99-stdint-1.c"
 ) <= (-0x7fff) && (
# 190 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 190 "./c99-stdint-1.c"
 ) >= (0x7fff)) ? 1 : -1]; } while (0);


  do { uintptr_t a; int b[(uintptr_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 193 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 193 "./c99-stdint-1.c"
 )) a; __typeof__((uintptr_t)0 + 0) *b = &a; } while (0); do { int a[((((
# 193 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 193 "./c99-stdint-1.c"
 )) == (uintptr_t)-1) && (
# 193 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 193 "./c99-stdint-1.c"
 ) >= (0xffffU)) ? 1 : -1]; } while (0);

}

void
test_max (void)
{
  do { int a[sizeof(intmax_t) * 8 
# 200 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { int a[sizeof(intmax_t) >= sizeof(long long) ? 1 : -1]; } while (0);
  do { int a[sizeof(intmax_t) >= sizeof(int_fast8_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(intmax_t) >= sizeof(int_fast16_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(intmax_t) >= sizeof(int_fast32_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(intmax_t) >= sizeof(int_fast64_t) ? 1 : -1]; } while (0);
  do { intmax_t a; int b[(intmax_t)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 206 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 206 "./c99-stdint-1.c"
 )) a; __typeof__((intmax_t)0 + 0) *b = &a; } while (0); do { __typeof__((
# 206 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 206 "./c99-stdint-1.c"
 )) a; __typeof__((intmax_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 206 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L -1)
# 206 "./c99-stdint-1.c"
 )) == -((
# 206 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 206 "./c99-stdint-1.c"
 ))-1 && (((
# 206 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 206 "./c99-stdint-1.c"
 )) & 1) && (((((
# 206 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 206 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(intmax_t) * 8 
# 206 "./c99-stdint-1.c"
 - 2)) == 1) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) * 8 
# 207 "./c99-stdint-1.c"
 >= (64) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) >= sizeof(unsigned long long) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) >= sizeof(uint_fast8_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) >= sizeof(uint_fast16_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) >= sizeof(uint_fast32_t) ? 1 : -1]; } while (0);
  do { int a[sizeof(uintmax_t) >= sizeof(uint_fast64_t) ? 1 : -1]; } while (0);
  do { uintmax_t a; int b[(uintmax_t)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 213 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 213 "./c99-stdint-1.c"
 )) a; __typeof__((uintmax_t)0 + 0) *b = &a; } while (0); do { int a[(((
# 213 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 213 "./c99-stdint-1.c"
 )) == (uintmax_t)-1) ? 1 : -1]; } while (0);
}

void
test_misc_limits (void)
{
  do { long int a; int b[(long int)-1 < 0 ? 1 : -1]; } while (0); do { __typeof__((
# 219 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 219 "./c99-stdint-1.c"
 )) a; __typeof__((long int)0 + 0) *b = &a; } while (0); do { __typeof__((
# 219 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 219 "./c99-stdint-1.c"
 )) a; __typeof__((long int)0 + 0) *b = &a; } while (0); do { int a[((((
# 219 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 219 "./c99-stdint-1.c"
 )) == -((
# 219 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 219 "./c99-stdint-1.c"
 ))-1 && (((
# 219 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 219 "./c99-stdint-1.c"
 )) & 1) && (((((
# 219 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 219 "./c99-stdint-1.c"
 )) >> 1) + 1) >> (sizeof(long int) * 8 
# 219 "./c99-stdint-1.c"
 - 2)) == 1) && (
# 219 "./c99-stdint-1.c" 3 4
 (-9223372036854775807L-1)
# 219 "./c99-stdint-1.c"
 ) <= (-65535L) && (
# 219 "./c99-stdint-1.c" 3 4
 (9223372036854775807L)
# 219 "./c99-stdint-1.c"
 ) >= (65535L)) ? 1 : -1]; } while (0);

  do { int a[(((sig_atomic_t)-1 < 0 ? ((((
# 221 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 221 "./c99-stdint-1.c"
 ))) == -(((
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 )))-1 && ((((
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 ))) & 1) && ((((((
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 ))) >> 1) + 1) >> (sizeof(sig_atomic_t) * 8 
# 221 "./c99-stdint-1.c"
 - 2)) == 1) : (((
# 221 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 221 "./c99-stdint-1.c"
 )) == 0 && ((((
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 ))) == (sig_atomic_t)-1))) && ((sig_atomic_t)-1 < 0 ? ((
# 221 "./c99-stdint-1.c" 3 4
 (-2147483647-1)
# 221 "./c99-stdint-1.c"
 ) <= (-127) && (
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 ) >= (127)) : (
# 221 "./c99-stdint-1.c" 3 4
 (2147483647)
# 221 "./c99-stdint-1.c"
 ) >= (255))) ? 1 : -1]; } while (0);

  do { long unsigned int a; int b[(long unsigned int)-1 < 0 ? -1 : 1]; } while (0); do { __typeof__((
# 223 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 223 "./c99-stdint-1.c"
 )) a; __typeof__((long unsigned int)0 + 0) *b = &a; } while (0); do { int a[((((
# 223 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 223 "./c99-stdint-1.c"
 )) == (long unsigned int)-1) && (
# 223 "./c99-stdint-1.c" 3 4
 (18446744073709551615UL)
# 223 "./c99-stdint-1.c"
 ) >= (65535U)) ? 1 : -1]; } while (0);
  do { int a[(((int)-1 < 0 ? ((((
# 224 "./c99-stdint-1.c" 3 4
 (-2147483647 - 1)
# 224 "./c99-stdint-1.c"
 ))) == -(((
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 )))-1 && ((((
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 ))) & 1) && ((((((
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 ))) >> 1) + 1) >> (sizeof(int) * 8 
# 224 "./c99-stdint-1.c"
 - 2)) == 1) : (((
# 224 "./c99-stdint-1.c" 3 4
 (-2147483647 - 1)
# 224 "./c99-stdint-1.c"
 )) == 0 && ((((
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 ))) == (int)-1))) && ((int)-1 < 0 ? ((
# 224 "./c99-stdint-1.c" 3 4
 (-2147483647 - 1)
# 224 "./c99-stdint-1.c"
 ) <= (-127) && (
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 ) >= (127)) : (
# 224 "./c99-stdint-1.c" 3 4
 (2147483647)
# 224 "./c99-stdint-1.c"
 ) >= (255))) ? 1 : -1]; } while (0);
  do { int a[(((unsigned int)-1 < 0 ? ((((
# 225 "./c99-stdint-1.c" 3 4
 (0u)
# 225 "./c99-stdint-1.c"
 ))) == -(((
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 )))-1 && ((((
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 ))) & 1) && ((((((
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 ))) >> 1) + 1) >> (sizeof(unsigned int) * 8 
# 225 "./c99-stdint-1.c"
 - 2)) == 1) : (((
# 225 "./c99-stdint-1.c" 3 4
 (0u)
# 225 "./c99-stdint-1.c"
 )) == 0 && ((((
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 ))) == (unsigned int)-1))) && ((unsigned int)-1 < 0 ? ((
# 225 "./c99-stdint-1.c" 3 4
 (0u)
# 225 "./c99-stdint-1.c"
 ) <= (-32767) && (
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 ) >= (32767)) : (
# 225 "./c99-stdint-1.c" 3 4
 (4294967295u)
# 225 "./c99-stdint-1.c"
 ) >= (65535))) ? 1 : -1]; } while (0);
}

void
test_constants (void)
{
  do { __typeof__(01) a; __typeof__((int_least8_t)0 + 0) *b = &a; } while (0); do { __typeof__(2) a; __typeof__((int_least8_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3) a; __typeof__((int_least8_t)0 + 0) *b = &a; } while (0); do { int a[(12 == 12 && 012 == 012 && 0x12 == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01) a; __typeof__((int_least16_t)0 + 0) *b = &a; } while (0); do { __typeof__(2) a; __typeof__((int_least16_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3) a; __typeof__((int_least16_t)0 + 0) *b = &a; } while (0); do { int a[(12 == 12 && 012 == 012 && 0x12 == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01) a; __typeof__((int_least32_t)0 + 0) *b = &a; } while (0); do { __typeof__(2) a; __typeof__((int_least32_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3) a; __typeof__((int_least32_t)0 + 0) *b = &a; } while (0); do { int a[(12 == 12 && 012 == 012 && 0x12 == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01L) a; __typeof__((int_least64_t)0 + 0) *b = &a; } while (0); do { __typeof__(2L) a; __typeof__((int_least64_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3L) a; __typeof__((int_least64_t)0 + 0) *b = &a; } while (0); do { int a[(12L == 12 && 012L == 012 && 0x12L == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01L) a; __typeof__((intmax_t)0 + 0) *b = &a; } while (0); do { __typeof__(2L) a; __typeof__((intmax_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3L) a; __typeof__((intmax_t)0 + 0) *b = &a; } while (0); do { int a[(12L == 12 && 012L == 012 && 0x12L == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01) a; __typeof__((uint_least8_t)0 + 0) *b = &a; } while (0); do { __typeof__(2) a; __typeof__((uint_least8_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3) a; __typeof__((uint_least8_t)0 + 0) *b = &a; } while (0); do { int a[(12 == 12 && 012 == 012 && 0x12 == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01) a; __typeof__((uint_least16_t)0 + 0) *b = &a; } while (0); do { __typeof__(2) a; __typeof__((uint_least16_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3) a; __typeof__((uint_least16_t)0 + 0) *b = &a; } while (0); do { int a[(12 == 12 && 012 == 012 && 0x12 == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01U) a; __typeof__((uint_least32_t)0 + 0) *b = &a; } while (0); do { __typeof__(2U) a; __typeof__((uint_least32_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3U) a; __typeof__((uint_least32_t)0 + 0) *b = &a; } while (0); do { int a[(12U == 12 && 012U == 012 && 0x12U == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01UL) a; __typeof__((uint_least64_t)0 + 0) *b = &a; } while (0); do { __typeof__(2UL) a; __typeof__((uint_least64_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3UL) a; __typeof__((uint_least64_t)0 + 0) *b = &a; } while (0); do { int a[(12UL == 12 && 012UL == 012 && 0x12UL == 0x12) ? 1 : -1]; } while (0);;
  do { __typeof__(01UL) a; __typeof__((uintmax_t)0 + 0) *b = &a; } while (0); do { __typeof__(2UL) a; __typeof__((uintmax_t)0 + 0) *b = &a; } while (0); do { __typeof__(0x3UL) a; __typeof__((uintmax_t)0 + 0) *b = &a; } while (0); do { int a[(12UL == 12 && 012UL == 012 && 0x12UL == 0x12) ? 1 : -1]; } while (0);;
# 271 "./c99-stdint-1.c"
}
