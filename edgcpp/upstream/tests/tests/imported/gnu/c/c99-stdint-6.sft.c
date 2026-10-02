//type: fp
//options: --c99 --strict_gnu
# 0 "./c99-stdint-6.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c99-stdint-6.c"






# 1 "/usr/include/inttypes.h" 1 3 4
# 25 "/usr/include/inttypes.h" 3 4
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
# 26 "/usr/include/inttypes.h" 2 3 4

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
# 28 "/usr/include/inttypes.h" 2 3 4






typedef int __gwchar_t;
# 267 "/usr/include/inttypes.h" 3 4





typedef struct
  {
    long int quot;
    long int rem;
  } imaxdiv_t;
# 291 "/usr/include/inttypes.h" 3 4
extern intmax_t imaxabs (intmax_t __n) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));


extern imaxdiv_t imaxdiv (intmax_t __numer, intmax_t __denom)
      __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));


extern intmax_t strtoimax (const char *__restrict __nptr,
      char **__restrict __endptr, int __base) __attribute__ ((__nothrow__ , __leaf__));


extern uintmax_t strtoumax (const char *__restrict __nptr,
       char ** __restrict __endptr, int __base) __attribute__ ((__nothrow__ , __leaf__));


extern intmax_t wcstoimax (const __gwchar_t *__restrict __nptr,
      __gwchar_t **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__));


extern uintmax_t wcstoumax (const __gwchar_t *__restrict __nptr,
       __gwchar_t ** __restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__));
# 433 "/usr/include/inttypes.h" 3 4

# 8 "./c99-stdint-6.c" 2

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




# 10 "./c99-stdint-6.c" 2






# 15 "./c99-stdint-6.c"
void
check_types (void)
{

  do { signed char a; int8_t *b = &a; int8_t c; signed char *d = &c; } while (0);


  do { short int a; int16_t *b = &a; int16_t c; short int *d = &c; } while (0);


  do { int a; int32_t *b = &a; int32_t c; int *d = &c; } while (0);


  do { long int a; int64_t *b = &a; int64_t c; long int *d = &c; } while (0);


  do { unsigned char a; uint8_t *b = &a; uint8_t c; unsigned char *d = &c; } while (0);


  do { short unsigned int a; uint16_t *b = &a; uint16_t c; short unsigned int *d = &c; } while (0);


  do { unsigned int a; uint32_t *b = &a; uint32_t c; unsigned int *d = &c; } while (0);


  do { long unsigned int a; uint64_t *b = &a; uint64_t c; long unsigned int *d = &c; } while (0);


  do { signed char a; int_least8_t *b = &a; int_least8_t c; signed char *d = &c; } while (0);
  do { short int a; int_least16_t *b = &a; int_least16_t c; short int *d = &c; } while (0);
  do { int a; int_least32_t *b = &a; int_least32_t c; int *d = &c; } while (0);
  do { long int a; int_least64_t *b = &a; int_least64_t c; long int *d = &c; } while (0);
  do { unsigned char a; uint_least8_t *b = &a; uint_least8_t c; unsigned char *d = &c; } while (0);
  do { short unsigned int a; uint_least16_t *b = &a; uint_least16_t c; short unsigned int *d = &c; } while (0);
  do { unsigned int a; uint_least32_t *b = &a; uint_least32_t c; unsigned int *d = &c; } while (0);
  do { long unsigned int a; uint_least64_t *b = &a; uint_least64_t c; long unsigned int *d = &c; } while (0);


  do { signed char a; int_fast8_t *b = &a; int_fast8_t c; signed char *d = &c; } while (0);
  do { long int a; int_fast16_t *b = &a; int_fast16_t c; long int *d = &c; } while (0);
  do { long int a; int_fast32_t *b = &a; int_fast32_t c; long int *d = &c; } while (0);
  do { long int a; int_fast64_t *b = &a; int_fast64_t c; long int *d = &c; } while (0);
  do { unsigned char a; uint_fast8_t *b = &a; uint_fast8_t c; unsigned char *d = &c; } while (0);
  do { long unsigned int a; uint_fast16_t *b = &a; uint_fast16_t c; long unsigned int *d = &c; } while (0);
  do { long unsigned int a; uint_fast32_t *b = &a; uint_fast32_t c; long unsigned int *d = &c; } while (0);
  do { long unsigned int a; uint_fast64_t *b = &a; uint_fast64_t c; long unsigned int *d = &c; } while (0);


  do { long int a; intptr_t *b = &a; intptr_t c; long int *d = &c; } while (0);


  do { long unsigned int a; uintptr_t *b = &a; uintptr_t c; long unsigned int *d = &c; } while (0);


  do { long int a; intmax_t *b = &a; intmax_t c; long int *d = &c; } while (0);
  do { long unsigned int a; uintmax_t *b = &a; uintmax_t c; long unsigned int *d = &c; } while (0);


  do { int a; sig_atomic_t *b = &a; sig_atomic_t c; int *d = &c; } while (0);

}
