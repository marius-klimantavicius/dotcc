//type: lp
//options: 
# 0 "./builtins-20.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./builtins-20.c"
# 11 "./builtins-20.c"
# 1 "./builtins-config.h" 1
# 48 "./builtins-config.h"
# 1 "/usr/include/sys/types.h" 1 3 4
# 25 "/usr/include/sys/types.h" 3 4
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
# 26 "/usr/include/sys/types.h" 2 3 4



# 1 "/usr/include/bits/types.h" 1 3 4
# 27 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/bits/types.h" 2 3 4



# 30 "/usr/include/bits/types.h" 3 4
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
# 30 "/usr/include/sys/types.h" 2 3 4



typedef __u_char u_char;
typedef __u_short u_short;
typedef __u_int u_int;
typedef __u_long u_long;
typedef __quad_t quad_t;
typedef __u_quad_t u_quad_t;
typedef __fsid_t fsid_t;




typedef __loff_t loff_t;



typedef __ino_t ino_t;
# 60 "/usr/include/sys/types.h" 3 4
typedef __dev_t dev_t;




typedef __gid_t gid_t;




typedef __mode_t mode_t;




typedef __nlink_t nlink_t;




typedef __uid_t uid_t;





typedef __off_t off_t;
# 98 "/usr/include/sys/types.h" 3 4
typedef __pid_t pid_t;





typedef __id_t id_t;




typedef __ssize_t ssize_t;





typedef __daddr_t daddr_t;
typedef __caddr_t caddr_t;





typedef __key_t key_t;
# 132 "/usr/include/sys/types.h" 3 4
# 1 "/usr/include/time.h" 1 3 4
# 57 "/usr/include/time.h" 3 4


typedef __clock_t clock_t;



# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;



# 91 "/usr/include/time.h" 3 4
typedef __clockid_t clockid_t;
# 103 "/usr/include/time.h" 3 4
typedef __timer_t timer_t;
# 133 "/usr/include/sys/types.h" 2 3 4
# 146 "/usr/include/sys/types.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 147 "/usr/include/sys/types.h" 2 3 4



typedef unsigned long int ulong;
typedef unsigned short int ushort;
typedef unsigned int uint;
# 194 "/usr/include/sys/types.h" 3 4
typedef int int8_t __attribute__ ((__mode__ (__QI__)));
typedef int int16_t __attribute__ ((__mode__ (__HI__)));
typedef int int32_t __attribute__ ((__mode__ (__SI__)));
typedef int int64_t __attribute__ ((__mode__ (__DI__)));


typedef unsigned int u_int8_t __attribute__ ((__mode__ (__QI__)));
typedef unsigned int u_int16_t __attribute__ ((__mode__ (__HI__)));
typedef unsigned int u_int32_t __attribute__ ((__mode__ (__SI__)));
typedef unsigned int u_int64_t __attribute__ ((__mode__ (__DI__)));

typedef int register_t __attribute__ ((__mode__ (__word__)));
# 216 "/usr/include/sys/types.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 60 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/byteswap.h" 1 3 4
# 28 "/usr/include/bits/byteswap.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/bits/byteswap.h" 2 3 4






# 1 "/usr/include/bits/byteswap-16.h" 1 3 4
# 36 "/usr/include/bits/byteswap.h" 2 3 4
# 44 "/usr/include/bits/byteswap.h" 3 4
static __inline unsigned int
__bswap_32 (unsigned int __bsx)
{
  return __builtin_bswap32 (__bsx);
}
# 108 "/usr/include/bits/byteswap.h" 3 4
static __inline __uint64_t
__bswap_64 (__uint64_t __bsx)
{
  return __builtin_bswap64 (__bsx);
}
# 61 "/usr/include/endian.h" 2 3 4
# 217 "/usr/include/sys/types.h" 2 3 4


# 1 "/usr/include/sys/select.h" 1 3 4
# 30 "/usr/include/sys/select.h" 3 4
# 1 "/usr/include/bits/select.h" 1 3 4
# 22 "/usr/include/bits/select.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/select.h" 2 3 4
# 31 "/usr/include/sys/select.h" 2 3 4


# 1 "/usr/include/bits/sigset.h" 1 3 4
# 23 "/usr/include/bits/sigset.h" 3 4
typedef int __sig_atomic_t;




typedef struct
  {
    unsigned long int __val[(1024 / (8 * sizeof (unsigned long int)))];
  } __sigset_t;
# 34 "/usr/include/sys/select.h" 2 3 4



typedef __sigset_t sigset_t;





# 1 "/usr/include/time.h" 1 3 4
# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };
# 44 "/usr/include/sys/select.h" 2 3 4

# 1 "/usr/include/bits/time.h" 1 3 4
# 30 "/usr/include/bits/time.h" 3 4
struct timeval
  {
    __time_t tv_sec;
    __suseconds_t tv_usec;
  };
# 46 "/usr/include/sys/select.h" 2 3 4


typedef __suseconds_t suseconds_t;





typedef long int __fd_mask;
# 64 "/usr/include/sys/select.h" 3 4
typedef struct
  {






    __fd_mask __fds_bits[1024 / (8 * (int) sizeof (__fd_mask))];


  } fd_set;






typedef __fd_mask fd_mask;
# 96 "/usr/include/sys/select.h" 3 4

# 106 "/usr/include/sys/select.h" 3 4
extern int select (int __nfds, fd_set *__restrict __readfds,
     fd_set *__restrict __writefds,
     fd_set *__restrict __exceptfds,
     struct timeval *__restrict __timeout);
# 118 "/usr/include/sys/select.h" 3 4
extern int pselect (int __nfds, fd_set *__restrict __readfds,
      fd_set *__restrict __writefds,
      fd_set *__restrict __exceptfds,
      const struct timespec *__restrict __timeout,
      const __sigset_t *__restrict __sigmask);
# 131 "/usr/include/sys/select.h" 3 4

# 220 "/usr/include/sys/types.h" 2 3 4


# 1 "/usr/include/sys/sysmacros.h" 1 3 4
# 29 "/usr/include/sys/sysmacros.h" 3 4


__extension__
extern unsigned int gnu_dev_major (unsigned long long int __dev)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));
__extension__
extern unsigned int gnu_dev_minor (unsigned long long int __dev)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));
__extension__
extern unsigned long long int gnu_dev_makedev (unsigned int __major,
            unsigned int __minor)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));
# 63 "/usr/include/sys/sysmacros.h" 3 4

# 223 "/usr/include/sys/types.h" 2 3 4





typedef __blksize_t blksize_t;






typedef __blkcnt_t blkcnt_t;



typedef __fsblkcnt_t fsblkcnt_t;



typedef __fsfilcnt_t fsfilcnt_t;
# 270 "/usr/include/sys/types.h" 3 4
# 1 "/usr/include/bits/pthreadtypes.h" 1 3 4
# 21 "/usr/include/bits/pthreadtypes.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 22 "/usr/include/bits/pthreadtypes.h" 2 3 4
# 60 "/usr/include/bits/pthreadtypes.h" 3 4
typedef unsigned long int pthread_t;


union pthread_attr_t
{
  char __size[56];
  long int __align;
};

typedef union pthread_attr_t pthread_attr_t;





typedef struct __pthread_internal_list
{
  struct __pthread_internal_list *__prev;
  struct __pthread_internal_list *__next;
} __pthread_list_t;
# 90 "/usr/include/bits/pthreadtypes.h" 3 4
typedef union
{
  struct __pthread_mutex_s
  {
    int __lock;
    unsigned int __count;
    int __owner;

    unsigned int __nusers;



    int __kind;

    short __spins;
    short __elision;
    __pthread_list_t __list;
# 124 "/usr/include/bits/pthreadtypes.h" 3 4
  } __data;
  char __size[40];
  long int __align;
} pthread_mutex_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_mutexattr_t;




typedef union
{
  struct
  {
    int __lock;
    unsigned int __futex;
    __extension__ unsigned long long int __total_seq;
    __extension__ unsigned long long int __wakeup_seq;
    __extension__ unsigned long long int __woken_seq;
    void *__mutex;
    unsigned int __nwaiters;
    unsigned int __broadcast_seq;
  } __data;
  char __size[48];
  __extension__ long long int __align;
} pthread_cond_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_condattr_t;



typedef unsigned int pthread_key_t;



typedef int pthread_once_t;





typedef union
{

  struct
  {
    int __lock;
    unsigned int __nr_readers;
    unsigned int __readers_wakeup;
    unsigned int __writer_wakeup;
    unsigned int __nr_readers_queued;
    unsigned int __nr_writers_queued;
    int __writer;
    int __shared;
    unsigned long int __pad1;
    unsigned long int __pad2;


    unsigned int __flags;

  } __data;
# 211 "/usr/include/bits/pthreadtypes.h" 3 4
  char __size[56];
  long int __align;
} pthread_rwlock_t;

typedef union
{
  char __size[8];
  long int __align;
} pthread_rwlockattr_t;





typedef volatile int pthread_spinlock_t;




typedef union
{
  char __size[32];
  long int __align;
} pthread_barrier_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_barrierattr_t;
# 271 "/usr/include/sys/types.h" 2 3 4



# 49 "./builtins-config.h" 2
# 12 "./builtins-20.c" 2


# 13 "./builtins-20.c"
extern double cos (double);
extern double sin (double);
extern double tan (double);
extern double fabs (double);
extern double atan2 (double, double);
extern double copysign (double, double);
extern double fmin (double, double);
extern double fmax (double, double);
extern double hypot (double, double);
extern double pure (double) __attribute__ ((__pure__));
extern double carg (__complex__ double);
extern __complex__ double ccos (__complex__ double);
extern __complex__ double ctan (__complex__ double);
extern float cosf (float);
extern float sinf (float);
extern float tanf (float);
extern float fabsf (float);
extern float atan2f (float, float);
extern float copysignf (float, float);
extern float fminf (float, float);
extern float fmaxf (float, float);
extern float hypotf (float, float);
extern float puref (float) __attribute__ ((__pure__));
extern float cargf (__complex__ float);
extern __complex__ float ccosf (__complex__ float);
extern __complex__ float ctanf (__complex__ float);
extern long double cosl (long double);
extern long double sinl (long double);
extern long double tanl (long double);
extern long double fabsl (long double);
extern long double atan2l (long double, long double);
extern long double copysignl (long double, long double);
extern long double fminl (long double, long double);
extern long double fmaxl (long double, long double);
extern long double hypotl (long double, long double);
extern long double purel (long double) __attribute__ ((__pure__));
extern long double cargl (__complex__ long double);
extern __complex__ long double ccosl (__complex__ long double);
extern __complex__ long double ctanl (__complex__ long double);

extern void link_error(void);

void test1(double x)
{
  if (cos(x) != cos(-x))
    link_error ();

  if (cos(x) != cos(fabs(x)))
    link_error ();

  if (cos(x) != cos(-fabs(x)))
    link_error ();

  if (cos(tan(x)) != cos(tan(-fabs(x))))
    link_error ();

  if (sin(x)/cos(x) != tan(x))
    link_error ();

  if (cos(x)/sin(x) != 1.0/tan(x))
    link_error ();

  if (tan(x)*cos(x) != sin(x))
    link_error ();

  if (cos(x)*tan(x) != sin(x))
    link_error ();

  if (sin(x)/tan(x) != cos(x))
    link_error ();

  if (tan(x)/sin(x) != 1.0/cos(x))
    link_error ();
}

void test2(double x, double y)
{
  if (-tan(x-y) != tan(y-x))
    link_error ();

  if (-sin(x-y) != sin(y-x))
    link_error ();

  if (cos(-x*y) != cos(x*y))
    link_error ();

  if (cos(x*-y) != cos(x*y))
    link_error ();

  if (cos(-x/y) != cos(x/y))
    link_error ();

  if (cos(x/-y) != cos(x/y))
    link_error ();

  if (cos(-fabs(tan(x/-y))) != cos(tan(x/y)))
    link_error ();

  if (cos(y<10 ? -x : y) != cos(y<10 ? x : y))
    link_error ();

  if (cos(y<10 ? x : -y) != cos(y<10 ? x : y))
    link_error ();

  if (cos(y<10 ? -fabs(x) : tan(x<20 ? -x : -fabs(y)))
      != cos(y<10 ? x : tan(x<20 ? x : y)))
    link_error ();

  if (cos((y*=3, -x)) != cos((y*=3,x)))
    link_error ();

  if (cos(-fabs(tan(x/-y))) != cos(tan(x/y)))
    link_error ();

  if (cos(copysign(x,y)) != cos(x))
    link_error ();

  if (cos(copysign(-fabs(x),y*=2)) != cos((y*=2,x)))
    link_error ();

  if (hypot (x, 0) != fabs(x))
    link_error ();

  if (hypot (0, x) != fabs(x))
    link_error ();

  if (hypot (x, x) != fabs(x) * __builtin_sqrt(2))
    link_error ();

  if (hypot (-x, y) != hypot (x, y))
    link_error ();

  if (hypot (x, -y) != hypot (x, y))
    link_error ();

  if (hypot (-x, -y) != hypot (x, y))
    link_error ();

  if (hypot (fabs(x), y) != hypot (x, y))
    link_error ();

  if (hypot (x, fabs(y)) != hypot (x, y))
    link_error ();

  if (hypot (fabs(x), fabs(y)) != hypot (x, y))
    link_error ();

  if (hypot (-fabs(-x), -fabs(fabs(fabs(-y)))) != hypot (x, y))
    link_error ();

  if (hypot (-x, 0) != fabs(x))
    link_error ();

  if (hypot (-x, x) != fabs(x) * __builtin_sqrt(2))
    link_error ();

  if (hypot (pure(x), -pure(x)) != fabs(pure(x)) * __builtin_sqrt(2))
    link_error ();

  if (hypot (tan(-x), tan(-fabs(y))) != hypot (tan(x), tan(y)))
    link_error ();

  if (fmin (fmax(x,y),y) != y)
    link_error ();

  if (fmin (fmax(y,x),y) != y)
    link_error ();

  if (fmin (x,fmax(x,y)) != x)
    link_error ();

  if (fmin (x,fmax(y,x)) != x)
    link_error ();

  if (fmax (fmin(x,y),y) != y)
    link_error ();

  if (fmax (fmin(y,x),y) != y)
    link_error ();

  if (fmax (x,fmin(x,y)) != x)
    link_error ();

  if (fmax (x,fmin(y,x)) != x)
    link_error ();

  if ((__complex__ double) x != -(__complex__ double) (-x))
    link_error ();

  if (x*1i != -(-x*1i))
    link_error ();

  if (x+(x-y)*1i != -(-x+(y-x)*1i))
    link_error ();

  if (x+(x-y)*1i != -(-x-(x-y)*1i))
    link_error ();

  if (ccos(tan(x)+sin(y)*1i) != ccos(-tan(-x)+-sin(-y)*1i))
    link_error ();

  if (ccos(tan(x)+sin(x-y)*1i) != ccos(-tan(-x)-sin(y-x)*1i))
    link_error ();

  if (-5+x*1i != -~(5+x*1i))
    link_error ();

  if (tan(x)+tan(y)*1i != -~(tan(-x)+tan(y)*1i))
    link_error ();
}

void test3(__complex__ double x, __complex__ double y, int i)
{
  if (carg(x) != atan2(__imag__ x, __real__ x))
    link_error ();

  if (ccos(x) != ccos(-x))
    link_error();

  if (ccos(ctan(x)) != ccos(ctan(-x)))
    link_error();

  if (ctan(x-y) != -ctan(y-x))
    link_error();

  if (ccos(x/y) != ccos(-x/y))
    link_error();

  if (ccos(x/y) != ccos(x/-y))
    link_error();

  if (ccos(x/ctan(y)) != ccos(-x/ctan(-y)))
    link_error();

  if (ccos(x*y) != ccos(-x*y))
    link_error();

  if (ccos(x*y) != ccos(x*-y))
    link_error();

  if (ccos(ctan(x)*y) != ccos(ctan(-x)*-y))
    link_error();

  if (ccos(ctan(x/y)) != ccos(-ctan(x/-y)))
    link_error();

  if (ccos(i ? x : y) != ccos(i ? -x : y))
    link_error();

  if (ccos(i ? x : y) != ccos(i ? x : -y))
    link_error();

  if (ccos(i ? x : ctan(y/x)) != ccos(i ? -x : -ctan(-y/x)))
    link_error();

  if (~x != -~-x)
    link_error();

  if (ccos(~x) != ccos(-~-x))
    link_error();

  if (ctan(~(x-y)) != -ctan(~(y-x)))
    link_error();

  if (ctan(~(x/y)) != -ctan(~(x/-y)))
    link_error();
}

void test1f(float x)
{
  if (cosf(x) != cosf(-x))
    link_error ();

  if (cosf(x) != cosf(fabsf(x)))
    link_error ();

  if (cosf(x) != cosf(-fabsf(x)))
    link_error ();

  if (cosf(tanf(x)) != cosf(tanf(-fabsf(x))))
    link_error ();


  if (sinf(x)/cosf(x) != tanf(x))
    link_error ();

  if (cosf(x)/sinf(x) != 1.0f/tanf(x))
    link_error ();

  if (tanf(x)*cosf(x) != sinf(x))
    link_error ();

  if (cosf(x)*tanf(x) != sinf(x))
    link_error ();

  if (sinf(x)/tanf(x) != cosf(x))
    link_error ();

  if (tanf(x)/sinf(x) != 1.0f/cosf(x))
    link_error ();

}

void test2f(float x, float y)
{
  if (-tanf(x-y) != tanf(y-x))
    link_error ();

  if (-sinf(x-y) != sinf(y-x))
    link_error ();

  if (cosf(-x*y) != cosf(x*y))
    link_error ();

  if (cosf(x*-y) != cosf(x*y))
    link_error ();

  if (cosf(-x/y) != cosf(x/y))
    link_error ();

  if (cosf(x/-y) != cosf(x/y))
    link_error ();

  if (cosf(-fabsf(tanf(x/-y))) != cosf(tanf(x/y)))
    link_error ();

  if (cosf(y<10 ? -x : y) != cosf(y<10 ? x : y))
    link_error ();

  if (cosf(y<10 ? x : -y) != cosf(y<10 ? x : y))
    link_error ();

  if (cosf(y<10 ? -fabsf(x) : tanf(x<20 ? -x : -fabsf(y)))
      != cosf(y<10 ? x : tanf(x<20 ? x : y)))
    link_error ();

  if (cosf((y*=3, -x)) != cosf((y*=3,x)))
    link_error ();

  if (cosf(-fabsf(tanf(x/-y))) != cosf(tanf(x/y)))
    link_error ();

  if (cosf(copysignf(x,y)) != cosf(x))
    link_error ();

  if (cosf(copysignf(-fabsf(x),y*=2)) != cosf((y*=2,x)))
    link_error ();

  if (hypotf (x, 0) != fabsf(x))
    link_error ();

  if (hypotf (0, x) != fabsf(x))
    link_error ();

  if (hypotf (x, x) != fabsf(x) * __builtin_sqrtf(2))
    link_error ();

  if (hypotf (-x, y) != hypotf (x, y))
    link_error ();

  if (hypotf (x, -y) != hypotf (x, y))
    link_error ();

  if (hypotf (-x, -y) != hypotf (x, y))
    link_error ();

  if (hypotf (fabsf(x), y) != hypotf (x, y))
    link_error ();

  if (hypotf (x, fabsf(y)) != hypotf (x, y))
    link_error ();

  if (hypotf (fabsf(x), fabsf(y)) != hypotf (x, y))
    link_error ();

  if (hypotf (-fabsf(-x), -fabsf(fabsf(fabsf(-y)))) != hypotf (x, y))
    link_error ();

  if (hypotf (-x, 0) != fabsf(x))
    link_error ();

  if (hypotf (-x, x) != fabsf(x) * __builtin_sqrtf(2))
    link_error ();

  if (hypotf (puref(x), -puref(x)) != fabsf(puref(x)) * __builtin_sqrtf(2))
    link_error ();

  if (hypotf (tanf(-x), tanf(-fabsf(y))) != hypotf (tanf(x), tanf(y)))
    link_error ();

  if (fminf (fmaxf(x,y),y) != y)
    link_error ();

  if (fminf (fmaxf(y,x),y) != y)
    link_error ();

  if (fminf (x,fmaxf(x,y)) != x)
    link_error ();

  if (fminf (x,fmaxf(y,x)) != x)
    link_error ();

  if (fmaxf (fminf(x,y),y) != y)
    link_error ();

  if (fmaxf (fminf(y,x),y) != y)
    link_error ();

  if (fmaxf (x,fminf(x,y)) != x)
    link_error ();

  if (fmaxf (x,fminf(y,x)) != x)
    link_error ();

  if ((__complex__ float) x != -(__complex__ float) (-x))
    link_error ();

  if (x+(x-y)*1i != -(-x+(y-x)*1i))
    link_error ();

  if (x+(x-y)*1i != -(-x-(x-y)*1i))
    link_error ();

  if (ccosf(tanf(x)+sinf(y)*1i) != ccosf(-tanf(-x)+-sinf(-y)*1i))
    link_error ();

  if (ccosf(tanf(x)+sinf(x-y)*1i) != ccosf(-tanf(-x)-sinf(y-x)*1i))
    link_error ();

  if (-5+x*1i != -~(5+x*1i))
    link_error ();

  if (tanf(x)+tanf(y)*1i != -~(tanf(-x)+tanf(y)*1i))
    link_error ();
}

void test3f(__complex__ float x, __complex__ float y, int i)
{
  if (ccosf(x) != ccosf(-x))
    link_error();

  if (ccosf(ctanf(x)) != ccosf(ctanf(-x)))
    link_error();

  if (ctanf(x-y) != -ctanf(y-x))
    link_error();

  if (ccosf(x/y) != ccosf(-x/y))
    link_error();

  if (ccosf(x/y) != ccosf(x/-y))
    link_error();

  if (ccosf(x/ctanf(y)) != ccosf(-x/ctanf(-y)))
    link_error();

  if (ccosf(x*y) != ccosf(-x*y))
    link_error();

  if (ccosf(x*y) != ccosf(x*-y))
    link_error();

  if (ccosf(ctanf(x)*y) != ccosf(ctanf(-x)*-y))
    link_error();

  if (ccosf(ctanf(x/y)) != ccosf(-ctanf(x/-y)))
    link_error();

  if (ccosf(i ? x : y) != ccosf(i ? -x : y))
    link_error();

  if (ccosf(i ? x : y) != ccosf(i ? x : -y))
    link_error();

  if (ccosf(i ? x : ctanf(y/x)) != ccosf(i ? -x : -ctanf(-y/x)))
    link_error();

  if (~x != -~-x)
    link_error();

  if (ccosf(~x) != ccosf(-~-x))
    link_error();

  if (ctanf(~(x-y)) != -ctanf(~(y-x)))
    link_error();

  if (ctanf(~(x/y)) != -ctanf(~(x/-y)))
    link_error();


  if (cargf(x) != atan2f(__imag__ x, __real__ x))
    link_error ();

}

void test1l(long double x)
{
  if (cosl(x) != cosl(-x))
    link_error ();

  if (cosl(x) != cosl(fabsl(x)))
    link_error ();

  if (cosl(x) != cosl(-fabsl(x)))
    link_error ();

  if (cosl(tanl(x)) != cosl(tanl(-fabsl(x))))
    link_error ();


  if (sinl(x)/cosl(x) != tanl(x))
    link_error ();

  if (cosl(x)/sinl(x) != 1.0l/tanl(x))
    link_error ();

  if (tanl(x)*cosl(x) != sinl(x))
    link_error ();

  if (cosl(x)*tanl(x) != sinl(x))
    link_error ();

  if (sinl(x)/tanl(x) != cosl(x))
    link_error ();

  if (tanl(x)/sinl(x) != 1.0l/cosl(x))
    link_error ();

}

void test2l(long double x, long double y)
{
  if (-tanl(x-y) != tanl(y-x))
    link_error ();

  if (-sinl(x-y) != sinl(y-x))
    link_error ();

  if (cosl(-x*y) != cosl(x*y))
    link_error ();

  if (cosl(x*-y) != cosl(x*y))
    link_error ();

  if (cosl(-x/y) != cosl(x/y))
    link_error ();

  if (cosl(x/-y) != cosl(x/y))
    link_error ();

  if (cosl(-fabsl(tanl(x/-y))) != cosl(tanl(x/y)))
    link_error ();

  if (cosl(y<10 ? -x : y) != cosl(y<10 ? x : y))
    link_error ();

  if (cosl(y<10 ? x : -y) != cosl(y<10 ? x : y))
    link_error ();

  if (cosl(y<10 ? -fabsl(x) : tanl(x<20 ? -x : -fabsl(y)))
      != cosl(y<10 ? x : tanl(x<20 ? x : y)))
    link_error ();

  if (cosl((y*=3, -x)) != cosl((y*=3,x)))
    link_error ();

  if (cosl(-fabsl(tanl(x/-y))) != cosl(tanl(x/y)))
    link_error ();

  if (cosl(copysignl(x,y)) != cosl(x))
    link_error ();

  if (cosl(copysignl(-fabsl(x),y*=2)) != cosl((y*=2,x)))
    link_error ();

  if (hypotl (x, 0) != fabsl(x))
    link_error ();

  if (hypotl (0, x) != fabsl(x))
    link_error ();

  if (hypotl (x, x) != fabsl(x) * __builtin_sqrtl(2))
    link_error ();

  if (hypotl (-x, y) != hypotl (x, y))
    link_error ();

  if (hypotl (x, -y) != hypotl (x, y))
    link_error ();

  if (hypotl (-x, -y) != hypotl (x, y))
    link_error ();

  if (hypotl (fabsl(x), y) != hypotl (x, y))
    link_error ();

  if (hypotl (x, fabsl(y)) != hypotl (x, y))
    link_error ();

  if (hypotl (fabsl(x), fabsl(y)) != hypotl (x, y))
    link_error ();

  if (hypotl (-fabsl(-x), -fabsl(fabsl(fabsl(-y)))) != hypotl (x, y))
    link_error ();

  if (hypotl (-x, 0) != fabsl(x))
    link_error ();

  if (hypotl (-x, x) != fabsl(x) * __builtin_sqrtl(2))
    link_error ();

  if (hypotl (purel(x), -purel(x)) != fabsl(purel(x)) * __builtin_sqrtl(2))
    link_error ();

  if (hypotl (tanl(-x), tanl(-fabsl(y))) != hypotl (tanl(x), tanl(y)))
    link_error ();

  if (fminl (fmaxl(x,y),y) != y)
    link_error ();

  if (fminl (fmaxl(y,x),y) != y)
    link_error ();

  if (fminl (x,fmaxl(x,y)) != x)
    link_error ();

  if (fminl (x,fmaxl(y,x)) != x)
    link_error ();

  if (fmaxl (fminl(x,y),y) != y)
    link_error ();

  if (fmaxl (fminl(y,x),y) != y)
    link_error ();

  if (fmaxl (x,fminl(x,y)) != x)
    link_error ();

  if (fmaxl (x,fminl(y,x)) != x)
    link_error ();

  if ((__complex__ long double) x != -(__complex__ long double) (-x))
    link_error ();

  if (x+(x-y)*1i != -(-x+(y-x)*1i))
    link_error ();

  if (x+(x-y)*1i != -(-x-(x-y)*1i))
    link_error ();

  if (ccosl(tanl(x)+sinl(y)*1i) != ccosl(-tanl(-x)+-sinl(-y)*1i))
    link_error ();

  if (ccosl(tanl(x)+sinl(x-y)*1i) != ccosl(-tanl(-x)-sinl(y-x)*1i))
    link_error ();

  if (-5+x*1i != -~(5+x*1i))
    link_error ();

  if (tanl(x)+tanl(y)*1i != -~(tanl(-x)+tanl(y)*1i))
    link_error ();
}

void test3l(__complex__ long double x, __complex__ long double y, int i)
{
  if (ccosl(x) != ccosl(-x))
    link_error();

  if (ccosl(ctanl(x)) != ccosl(ctanl(-x)))
    link_error();

  if (ctanl(x-y) != -ctanl(y-x))
    link_error();

  if (ccosl(x/y) != ccosl(-x/y))
    link_error();

  if (ccosl(x/y) != ccosl(x/-y))
    link_error();

  if (ccosl(x/ctanl(y)) != ccosl(-x/ctanl(-y)))
    link_error();

  if (ccosl(x*y) != ccosl(-x*y))
    link_error();

  if (ccosl(x*y) != ccosl(x*-y))
    link_error();

  if (ccosl(ctanl(x)*y) != ccosl(ctanl(-x)*-y))
    link_error();

  if (ccosl(ctanl(x/y)) != ccosl(-ctanl(x/-y)))
    link_error();

  if (ccosl(i ? x : y) != ccosl(i ? -x : y))
    link_error();

  if (ccosl(i ? x : y) != ccosl(i ? x : -y))
    link_error();

  if (ccosl(i ? x : ctanl(y/x)) != ccosl(i ? -x : -ctanl(-y/x)))
    link_error();

  if (~x != -~-x)
    link_error();

  if (ccosl(~x) != ccosl(-~-x))
    link_error();

  if (ctanl(~(x-y)) != -ctanl(~(y-x)))
    link_error();

  if (ctanl(~(x/y)) != -ctanl(~(x/-y)))
    link_error();


  if (cargl(x) != atan2l(__imag__ x, __real__ x))
    link_error ();

}

int main()
{
  test1 (1.0);
  test2 (1.0, 2.0);

  test1f (1.0f);
  test2f (1.0f, 2.0f);

  test1l (1.0l);
  test2l (1.0l, 2.0l);

  return 0;
}
