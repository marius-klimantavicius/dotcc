//type: fp
//options: 
# 0 "./compat/struct-complex-1_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/struct-complex-1_y.c"



# 1 "/usr/include/stdlib.h" 1 3 4
# 24 "/usr/include/stdlib.h" 3 4
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
# 25 "/usr/include/stdlib.h" 2 3 4







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 33 "/usr/include/stdlib.h" 2 3 4








# 1 "/usr/include/bits/waitflags.h" 1 3 4
# 42 "/usr/include/stdlib.h" 2 3 4
# 1 "/usr/include/bits/waitstatus.h" 1 3 4
# 64 "/usr/include/bits/waitstatus.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 60 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/byteswap.h" 1 3 4
# 27 "/usr/include/bits/byteswap.h" 3 4
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
# 28 "/usr/include/bits/byteswap.h" 2 3 4
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
# 65 "/usr/include/bits/waitstatus.h" 2 3 4

union wait
  {
    int w_status;
    struct
      {

 unsigned int __w_termsig:7;
 unsigned int __w_coredump:1;
 unsigned int __w_retcode:8;
 unsigned int:16;







      } __wait_terminated;
    struct
      {

 unsigned int __w_stopval:8;
 unsigned int __w_stopsig:8;
 unsigned int:16;






      } __wait_stopped;
  };
# 43 "/usr/include/stdlib.h" 2 3 4
# 67 "/usr/include/stdlib.h" 3 4
typedef union
  {
    union wait *__uptr;
    int *__iptr;
  } __WAIT_STATUS __attribute__ ((__transparent_union__));
# 95 "/usr/include/stdlib.h" 3 4


typedef struct
  {
    int quot;
    int rem;
  } div_t;



typedef struct
  {
    long int quot;
    long int rem;
  } ldiv_t;







__extension__ typedef struct
  {
    long long int quot;
    long long int rem;
  } lldiv_t;


# 139 "/usr/include/stdlib.h" 3 4
extern size_t __ctype_get_mb_cur_max (void) __attribute__ ((__nothrow__ , __leaf__)) ;




extern double atof (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern int atoi (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern long int atol (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





__extension__ extern long long int atoll (const char *__nptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





extern double strtod (const char *__restrict __nptr,
        char **__restrict __endptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





extern float strtof (const char *__restrict __nptr,
       char **__restrict __endptr) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

extern long double strtold (const char *__restrict __nptr,
       char **__restrict __endptr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





extern long int strtol (const char *__restrict __nptr,
   char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

extern unsigned long int strtoul (const char *__restrict __nptr,
      char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));




__extension__
extern long long int strtoq (const char *__restrict __nptr,
        char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtouq (const char *__restrict __nptr,
           char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





__extension__
extern long long int strtoll (const char *__restrict __nptr,
         char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtoull (const char *__restrict __nptr,
     char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

# 305 "/usr/include/stdlib.h" 3 4
extern char *l64a (long int __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern long int a64l (const char *__s)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;




# 1 "/usr/include/sys/types.h" 1 3 4
# 27 "/usr/include/sys/types.h" 3 4






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
# 219 "/usr/include/sys/types.h" 3 4
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



# 315 "/usr/include/stdlib.h" 2 3 4






extern long int random (void) __attribute__ ((__nothrow__ , __leaf__));


extern void srandom (unsigned int __seed) __attribute__ ((__nothrow__ , __leaf__));





extern char *initstate (unsigned int __seed, char *__statebuf,
   size_t __statelen) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));



extern char *setstate (char *__statebuf) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));







struct random_data
  {
    int32_t *fptr;
    int32_t *rptr;
    int32_t *state;
    int rand_type;
    int rand_deg;
    int rand_sep;
    int32_t *end_ptr;
  };

extern int random_r (struct random_data *__restrict __buf,
       int32_t *__restrict __result) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));

extern int srandom_r (unsigned int __seed, struct random_data *__buf)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));

extern int initstate_r (unsigned int __seed, char *__restrict __statebuf,
   size_t __statelen,
   struct random_data *__restrict __buf)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2, 4)));

extern int setstate_r (char *__restrict __statebuf,
         struct random_data *__restrict __buf)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));






extern int rand (void) __attribute__ ((__nothrow__ , __leaf__));

extern void srand (unsigned int __seed) __attribute__ ((__nothrow__ , __leaf__));




extern int rand_r (unsigned int *__seed) __attribute__ ((__nothrow__ , __leaf__));







extern double drand48 (void) __attribute__ ((__nothrow__ , __leaf__));
extern double erand48 (unsigned short int __xsubi[3]) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern long int lrand48 (void) __attribute__ ((__nothrow__ , __leaf__));
extern long int nrand48 (unsigned short int __xsubi[3])
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern long int mrand48 (void) __attribute__ ((__nothrow__ , __leaf__));
extern long int jrand48 (unsigned short int __xsubi[3])
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern void srand48 (long int __seedval) __attribute__ ((__nothrow__ , __leaf__));
extern unsigned short int *seed48 (unsigned short int __seed16v[3])
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
extern void lcong48 (unsigned short int __param[7]) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





struct drand48_data
  {
    unsigned short int __x[3];
    unsigned short int __old_x[3];
    unsigned short int __c;
    unsigned short int __init;
    unsigned long long int __a;
  };


extern int drand48_r (struct drand48_data *__restrict __buffer,
        double *__restrict __result) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
extern int erand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        double *__restrict __result) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int lrand48_r (struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
extern int nrand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int mrand48_r (struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));
extern int jrand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int srand48_r (long int __seedval, struct drand48_data *__buffer)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));

extern int seed48_r (unsigned short int __seed16v[3],
       struct drand48_data *__buffer) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));

extern int lcong48_r (unsigned short int __param[7],
        struct drand48_data *__buffer)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));









extern void *malloc (size_t __size) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;

extern void *calloc (size_t __nmemb, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;










extern void *realloc (void *__ptr, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__warn_unused_result__));

extern void free (void *__ptr) __attribute__ ((__nothrow__ , __leaf__));




extern void cfree (void *__ptr) __attribute__ ((__nothrow__ , __leaf__));



# 1 "/usr/include/alloca.h" 1 3 4
# 24 "/usr/include/alloca.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 25 "/usr/include/alloca.h" 2 3 4







extern void *alloca (size_t __size) __attribute__ ((__nothrow__ , __leaf__));






# 492 "/usr/include/stdlib.h" 2 3 4





extern void *valloc (size_t __size) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;




extern int posix_memalign (void **__memptr, size_t __alignment, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;




extern void *aligned_alloc (size_t __alignment, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__, __alloc_size__ (2)));




extern void abort (void) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



extern int atexit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));







extern int at_quick_exit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));







extern int on_exit (void (*__func) (int __status, void *__arg), void *__arg)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));






extern void exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));





extern void quick_exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));







extern void _Exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));






extern char *getenv (const char *__name) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;

# 577 "/usr/include/stdlib.h" 3 4
extern int putenv (char *__string) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));





extern int setenv (const char *__name, const char *__value, int __replace)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2)));


extern int unsetenv (const char *__name) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));






extern int clearenv (void) __attribute__ ((__nothrow__ , __leaf__));
# 605 "/usr/include/stdlib.h" 3 4
extern char *mktemp (char *__template) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 619 "/usr/include/stdlib.h" 3 4
extern int mkstemp (char *__template) __attribute__ ((__nonnull__ (1))) ;
# 641 "/usr/include/stdlib.h" 3 4
extern int mkstemps (char *__template, int __suffixlen) __attribute__ ((__nonnull__ (1))) ;
# 662 "/usr/include/stdlib.h" 3 4
extern char *mkdtemp (char *__template) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;
# 711 "/usr/include/stdlib.h" 3 4





extern int system (const char *__command) ;

# 733 "/usr/include/stdlib.h" 3 4
extern char *realpath (const char *__restrict __name,
         char *__restrict __resolved) __attribute__ ((__nothrow__ , __leaf__)) ;






typedef int (*__compar_fn_t) (const void *, const void *);
# 751 "/usr/include/stdlib.h" 3 4



extern void *bsearch (const void *__key, const void *__base,
        size_t __nmemb, size_t __size, __compar_fn_t __compar)
     __attribute__ ((__nonnull__ (1, 2, 5))) ;



extern void qsort (void *__base, size_t __nmemb, size_t __size,
     __compar_fn_t __compar) __attribute__ ((__nonnull__ (1, 4)));
# 770 "/usr/include/stdlib.h" 3 4
extern int abs (int __x) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;
extern long int labs (long int __x) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;



__extension__ extern long long int llabs (long long int __x)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;







extern div_t div (int __numer, int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;
extern ldiv_t ldiv (long int __numer, long int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;




__extension__ extern lldiv_t lldiv (long long int __numer,
        long long int __denom)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__)) ;

# 807 "/usr/include/stdlib.h" 3 4
extern char *ecvt (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4))) ;




extern char *fcvt (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4))) ;




extern char *gcvt (double __value, int __ndigit, char *__buf)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3))) ;




extern char *qecvt (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4))) ;
extern char *qfcvt (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4))) ;
extern char *qgcvt (long double __value, int __ndigit, char *__buf)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3))) ;




extern int ecvt_r (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign, char *__restrict __buf,
     size_t __len) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4, 5)));
extern int fcvt_r (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign, char *__restrict __buf,
     size_t __len) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4, 5)));

extern int qecvt_r (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign,
      char *__restrict __buf, size_t __len)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4, 5)));
extern int qfcvt_r (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign,
      char *__restrict __buf, size_t __len)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3, 4, 5)));







extern int mblen (const char *__s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int mbtowc (wchar_t *__restrict __pwc,
     const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int wctomb (char *__s, wchar_t __wchar) __attribute__ ((__nothrow__ , __leaf__)) ;



extern size_t mbstowcs (wchar_t *__restrict __pwcs,
   const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__));

extern size_t wcstombs (char *__restrict __s,
   const wchar_t *__restrict __pwcs, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__));








extern int rpmatch (const char *__response) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;
# 895 "/usr/include/stdlib.h" 3 4
extern int getsubopt (char **__restrict __optionp,
        char *const *__restrict __tokens,
        char **__restrict __valuep)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2, 3))) ;
# 947 "/usr/include/stdlib.h" 3 4
extern int getloadavg (double __loadavg[], int __nelem)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


# 1 "/usr/include/bits/stdlib-float.h" 1 3 4
# 952 "/usr/include/stdlib.h" 2 3 4
# 964 "/usr/include/stdlib.h" 3 4

# 5 "./compat/struct-complex-1_y.c" 2
# 1 "./compat/struct-complex-1.h" 1
# 1 "/usr/include/complex.h" 1 3 4
# 28 "/usr/include/complex.h" 3 4
# 1 "/usr/include/bits/mathdef.h" 1 3 4
# 29 "/usr/include/complex.h" 2 3 4


# 75 "/usr/include/complex.h" 3 4
# 1 "/usr/include/bits/cmathcalls.h" 1 3 4
# 53 "/usr/include/bits/cmathcalls.h" 3 4
extern double _Complex cacos (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __cacos (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex casin (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __casin (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex catan (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __catan (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex ccos (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __ccos (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex csin (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __csin (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex ctan (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __ctan (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern double _Complex cacosh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __cacosh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex casinh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __casinh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex catanh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __catanh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex ccosh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __ccosh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex csinh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __csinh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern double _Complex ctanh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __ctanh (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern double _Complex cexp (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __cexp (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex clog (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __clog (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 101 "/usr/include/bits/cmathcalls.h" 3 4
extern double _Complex cpow (double _Complex __x, double _Complex __y) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __cpow (double _Complex __x, double _Complex __y) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex csqrt (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __csqrt (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern double cabs (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double __cabs (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double carg (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double __carg (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex conj (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __conj (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double _Complex cproj (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double _Complex __cproj (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern double cimag (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double __cimag (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern double creal (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern double __creal (double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 76 "/usr/include/complex.h" 2 3 4
# 85 "/usr/include/complex.h" 3 4
# 1 "/usr/include/bits/cmathcalls.h" 1 3 4
# 53 "/usr/include/bits/cmathcalls.h" 3 4
extern float _Complex cacosf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __cacosf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex casinf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __casinf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex catanf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __catanf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex ccosf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __ccosf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex csinf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __csinf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex ctanf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __ctanf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern float _Complex cacoshf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __cacoshf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex casinhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __casinhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex catanhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __catanhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex ccoshf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __ccoshf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex csinhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __csinhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern float _Complex ctanhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __ctanhf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern float _Complex cexpf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __cexpf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex clogf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __clogf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 101 "/usr/include/bits/cmathcalls.h" 3 4
extern float _Complex cpowf (float _Complex __x, float _Complex __y) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __cpowf (float _Complex __x, float _Complex __y) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex csqrtf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __csqrtf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern float cabsf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float __cabsf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float cargf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float __cargf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex conjf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __conjf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float _Complex cprojf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float _Complex __cprojf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern float cimagf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float __cimagf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern float crealf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern float __crealf (float _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 86 "/usr/include/complex.h" 2 3 4
# 104 "/usr/include/complex.h" 3 4
# 1 "/usr/include/bits/cmathcalls.h" 1 3 4
# 53 "/usr/include/bits/cmathcalls.h" 3 4
extern long double _Complex cacosl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __cacosl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex casinl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __casinl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex catanl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __catanl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex ccosl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __ccosl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex csinl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __csinl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex ctanl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __ctanl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern long double _Complex cacoshl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __cacoshl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex casinhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __casinhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex catanhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __catanhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex ccoshl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __ccoshl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex csinhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __csinhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));

extern long double _Complex ctanhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __ctanhl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern long double _Complex cexpl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __cexpl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex clogl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __clogl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 101 "/usr/include/bits/cmathcalls.h" 3 4
extern long double _Complex cpowl (long double _Complex __x, long double _Complex __y) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __cpowl (long double _Complex __x, long double _Complex __y) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex csqrtl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __csqrtl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern long double cabsl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double __cabsl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double cargl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double __cargl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex conjl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __conjl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double _Complex cprojl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double _Complex __cprojl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));





extern long double cimagl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double __cimagl (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));


extern long double creall (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__)); extern long double __creall (long double _Complex __z) __attribute__ ((__nothrow__ , __leaf__));
# 105 "/usr/include/complex.h" 2 3 4








# 2 "./compat/struct-complex-1.h" 2


# 3 "./compat/struct-complex-1.h"
struct st
{
  int s1;
  float 
# 6 "./compat/struct-complex-1.h" 3 4
       _Complex 
# 6 "./compat/struct-complex-1.h"
               x;
  int s2;
};

typedef struct { float r, i; } _complex;

struct stc
{
  int s1;
  _complex x;
  int s2;
};
# 6 "./compat/struct-complex-1_y.c" 2

void
bar(struct st x)
{
  if (x.s1 != 1 || x.s2 != 2
      || __real__ x.x != 2 || __imag__ x.x != 4)
    abort ();
}

void
foo(struct stc x)
{
  if (x.s1 != 1 || x.s2 != 2 || x.x.r != 2 || x.x.i != 4)
    abort ();
}
