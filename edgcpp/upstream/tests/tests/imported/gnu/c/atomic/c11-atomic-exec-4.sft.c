//type: rp
//options: --c11
# 0 "./atomic/c11-atomic-exec-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/c11-atomic-exec-4.c"
# 10 "./atomic/c11-atomic-exec-4.c"
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
# 11 "./atomic/c11-atomic-exec-4.c" 2
# 1 "/usr/include/pthread.h" 1 3 4
# 22 "/usr/include/pthread.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 23 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/sched.h" 1 3 4
# 26 "/usr/include/sched.h" 3 4
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
# 27 "/usr/include/sched.h" 2 3 4


# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 30 "/usr/include/sched.h" 2 3 4



# 1 "/usr/include/time.h" 1 3 4
# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;

# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };
# 34 "/usr/include/sched.h" 2 3 4


typedef __pid_t pid_t;





# 1 "/usr/include/bits/sched.h" 1 3 4
# 73 "/usr/include/bits/sched.h" 3 4
struct sched_param
  {
    int __sched_priority;
  };


# 96 "/usr/include/bits/sched.h" 3 4








struct __sched_param
  {
    int __sched_priority;
  };
# 119 "/usr/include/bits/sched.h" 3 4
typedef unsigned long int __cpu_mask;






typedef struct
{
  __cpu_mask __bits[1024 / (8 * sizeof (__cpu_mask))];
} cpu_set_t;
# 202 "/usr/include/bits/sched.h" 3 4


extern int __sched_cpucount (size_t __setsize, const cpu_set_t *__setp)
  __attribute__ ((__nothrow__ , __leaf__));
extern cpu_set_t *__sched_cpualloc (size_t __count) __attribute__ ((__nothrow__ , __leaf__)) ;
extern void __sched_cpufree (cpu_set_t *__set) __attribute__ ((__nothrow__ , __leaf__));


# 43 "/usr/include/sched.h" 2 3 4







extern int sched_setparam (__pid_t __pid, const struct sched_param *__param)
     __attribute__ ((__nothrow__ , __leaf__));


extern int sched_getparam (__pid_t __pid, struct sched_param *__param) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_setscheduler (__pid_t __pid, int __policy,
          const struct sched_param *__param) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_getscheduler (__pid_t __pid) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_yield (void) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_get_priority_max (int __algorithm) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_get_priority_min (int __algorithm) __attribute__ ((__nothrow__ , __leaf__));


extern int sched_rr_get_interval (__pid_t __pid, struct timespec *__t) __attribute__ ((__nothrow__ , __leaf__));
# 125 "/usr/include/sched.h" 3 4

# 24 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/time.h" 1 3 4
# 29 "/usr/include/time.h" 3 4








# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 38 "/usr/include/time.h" 2 3 4



# 1 "/usr/include/bits/time.h" 1 3 4
# 42 "/usr/include/time.h" 2 3 4
# 57 "/usr/include/time.h" 3 4


typedef __clock_t clock_t;

# 131 "/usr/include/time.h" 3 4


struct tm
{
  int tm_sec;
  int tm_min;
  int tm_hour;
  int tm_mday;
  int tm_mon;
  int tm_year;
  int tm_wday;
  int tm_yday;
  int tm_isdst;





  long int __tm_gmtoff;
  const char *__tm_zone;

};

# 186 "/usr/include/time.h" 3 4



extern clock_t clock (void) __attribute__ ((__nothrow__ , __leaf__));


extern time_t time (time_t *__timer) __attribute__ ((__nothrow__ , __leaf__));


extern double difftime (time_t __time1, time_t __time0)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));


extern time_t mktime (struct tm *__tp) __attribute__ ((__nothrow__ , __leaf__));





extern size_t strftime (char *__restrict __s, size_t __maxsize,
   const char *__restrict __format,
   const struct tm *__restrict __tp) __attribute__ ((__nothrow__ , __leaf__));

# 236 "/usr/include/time.h" 3 4



extern struct tm *gmtime (const time_t *__timer) __attribute__ ((__nothrow__ , __leaf__));



extern struct tm *localtime (const time_t *__timer) __attribute__ ((__nothrow__ , __leaf__));

# 258 "/usr/include/time.h" 3 4



extern char *asctime (const struct tm *__tp) __attribute__ ((__nothrow__ , __leaf__));


extern char *ctime (const time_t *__timer) __attribute__ ((__nothrow__ , __leaf__));

# 282 "/usr/include/time.h" 3 4
extern char *__tzname[2];
extern int __daylight;
extern long int __timezone;
# 386 "/usr/include/time.h" 3 4
extern int timespec_get (struct timespec *__ts, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 430 "/usr/include/time.h" 3 4

# 25 "/usr/include/pthread.h" 2 3 4

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
# 27 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/setjmp.h" 1 3 4
# 26 "/usr/include/bits/setjmp.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 27 "/usr/include/bits/setjmp.h" 2 3 4




typedef long int __jmp_buf[8];
# 28 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/pthread.h" 2 3 4



enum
{
  PTHREAD_CREATE_JOINABLE,

  PTHREAD_CREATE_DETACHED

};



enum
{
  PTHREAD_MUTEX_TIMED_NP,
  PTHREAD_MUTEX_RECURSIVE_NP,
  PTHREAD_MUTEX_ERRORCHECK_NP,
  PTHREAD_MUTEX_ADAPTIVE_NP
# 59 "/usr/include/pthread.h" 3 4
};
# 166 "/usr/include/pthread.h" 3 4
enum
{
  PTHREAD_INHERIT_SCHED,

  PTHREAD_EXPLICIT_SCHED

};



enum
{
  PTHREAD_SCOPE_SYSTEM,

  PTHREAD_SCOPE_PROCESS

};



enum
{
  PTHREAD_PROCESS_PRIVATE,

  PTHREAD_PROCESS_SHARED

};
# 201 "/usr/include/pthread.h" 3 4
struct _pthread_cleanup_buffer
{
  void (*__routine) (void *);
  void *__arg;
  int __canceltype;
  struct _pthread_cleanup_buffer *__prev;
};


enum
{
  PTHREAD_CANCEL_ENABLE,

  PTHREAD_CANCEL_DISABLE

};
enum
{
  PTHREAD_CANCEL_DEFERRED,

  PTHREAD_CANCEL_ASYNCHRONOUS

};
# 239 "/usr/include/pthread.h" 3 4





extern int pthread_create (pthread_t *__restrict __newthread,
      const pthread_attr_t *__restrict __attr,
      void *(*__start_routine) (void *),
      void *__restrict __arg) __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1, 3)));





extern void pthread_exit (void *__retval) __attribute__ ((__noreturn__));







extern int pthread_join (pthread_t __th, void **__thread_return);
# 282 "/usr/include/pthread.h" 3 4
extern int pthread_detach (pthread_t __th) __attribute__ ((__nothrow__ , __leaf__));



extern pthread_t pthread_self (void) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));


extern int pthread_equal (pthread_t __thread1, pthread_t __thread2)
  __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__const__));







extern int pthread_attr_init (pthread_attr_t *__attr) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_destroy (pthread_attr_t *__attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getdetachstate (const pthread_attr_t *__attr,
     int *__detachstate)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setdetachstate (pthread_attr_t *__attr,
     int __detachstate)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getguardsize (const pthread_attr_t *__attr,
          size_t *__guardsize)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setguardsize (pthread_attr_t *__attr,
          size_t __guardsize)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getschedparam (const pthread_attr_t *__restrict __attr,
           struct sched_param *__restrict __param)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedparam (pthread_attr_t *__restrict __attr,
           const struct sched_param *__restrict
           __param) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_getschedpolicy (const pthread_attr_t *__restrict
     __attr, int *__restrict __policy)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedpolicy (pthread_attr_t *__attr, int __policy)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getinheritsched (const pthread_attr_t *__restrict
      __attr, int *__restrict __inherit)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setinheritsched (pthread_attr_t *__attr,
      int __inherit)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getscope (const pthread_attr_t *__restrict __attr,
      int *__restrict __scope)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setscope (pthread_attr_t *__attr, int __scope)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getstackaddr (const pthread_attr_t *__restrict
          __attr, void **__restrict __stackaddr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2))) __attribute__ ((__deprecated__));





extern int pthread_attr_setstackaddr (pthread_attr_t *__attr,
          void *__stackaddr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) __attribute__ ((__deprecated__));


extern int pthread_attr_getstacksize (const pthread_attr_t *__restrict
          __attr, size_t *__restrict __stacksize)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));




extern int pthread_attr_setstacksize (pthread_attr_t *__attr,
          size_t __stacksize)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 432 "/usr/include/pthread.h" 3 4
extern int pthread_setschedparam (pthread_t __target_thread, int __policy,
      const struct sched_param *__param)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (3)));


extern int pthread_getschedparam (pthread_t __target_thread,
      int *__restrict __policy,
      struct sched_param *__restrict __param)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (2, 3)));


extern int pthread_setschedprio (pthread_t __target_thread, int __prio)
     __attribute__ ((__nothrow__ , __leaf__));
# 497 "/usr/include/pthread.h" 3 4
extern int pthread_once (pthread_once_t *__once_control,
    void (*__init_routine) (void)) __attribute__ ((__nonnull__ (1, 2)));
# 509 "/usr/include/pthread.h" 3 4
extern int pthread_setcancelstate (int __state, int *__oldstate);



extern int pthread_setcanceltype (int __type, int *__oldtype);


extern int pthread_cancel (pthread_t __th);




extern void pthread_testcancel (void);




typedef struct
{
  struct
  {
    __jmp_buf __cancel_jmp_buf;
    int __mask_was_saved;
  } __cancel_jmp_buf[1];
  void *__pad[4];
} __pthread_unwind_buf_t __attribute__ ((__aligned__));
# 543 "/usr/include/pthread.h" 3 4
struct __pthread_cleanup_frame
{
  void (*__cancel_routine) (void *);
  void *__cancel_arg;
  int __do_it;
  int __cancel_type;
};
# 683 "/usr/include/pthread.h" 3 4
extern void __pthread_register_cancel (__pthread_unwind_buf_t *__buf)
     ;
# 695 "/usr/include/pthread.h" 3 4
extern void __pthread_unregister_cancel (__pthread_unwind_buf_t *__buf)
  ;
# 736 "/usr/include/pthread.h" 3 4
extern void __pthread_unwind_next (__pthread_unwind_buf_t *__buf)
     __attribute__ ((__noreturn__))

     __attribute__ ((__weak__))

     ;



struct __jmp_buf_tag;
extern int __sigsetjmp (struct __jmp_buf_tag *__env, int __savemask) __attribute__ ((__nothrow__));





extern int pthread_mutex_init (pthread_mutex_t *__mutex,
          const pthread_mutexattr_t *__mutexattr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_destroy (pthread_mutex_t *__mutex)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_trylock (pthread_mutex_t *__mutex)
     __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_lock (pthread_mutex_t *__mutex)
     __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1)));
# 776 "/usr/include/pthread.h" 3 4
extern int pthread_mutex_unlock (pthread_mutex_t *__mutex)
     __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1)));



extern int pthread_mutex_getprioceiling (const pthread_mutex_t *
      __restrict __mutex,
      int *__restrict __prioceiling)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutex_setprioceiling (pthread_mutex_t *__restrict __mutex,
      int __prioceiling,
      int *__restrict __old_ceiling)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 3)));
# 809 "/usr/include/pthread.h" 3 4
extern int pthread_mutexattr_init (pthread_mutexattr_t *__attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_destroy (pthread_mutexattr_t *__attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getpshared (const pthread_mutexattr_t *
      __restrict __attr,
      int *__restrict __pshared)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setpshared (pthread_mutexattr_t *__attr,
      int __pshared)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 841 "/usr/include/pthread.h" 3 4
extern int pthread_mutexattr_getprotocol (const pthread_mutexattr_t *
       __restrict __attr,
       int *__restrict __protocol)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutexattr_setprotocol (pthread_mutexattr_t *__attr,
       int __protocol)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getprioceiling (const pthread_mutexattr_t *
          __restrict __attr,
          int *__restrict __prioceiling)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setprioceiling (pthread_mutexattr_t *__attr,
          int __prioceiling)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 971 "/usr/include/pthread.h" 3 4
extern int pthread_cond_init (pthread_cond_t *__restrict __cond,
         const pthread_condattr_t *__restrict __cond_attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_destroy (pthread_cond_t *__cond)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_signal (pthread_cond_t *__cond)
     __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_broadcast (pthread_cond_t *__cond)
     __attribute__ ((__nothrow__)) __attribute__ ((__nonnull__ (1)));






extern int pthread_cond_wait (pthread_cond_t *__restrict __cond,
         pthread_mutex_t *__restrict __mutex)
     __attribute__ ((__nonnull__ (1, 2)));
# 1003 "/usr/include/pthread.h" 3 4
extern int pthread_cond_timedwait (pthread_cond_t *__restrict __cond,
       pthread_mutex_t *__restrict __mutex,
       const struct timespec *__restrict __abstime)
     __attribute__ ((__nonnull__ (1, 2, 3)));




extern int pthread_condattr_init (pthread_condattr_t *__attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_destroy (pthread_condattr_t *__attr)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_getpshared (const pthread_condattr_t *
     __restrict __attr,
     int *__restrict __pshared)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_condattr_setpshared (pthread_condattr_t *__attr,
     int __pshared) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));
# 1114 "/usr/include/pthread.h" 3 4
extern int pthread_key_create (pthread_key_t *__key,
          void (*__destr_function) (void *))
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));


extern int pthread_key_delete (pthread_key_t __key) __attribute__ ((__nothrow__ , __leaf__));


extern void *pthread_getspecific (pthread_key_t __key) __attribute__ ((__nothrow__ , __leaf__));


extern int pthread_setspecific (pthread_key_t __key,
    const void *__pointer) __attribute__ ((__nothrow__ , __leaf__)) ;
# 1148 "/usr/include/pthread.h" 3 4
extern int pthread_atfork (void (*__prepare) (void),
      void (*__parent) (void),
      void (*__child) (void)) __attribute__ ((__nothrow__ , __leaf__));
# 1162 "/usr/include/pthread.h" 3 4

# 12 "./atomic/c11-atomic-exec-4.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 13 "./atomic/c11-atomic-exec-4.c" 2
# 1 "/usr/include/stdio.h" 1 3 4
# 29 "/usr/include/stdio.h" 3 4




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 34 "/usr/include/stdio.h" 2 3 4
# 44 "/usr/include/stdio.h" 3 4
struct _IO_FILE;



typedef struct _IO_FILE FILE;





# 64 "/usr/include/stdio.h" 3 4
typedef struct _IO_FILE __FILE;
# 74 "/usr/include/stdio.h" 3 4
# 1 "/usr/include/libio.h" 1 3 4
# 32 "/usr/include/libio.h" 3 4
# 1 "/usr/include/_G_config.h" 1 3 4
# 15 "/usr/include/_G_config.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 16 "/usr/include/_G_config.h" 2 3 4




# 1 "/usr/include/wchar.h" 1 3 4
# 82 "/usr/include/wchar.h" 3 4
typedef struct
{
  int __count;
  union
  {

    unsigned int __wch;



    char __wchb[4];
  } __value;
} __mbstate_t;
# 21 "/usr/include/_G_config.h" 2 3 4
typedef struct
{
  __off_t __pos;
  __mbstate_t __state;
} _G_fpos_t;
typedef struct
{
  __off64_t __pos;
  __mbstate_t __state;
} _G_fpos64_t;
# 33 "/usr/include/libio.h" 2 3 4
# 50 "/usr/include/libio.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 51 "/usr/include/libio.h" 2 3 4
# 145 "/usr/include/libio.h" 3 4
struct _IO_jump_t; struct _IO_FILE;
# 155 "/usr/include/libio.h" 3 4
typedef void _IO_lock_t;





struct _IO_marker {
  struct _IO_marker *_next;
  struct _IO_FILE *_sbuf;



  int _pos;
# 178 "/usr/include/libio.h" 3 4
};


enum __codecvt_result
{
  __codecvt_ok,
  __codecvt_partial,
  __codecvt_error,
  __codecvt_noconv
};
# 246 "/usr/include/libio.h" 3 4
struct _IO_FILE {
  int _flags;




  char* _IO_read_ptr;
  char* _IO_read_end;
  char* _IO_read_base;
  char* _IO_write_base;
  char* _IO_write_ptr;
  char* _IO_write_end;
  char* _IO_buf_base;
  char* _IO_buf_end;

  char *_IO_save_base;
  char *_IO_backup_base;
  char *_IO_save_end;

  struct _IO_marker *_markers;

  struct _IO_FILE *_chain;

  int _fileno;



  int _flags2;

  __off_t _old_offset;



  unsigned short _cur_column;
  signed char _vtable_offset;
  char _shortbuf[1];



  _IO_lock_t *_lock;
# 294 "/usr/include/libio.h" 3 4
  __off64_t _offset;
# 303 "/usr/include/libio.h" 3 4
  void *__pad1;
  void *__pad2;
  void *__pad3;
  void *__pad4;
  size_t __pad5;

  int _mode;

  char _unused2[15 * sizeof (int) - 4 * sizeof (void *) - sizeof (size_t)];

};


typedef struct _IO_FILE _IO_FILE;


struct _IO_FILE_plus;

extern struct _IO_FILE_plus _IO_2_1_stdin_;
extern struct _IO_FILE_plus _IO_2_1_stdout_;
extern struct _IO_FILE_plus _IO_2_1_stderr_;
# 339 "/usr/include/libio.h" 3 4
typedef __ssize_t __io_read_fn (void *__cookie, char *__buf, size_t __nbytes);







typedef __ssize_t __io_write_fn (void *__cookie, const char *__buf,
     size_t __n);







typedef int __io_seek_fn (void *__cookie, __off64_t *__pos, int __w);


typedef int __io_close_fn (void *__cookie);
# 391 "/usr/include/libio.h" 3 4
extern int __underflow (_IO_FILE *);
extern int __uflow (_IO_FILE *);
extern int __overflow (_IO_FILE *, int);
# 435 "/usr/include/libio.h" 3 4
extern int _IO_getc (_IO_FILE *__fp);
extern int _IO_putc (int __c, _IO_FILE *__fp);
extern int _IO_feof (_IO_FILE *__fp) __attribute__ ((__nothrow__ , __leaf__));
extern int _IO_ferror (_IO_FILE *__fp) __attribute__ ((__nothrow__ , __leaf__));

extern int _IO_peekc_locked (_IO_FILE *__fp);





extern void _IO_flockfile (_IO_FILE *) __attribute__ ((__nothrow__ , __leaf__));
extern void _IO_funlockfile (_IO_FILE *) __attribute__ ((__nothrow__ , __leaf__));
extern int _IO_ftrylockfile (_IO_FILE *) __attribute__ ((__nothrow__ , __leaf__));
# 465 "/usr/include/libio.h" 3 4
extern int _IO_vfscanf (_IO_FILE * __restrict, const char * __restrict,
   __gnuc_va_list, int *__restrict);
extern int _IO_vfprintf (_IO_FILE *__restrict, const char *__restrict,
    __gnuc_va_list);
extern __ssize_t _IO_padn (_IO_FILE *, int, __ssize_t);
extern size_t _IO_sgetn (_IO_FILE *, void *, size_t);

extern __off64_t _IO_seekoff (_IO_FILE *, __off64_t, int, int);
extern __off64_t _IO_seekpos (_IO_FILE *, __off64_t, int);

extern void _IO_free_backup_area (_IO_FILE *) __attribute__ ((__nothrow__ , __leaf__));
# 75 "/usr/include/stdio.h" 2 3 4
# 108 "/usr/include/stdio.h" 3 4


typedef _G_fpos_t fpos_t;




# 164 "/usr/include/stdio.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 165 "/usr/include/stdio.h" 2 3 4



extern struct _IO_FILE *stdin;
extern struct _IO_FILE *stdout;
extern struct _IO_FILE *stderr;







extern int remove (const char *__filename) __attribute__ ((__nothrow__ , __leaf__));

extern int rename (const char *__old, const char *__new) __attribute__ ((__nothrow__ , __leaf__));














extern FILE *tmpfile (void) ;
# 209 "/usr/include/stdio.h" 3 4
extern char *tmpnam (char *__s) __attribute__ ((__nothrow__ , __leaf__)) ;

# 232 "/usr/include/stdio.h" 3 4





extern int fclose (FILE *__stream);




extern int fflush (FILE *__stream);

# 266 "/usr/include/stdio.h" 3 4






extern FILE *fopen (const char *__restrict __filename,
      const char *__restrict __modes) ;




extern FILE *freopen (const char *__restrict __filename,
        const char *__restrict __modes,
        FILE *__restrict __stream) ;
# 295 "/usr/include/stdio.h" 3 4

# 329 "/usr/include/stdio.h" 3 4



extern void setbuf (FILE *__restrict __stream, char *__restrict __buf) __attribute__ ((__nothrow__ , __leaf__));



extern int setvbuf (FILE *__restrict __stream, char *__restrict __buf,
      int __modes, size_t __n) __attribute__ ((__nothrow__ , __leaf__));

# 351 "/usr/include/stdio.h" 3 4





extern int fprintf (FILE *__restrict __stream,
      const char *__restrict __format, ...);




extern int printf (const char *__restrict __format, ...);

extern int sprintf (char *__restrict __s,
      const char *__restrict __format, ...) __attribute__ ((__nothrow__));





extern int vfprintf (FILE *__restrict __s, const char *__restrict __format,
       __gnuc_va_list __arg);




extern int vprintf (const char *__restrict __format, __gnuc_va_list __arg);

extern int vsprintf (char *__restrict __s, const char *__restrict __format,
       __gnuc_va_list __arg) __attribute__ ((__nothrow__));





extern int snprintf (char *__restrict __s, size_t __maxlen,
       const char *__restrict __format, ...)
     __attribute__ ((__nothrow__)) __attribute__ ((__format__ (__printf__, 3, 4)));

extern int vsnprintf (char *__restrict __s, size_t __maxlen,
        const char *__restrict __format, __gnuc_va_list __arg)
     __attribute__ ((__nothrow__)) __attribute__ ((__format__ (__printf__, 3, 0)));

# 420 "/usr/include/stdio.h" 3 4





extern int fscanf (FILE *__restrict __stream,
     const char *__restrict __format, ...) ;




extern int scanf (const char *__restrict __format, ...) ;

extern int sscanf (const char *__restrict __s,
     const char *__restrict __format, ...) __attribute__ ((__nothrow__ , __leaf__));
# 443 "/usr/include/stdio.h" 3 4
extern int fscanf (FILE *__restrict __stream, const char *__restrict __format, ...) __asm__ ("" "__isoc99_fscanf")

                               ;
extern int scanf (const char *__restrict __format, ...) __asm__ ("" "__isoc99_scanf")
                              ;
extern int sscanf (const char *__restrict __s, const char *__restrict __format, ...) __asm__ ("" "__isoc99_sscanf") __attribute__ ((__nothrow__ , __leaf__))

                      ;
# 463 "/usr/include/stdio.h" 3 4








extern int vfscanf (FILE *__restrict __s, const char *__restrict __format,
      __gnuc_va_list __arg)
     __attribute__ ((__format__ (__scanf__, 2, 0))) ;





extern int vscanf (const char *__restrict __format, __gnuc_va_list __arg)
     __attribute__ ((__format__ (__scanf__, 1, 0))) ;


extern int vsscanf (const char *__restrict __s,
      const char *__restrict __format, __gnuc_va_list __arg)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__format__ (__scanf__, 2, 0)));
# 494 "/usr/include/stdio.h" 3 4
extern int vfscanf (FILE *__restrict __s, const char *__restrict __format, __gnuc_va_list __arg) __asm__ ("" "__isoc99_vfscanf")



     __attribute__ ((__format__ (__scanf__, 2, 0))) ;
extern int vscanf (const char *__restrict __format, __gnuc_va_list __arg) __asm__ ("" "__isoc99_vscanf")

     __attribute__ ((__format__ (__scanf__, 1, 0))) ;
extern int vsscanf (const char *__restrict __s, const char *__restrict __format, __gnuc_va_list __arg) __asm__ ("" "__isoc99_vsscanf") __attribute__ ((__nothrow__ , __leaf__))



     __attribute__ ((__format__ (__scanf__, 2, 0)));
# 522 "/usr/include/stdio.h" 3 4









extern int fgetc (FILE *__stream);
extern int getc (FILE *__stream);





extern int getchar (void);

# 565 "/usr/include/stdio.h" 3 4








extern int fputc (int __c, FILE *__stream);
extern int putc (int __c, FILE *__stream);





extern int putchar (int __c);

# 617 "/usr/include/stdio.h" 3 4





extern char *fgets (char *__restrict __s, int __n, FILE *__restrict __stream)
     ;
# 640 "/usr/include/stdio.h" 3 4

# 684 "/usr/include/stdio.h" 3 4





extern int fputs (const char *__restrict __s, FILE *__restrict __stream);





extern int puts (const char *__s);






extern int ungetc (int __c, FILE *__stream);






extern size_t fread (void *__restrict __ptr, size_t __size,
       size_t __n, FILE *__restrict __stream) ;




extern size_t fwrite (const void *__restrict __ptr, size_t __size,
        size_t __n, FILE *__restrict __s);

# 744 "/usr/include/stdio.h" 3 4





extern int fseek (FILE *__stream, long int __off, int __whence);




extern long int ftell (FILE *__stream) ;




extern void rewind (FILE *__stream);

# 792 "/usr/include/stdio.h" 3 4






extern int fgetpos (FILE *__restrict __stream, fpos_t *__restrict __pos);




extern int fsetpos (FILE *__stream, const fpos_t *__pos);
# 815 "/usr/include/stdio.h" 3 4

# 824 "/usr/include/stdio.h" 3 4


extern void clearerr (FILE *__stream) __attribute__ ((__nothrow__ , __leaf__));

extern int feof (FILE *__stream) __attribute__ ((__nothrow__ , __leaf__)) ;

extern int ferror (FILE *__stream) __attribute__ ((__nothrow__ , __leaf__)) ;

# 841 "/usr/include/stdio.h" 3 4





extern void perror (const char *__s);






# 1 "/usr/include/bits/sys_errlist.h" 1 3 4
# 854 "/usr/include/stdio.h" 2 3 4
# 943 "/usr/include/stdio.h" 3 4

# 14 "./atomic/c11-atomic-exec-4.c" 2
# 1 "/usr/include/stdlib.h" 1 3 4
# 32 "/usr/include/stdlib.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 33 "/usr/include/stdlib.h" 2 3 4


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

# 206 "/usr/include/stdlib.h" 3 4


__extension__
extern long long int strtoll (const char *__restrict __nptr,
         char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtoull (const char *__restrict __nptr,
     char **__restrict __endptr, int __base)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));

# 372 "/usr/include/stdlib.h" 3 4


extern int rand (void) __attribute__ ((__nothrow__ , __leaf__));

extern void srand (unsigned int __seed) __attribute__ ((__nothrow__ , __leaf__));

# 463 "/usr/include/stdlib.h" 3 4


extern void *malloc (size_t __size) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;

extern void *calloc (size_t __nmemb, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__)) ;










extern void *realloc (void *__ptr, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__warn_unused_result__));

extern void free (void *__ptr) __attribute__ ((__nothrow__ , __leaf__));

# 508 "/usr/include/stdlib.h" 3 4
extern void *aligned_alloc (size_t __alignment, size_t __size)
     __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__malloc__, __alloc_size__ (2)));




extern void abort (void) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));



extern int atexit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));







extern int at_quick_exit (void (*__func) (void)) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1)));



# 538 "/usr/include/stdlib.h" 3 4




extern void exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));





extern void quick_exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));







extern void _Exit (int __status) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__noreturn__));






extern char *getenv (const char *__name) __attribute__ ((__nothrow__ , __leaf__)) __attribute__ ((__nonnull__ (1))) ;

# 711 "/usr/include/stdlib.h" 3 4





extern int system (const char *__command) ;

# 741 "/usr/include/stdlib.h" 3 4
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

# 856 "/usr/include/stdlib.h" 3 4



extern int mblen (const char *__s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int mbtowc (wchar_t *__restrict __pwc,
     const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__)) ;


extern int wctomb (char *__s, wchar_t __wchar) __attribute__ ((__nothrow__ , __leaf__)) ;



extern size_t mbstowcs (wchar_t *__restrict __pwcs,
   const char *__restrict __s, size_t __n) __attribute__ ((__nothrow__ , __leaf__));

extern size_t wcstombs (char *__restrict __s,
   const wchar_t *__restrict __pwcs, size_t __n)
     __attribute__ ((__nothrow__ , __leaf__));

# 951 "/usr/include/stdlib.h" 3 4
# 1 "/usr/include/bits/stdlib-float.h" 1 3 4
# 952 "/usr/include/stdlib.h" 2 3 4
# 964 "/usr/include/stdlib.h" 3 4

# 15 "./atomic/c11-atomic-exec-4.c" 2




# 18 "./atomic/c11-atomic-exec-4.c"
static volatile _Atomic 
# 18 "./atomic/c11-atomic-exec-4.c" 3 4
                       _Bool 
# 18 "./atomic/c11-atomic-exec-4.c"
                            thread_ready;
# 75 "./atomic/c11-atomic-exec-4.c"
static volatile _Atomic uint8_t var_uint8_add = (0); static void * test_thread_uint8_add (void *arg) { thread_ready = 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 75 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_add += 1; } return 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 75 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_add (void) { thread_ready = 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 75 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 75 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_add, 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 75 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_add += 1; sched_yield (); } pthread_join (thread_id, 
# 75 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 75 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_add != ((uint8_t) 20000)) { printf ("uint8_add" " failed\n"); return 1; } else { printf ("uint8_add" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_add_3 = (0); static void * test_thread_uint8_add_3 (void *arg) { thread_ready = 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 76 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_add_3 += 3; } return 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 76 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_add_3 (void) { thread_ready = 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 76 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 76 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_add_3, 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 76 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_add_3 += 3; sched_yield (); } pthread_join (thread_id, 
# 76 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 76 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_add_3 != ((uint8_t) 60000)) { printf ("uint8_add_3" " failed\n"); return 1; } else { printf ("uint8_add_3" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_add = (0); static void * test_thread_uint16_add (void *arg) { thread_ready = 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 77 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_add += 1; } return 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 77 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_add (void) { thread_ready = 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 77 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 77 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_add, 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 77 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_add += 1; sched_yield (); } pthread_join (thread_id, 
# 77 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 77 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_add != ((uint16_t) 20000)) { printf ("uint16_add" " failed\n"); return 1; } else { printf ("uint16_add" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_add_3 = (0); static void * test_thread_uint16_add_3 (void *arg) { thread_ready = 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 78 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_add_3 += 3; } return 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 78 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_add_3 (void) { thread_ready = 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 78 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 78 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_add_3, 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 78 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_add_3 += 3; sched_yield (); } pthread_join (thread_id, 
# 78 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 78 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_add_3 != ((uint16_t) 60000)) { printf ("uint16_add_3" " failed\n"); return 1; } else { printf ("uint16_add_3" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_add = (0); static void * test_thread_uint32_add (void *arg) { thread_ready = 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 79 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_add += 1; } return 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 79 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_add (void) { thread_ready = 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 79 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 79 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_add, 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 79 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_add += 1; sched_yield (); } pthread_join (thread_id, 
# 79 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 79 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_add != ((uint32_t) 20000)) { printf ("uint32_add" " failed\n"); return 1; } else { printf ("uint32_add" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_add_3 = (0); static void * test_thread_uint32_add_3 (void *arg) { thread_ready = 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 80 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_add_3 += 3; } return 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 80 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_add_3 (void) { thread_ready = 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 80 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 80 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_add_3, 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 80 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_add_3 += 3; sched_yield (); } pthread_join (thread_id, 
# 80 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 80 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_add_3 != ((uint32_t) 60000)) { printf ("uint32_add_3" " failed\n"); return 1; } else { printf ("uint32_add_3" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_add = (0); static void * test_thread_uint64_add (void *arg) { thread_ready = 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 81 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_add += 1; } return 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 81 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_add (void) { thread_ready = 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 81 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 81 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_add, 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 81 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_add += 1; sched_yield (); } pthread_join (thread_id, 
# 81 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 81 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_add != ((uint64_t) 20000)) { printf ("uint64_add" " failed\n"); return 1; } else { printf ("uint64_add" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_add_3 = (0); static void * test_thread_uint64_add_3 (void *arg) { thread_ready = 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 82 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_add_3 += 3; } return 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 82 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_add_3 (void) { thread_ready = 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 82 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 82 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_add_3, 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 82 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_add_3 += 3; sched_yield (); } pthread_join (thread_id, 
# 82 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 82 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_add_3 != ((uint64_t) 60000)) { printf ("uint64_add_3" " failed\n"); return 1; } else { printf ("uint64_add_3" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_add_neg = (-10000); static void * test_thread_uint64_add_neg (void *arg) { thread_ready = 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 83 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_add_neg += 1; } return 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 83 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_add_neg (void) { thread_ready = 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 83 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 83 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_add_neg, 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 83 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_add_neg += 1; sched_yield (); } pthread_join (thread_id, 
# 83 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 83 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_add_neg != ((uint64_t) 10000)) { printf ("uint64_add_neg" " failed\n"); return 1; } else { printf ("uint64_add_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_add = (0); static void * test_thread_float_add (void *arg) { thread_ready = 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 84 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_float_add += 1; } return 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 84 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_add (void) { thread_ready = 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 84 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 84 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_add, 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 84 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_float_add += 1; sched_yield (); } pthread_join (thread_id, 
# 84 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 84 "./atomic/c11-atomic-exec-4.c"
); if (var_float_add != (20000)) { printf ("float_add" " failed\n"); return 1; } else { printf ("float_add" " passed\n"); return 0; } }
static volatile _Atomic double var_double_add = (0); static void * test_thread_double_add (void *arg) { thread_ready = 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 85 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_double_add += 1; } return 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 85 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_add (void) { thread_ready = 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 85 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 85 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_add, 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 85 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_double_add += 1; sched_yield (); } pthread_join (thread_id, 
# 85 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 85 "./atomic/c11-atomic-exec-4.c"
); if (var_double_add != (20000)) { printf ("double_add" " failed\n"); return 1; } else { printf ("double_add" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_add = (0); static void * test_thread_long_double_add (void *arg) { thread_ready = 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 86 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_long_double_add += 1; } return 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 86 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_add (void) { thread_ready = 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 86 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 86 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_add, 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 86 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_long_double_add += 1; sched_yield (); } pthread_join (thread_id, 
# 86 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 86 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_add != (20000)) { printf ("long_double_add" " failed\n"); return 1; } else { printf ("long_double_add" " passed\n"); return 0; } }
static volatile _Atomic _Complex float var_complex_float_add = (0); static void * test_thread_complex_float_add (void *arg) { thread_ready = 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 87 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_float_add += 1; } return 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 87 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_float_add (void) { thread_ready = 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 87 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 87 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_float_add, 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 87 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_float_add += 1; sched_yield (); } pthread_join (thread_id, 
# 87 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 87 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_float_add != (20000)) { printf ("complex_float_add" " failed\n"); return 1; } else { printf ("complex_float_add" " passed\n"); return 0; } }
static volatile _Atomic _Complex double var_complex_double_add = (0); static void * test_thread_complex_double_add (void *arg) { thread_ready = 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 88 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_double_add += 1; } return 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 88 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_double_add (void) { thread_ready = 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 88 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 88 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_double_add, 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 88 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_double_add += 1; sched_yield (); } pthread_join (thread_id, 
# 88 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 88 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_double_add != (20000)) { printf ("complex_double_add" " failed\n"); return 1; } else { printf ("complex_double_add" " passed\n"); return 0; } }
static volatile _Atomic _Complex long double var_complex_long_double_add = (0); static void * test_thread_complex_long_double_add (void *arg) { thread_ready = 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 89 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_long_double_add += 1; } return 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 89 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_long_double_add (void) { thread_ready = 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 89 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 89 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_long_double_add, 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 89 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_long_double_add += 1; sched_yield (); } pthread_join (thread_id, 
# 89 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 89 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_long_double_add != (20000)) { printf ("complex_long_double_add" " failed\n"); return 1; } else { printf ("complex_long_double_add" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_postinc = (0); static void * test_thread_uint8_postinc (void *arg) { thread_ready = 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 90 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_postinc ++; } return 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 90 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_postinc (void) { thread_ready = 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 90 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 90 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_postinc, 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 90 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 90 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 90 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_postinc != ((uint8_t) 20000)) { printf ("uint8_postinc" " failed\n"); return 1; } else { printf ("uint8_postinc" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_postinc = (0); static void * test_thread_uint16_postinc (void *arg) { thread_ready = 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 91 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_postinc ++; } return 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 91 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_postinc (void) { thread_ready = 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 91 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 91 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_postinc, 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 91 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 91 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 91 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_postinc != ((uint16_t) 20000)) { printf ("uint16_postinc" " failed\n"); return 1; } else { printf ("uint16_postinc" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_postinc = (0); static void * test_thread_uint32_postinc (void *arg) { thread_ready = 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 92 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_postinc ++; } return 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 92 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_postinc (void) { thread_ready = 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 92 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 92 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_postinc, 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 92 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 92 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 92 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_postinc != ((uint32_t) 20000)) { printf ("uint32_postinc" " failed\n"); return 1; } else { printf ("uint32_postinc" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_postinc = (0); static void * test_thread_uint64_postinc (void *arg) { thread_ready = 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 93 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_postinc ++; } return 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 93 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_postinc (void) { thread_ready = 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 93 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 93 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_postinc, 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 93 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 93 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 93 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_postinc != ((uint64_t) 20000)) { printf ("uint64_postinc" " failed\n"); return 1; } else { printf ("uint64_postinc" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_postinc_neg = (-10000); static void * test_thread_uint64_postinc_neg (void *arg) { thread_ready = 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 94 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_postinc_neg ++; } return 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 94 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_postinc_neg (void) { thread_ready = 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 94 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 94 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_postinc_neg, 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 94 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_postinc_neg ++; sched_yield (); } pthread_join (thread_id, 
# 94 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 94 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_postinc_neg != ((uint64_t) 10000)) { printf ("uint64_postinc_neg" " failed\n"); return 1; } else { printf ("uint64_postinc_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_postinc = (0); static void * test_thread_float_postinc (void *arg) { thread_ready = 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 95 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_float_postinc ++; } return 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 95 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_postinc (void) { thread_ready = 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 95 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 95 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_postinc, 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 95 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_float_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 95 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 95 "./atomic/c11-atomic-exec-4.c"
); if (var_float_postinc != (20000)) { printf ("float_postinc" " failed\n"); return 1; } else { printf ("float_postinc" " passed\n"); return 0; } }
static volatile _Atomic double var_double_postinc = (0); static void * test_thread_double_postinc (void *arg) { thread_ready = 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 96 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_double_postinc ++; } return 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 96 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_postinc (void) { thread_ready = 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 96 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 96 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_postinc, 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 96 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_double_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 96 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 96 "./atomic/c11-atomic-exec-4.c"
); if (var_double_postinc != (20000)) { printf ("double_postinc" " failed\n"); return 1; } else { printf ("double_postinc" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_postinc = (0); static void * test_thread_long_double_postinc (void *arg) { thread_ready = 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 97 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_long_double_postinc ++; } return 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 97 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_postinc (void) { thread_ready = 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 97 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 97 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_postinc, 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 97 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_long_double_postinc ++; sched_yield (); } pthread_join (thread_id, 
# 97 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 97 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_postinc != (20000)) { printf ("long_double_postinc" " failed\n"); return 1; } else { printf ("long_double_postinc" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_preinc = (0); static void * test_thread_uint8_preinc (void *arg) { thread_ready = 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 98 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_uint8_preinc ; } return 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 98 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_preinc (void) { thread_ready = 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 98 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 98 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_preinc, 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 98 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_uint8_preinc ; sched_yield (); } pthread_join (thread_id, 
# 98 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 98 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_preinc != ((uint8_t) 20000)) { printf ("uint8_preinc" " failed\n"); return 1; } else { printf ("uint8_preinc" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_preinc = (0); static void * test_thread_uint16_preinc (void *arg) { thread_ready = 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 99 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_uint16_preinc ; } return 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 99 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_preinc (void) { thread_ready = 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 99 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 99 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_preinc, 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 99 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_uint16_preinc ; sched_yield (); } pthread_join (thread_id, 
# 99 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 99 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_preinc != ((uint16_t) 20000)) { printf ("uint16_preinc" " failed\n"); return 1; } else { printf ("uint16_preinc" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_preinc = (0); static void * test_thread_uint32_preinc (void *arg) { thread_ready = 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 100 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_uint32_preinc ; } return 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 100 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_preinc (void) { thread_ready = 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 100 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 100 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_preinc, 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 100 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_uint32_preinc ; sched_yield (); } pthread_join (thread_id, 
# 100 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 100 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_preinc != ((uint32_t) 20000)) { printf ("uint32_preinc" " failed\n"); return 1; } else { printf ("uint32_preinc" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_preinc = (0); static void * test_thread_uint64_preinc (void *arg) { thread_ready = 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 101 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_uint64_preinc ; } return 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 101 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_preinc (void) { thread_ready = 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 101 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 101 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_preinc, 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 101 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_uint64_preinc ; sched_yield (); } pthread_join (thread_id, 
# 101 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 101 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_preinc != ((uint64_t) 20000)) { printf ("uint64_preinc" " failed\n"); return 1; } else { printf ("uint64_preinc" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_preinc_neg = (-10000); static void * test_thread_uint64_preinc_neg (void *arg) { thread_ready = 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 102 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_uint64_preinc_neg ; } return 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 102 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_preinc_neg (void) { thread_ready = 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 102 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 102 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_preinc_neg, 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 102 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_uint64_preinc_neg ; sched_yield (); } pthread_join (thread_id, 
# 102 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 102 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_preinc_neg != ((uint64_t) 10000)) { printf ("uint64_preinc_neg" " failed\n"); return 1; } else { printf ("uint64_preinc_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_preinc = (0); static void * test_thread_float_preinc (void *arg) { thread_ready = 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 103 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_float_preinc ; } return 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 103 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_preinc (void) { thread_ready = 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 103 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 103 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_preinc, 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 103 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_float_preinc ; sched_yield (); } pthread_join (thread_id, 
# 103 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 103 "./atomic/c11-atomic-exec-4.c"
); if (var_float_preinc != (20000)) { printf ("float_preinc" " failed\n"); return 1; } else { printf ("float_preinc" " passed\n"); return 0; } }
static volatile _Atomic double var_double_preinc = (0); static void * test_thread_double_preinc (void *arg) { thread_ready = 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 104 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_double_preinc ; } return 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 104 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_preinc (void) { thread_ready = 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 104 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 104 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_preinc, 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 104 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_double_preinc ; sched_yield (); } pthread_join (thread_id, 
# 104 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 104 "./atomic/c11-atomic-exec-4.c"
); if (var_double_preinc != (20000)) { printf ("double_preinc" " failed\n"); return 1; } else { printf ("double_preinc" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_preinc = (0); static void * test_thread_long_double_preinc (void *arg) { thread_ready = 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 105 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); ++ var_long_double_preinc ; } return 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 105 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_preinc (void) { thread_ready = 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 105 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 105 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_preinc, 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 105 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { ++ var_long_double_preinc ; sched_yield (); } pthread_join (thread_id, 
# 105 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 105 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_preinc != (20000)) { printf ("long_double_preinc" " failed\n"); return 1; } else { printf ("long_double_preinc" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_sub = (0); static void * test_thread_uint8_sub (void *arg) { thread_ready = 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 106 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_sub -= 1; } return 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 106 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_sub (void) { thread_ready = 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 106 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 106 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_sub, 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 106 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 106 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 106 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_sub != ((uint8_t) -20000)) { printf ("uint8_sub" " failed\n"); return 1; } else { printf ("uint8_sub" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_sub_3 = (0); static void * test_thread_uint8_sub_3 (void *arg) { thread_ready = 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 107 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_sub_3 -= 3; } return 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 107 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_sub_3 (void) { thread_ready = 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 107 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 107 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_sub_3, 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 107 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_sub_3 -= 3; sched_yield (); } pthread_join (thread_id, 
# 107 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 107 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_sub_3 != ((uint8_t) -60000)) { printf ("uint8_sub_3" " failed\n"); return 1; } else { printf ("uint8_sub_3" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_sub = (0); static void * test_thread_uint16_sub (void *arg) { thread_ready = 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 108 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_sub -= 1; } return 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 108 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_sub (void) { thread_ready = 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 108 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 108 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_sub, 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 108 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 108 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 108 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_sub != ((uint16_t) -20000)) { printf ("uint16_sub" " failed\n"); return 1; } else { printf ("uint16_sub" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_sub_3 = (0); static void * test_thread_uint16_sub_3 (void *arg) { thread_ready = 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 109 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_sub_3 -= 3; } return 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 109 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_sub_3 (void) { thread_ready = 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 109 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 109 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_sub_3, 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 109 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_sub_3 -= 3; sched_yield (); } pthread_join (thread_id, 
# 109 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 109 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_sub_3 != ((uint16_t) -60000)) { printf ("uint16_sub_3" " failed\n"); return 1; } else { printf ("uint16_sub_3" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_sub = (0); static void * test_thread_uint32_sub (void *arg) { thread_ready = 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 110 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_sub -= 1; } return 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 110 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_sub (void) { thread_ready = 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 110 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 110 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_sub, 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 110 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 110 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 110 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_sub != ((uint32_t) -20000)) { printf ("uint32_sub" " failed\n"); return 1; } else { printf ("uint32_sub" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_sub_3 = (0); static void * test_thread_uint32_sub_3 (void *arg) { thread_ready = 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 111 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_sub_3 -= 3; } return 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 111 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_sub_3 (void) { thread_ready = 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 111 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 111 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_sub_3, 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 111 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_sub_3 -= 3; sched_yield (); } pthread_join (thread_id, 
# 111 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 111 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_sub_3 != ((uint32_t) -60000)) { printf ("uint32_sub_3" " failed\n"); return 1; } else { printf ("uint32_sub_3" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_sub = (0); static void * test_thread_uint64_sub (void *arg) { thread_ready = 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 112 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_sub -= 1; } return 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 112 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_sub (void) { thread_ready = 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 112 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 112 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_sub, 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 112 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 112 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 112 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_sub != ((uint64_t) -20000)) { printf ("uint64_sub" " failed\n"); return 1; } else { printf ("uint64_sub" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_sub_3 = (0); static void * test_thread_uint64_sub_3 (void *arg) { thread_ready = 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 113 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_sub_3 -= 3; } return 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 113 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_sub_3 (void) { thread_ready = 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 113 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 113 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_sub_3, 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 113 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_sub_3 -= 3; sched_yield (); } pthread_join (thread_id, 
# 113 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 113 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_sub_3 != ((uint64_t) -60000)) { printf ("uint64_sub_3" " failed\n"); return 1; } else { printf ("uint64_sub_3" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_sub_neg = (10000); static void * test_thread_uint64_sub_neg (void *arg) { thread_ready = 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 114 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_sub_neg -= 1; } return 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_sub_neg (void) { thread_ready = 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 114 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_sub_neg, 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_sub_neg -= 1; sched_yield (); } pthread_join (thread_id, 
# 114 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_sub_neg != ((uint64_t) -10000)) { printf ("uint64_sub_neg" " failed\n"); return 1; } else { printf ("uint64_sub_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_sub = (0); static void * test_thread_float_sub (void *arg) { thread_ready = 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 115 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_float_sub -= 1; } return 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 115 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_sub (void) { thread_ready = 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 115 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 115 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_sub, 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 115 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_float_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 115 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 115 "./atomic/c11-atomic-exec-4.c"
); if (var_float_sub != (-20000)) { printf ("float_sub" " failed\n"); return 1; } else { printf ("float_sub" " passed\n"); return 0; } }
static volatile _Atomic double var_double_sub = (0); static void * test_thread_double_sub (void *arg) { thread_ready = 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 116 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_double_sub -= 1; } return 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 116 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_sub (void) { thread_ready = 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 116 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 116 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_sub, 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 116 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_double_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 116 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 116 "./atomic/c11-atomic-exec-4.c"
); if (var_double_sub != (-20000)) { printf ("double_sub" " failed\n"); return 1; } else { printf ("double_sub" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_sub = (0); static void * test_thread_long_double_sub (void *arg) { thread_ready = 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 117 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_long_double_sub -= 1; } return 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_sub (void) { thread_ready = 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 117 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_sub, 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_long_double_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 117 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_sub != (-20000)) { printf ("long_double_sub" " failed\n"); return 1; } else { printf ("long_double_sub" " passed\n"); return 0; } }
static volatile _Atomic _Complex float var_complex_float_sub = (0); static void * test_thread_complex_float_sub (void *arg) { thread_ready = 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 118 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_float_sub -= 1; } return 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 118 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_float_sub (void) { thread_ready = 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 118 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 118 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_float_sub, 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 118 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_float_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 118 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 118 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_float_sub != (-20000)) { printf ("complex_float_sub" " failed\n"); return 1; } else { printf ("complex_float_sub" " passed\n"); return 0; } }
static volatile _Atomic _Complex double var_complex_double_sub = (0); static void * test_thread_complex_double_sub (void *arg) { thread_ready = 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 119 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_double_sub -= 1; } return 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 119 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_double_sub (void) { thread_ready = 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 119 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 119 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_double_sub, 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 119 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_double_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 119 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 119 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_double_sub != (-20000)) { printf ("complex_double_sub" " failed\n"); return 1; } else { printf ("complex_double_sub" " passed\n"); return 0; } }
static volatile _Atomic _Complex long double var_complex_long_double_sub = (0); static void * test_thread_complex_long_double_sub (void *arg) { thread_ready = 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 120 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_complex_long_double_sub -= 1; } return 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 120 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_complex_long_double_sub (void) { thread_ready = 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 120 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 120 "./atomic/c11-atomic-exec-4.c"
, test_thread_complex_long_double_sub, 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 120 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_complex_long_double_sub -= 1; sched_yield (); } pthread_join (thread_id, 
# 120 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 120 "./atomic/c11-atomic-exec-4.c"
); if (var_complex_long_double_sub != (-20000)) { printf ("complex_long_double_sub" " failed\n"); return 1; } else { printf ("complex_long_double_sub" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_postdec = (0); static void * test_thread_uint8_postdec (void *arg) { thread_ready = 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 121 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_postdec --; } return 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_postdec (void) { thread_ready = 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 121 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_postdec, 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_postdec --; sched_yield (); } pthread_join (thread_id, 
# 121 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_postdec != ((uint8_t) -20000)) { printf ("uint8_postdec" " failed\n"); return 1; } else { printf ("uint8_postdec" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_postdec = (0); static void * test_thread_uint16_postdec (void *arg) { thread_ready = 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 122 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_postdec --; } return 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 122 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_postdec (void) { thread_ready = 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 122 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 122 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_postdec, 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 122 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_postdec --; sched_yield (); } pthread_join (thread_id, 
# 122 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 122 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_postdec != ((uint16_t) -20000)) { printf ("uint16_postdec" " failed\n"); return 1; } else { printf ("uint16_postdec" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_postdec = (0); static void * test_thread_uint32_postdec (void *arg) { thread_ready = 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 123 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_postdec --; } return 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 123 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_postdec (void) { thread_ready = 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 123 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 123 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_postdec, 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 123 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_postdec --; sched_yield (); } pthread_join (thread_id, 
# 123 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 123 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_postdec != ((uint32_t) -20000)) { printf ("uint32_postdec" " failed\n"); return 1; } else { printf ("uint32_postdec" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_postdec = (0); static void * test_thread_uint64_postdec (void *arg) { thread_ready = 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 124 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_postdec --; } return 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_postdec (void) { thread_ready = 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 124 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_postdec, 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_postdec --; sched_yield (); } pthread_join (thread_id, 
# 124 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_postdec != ((uint64_t) -20000)) { printf ("uint64_postdec" " failed\n"); return 1; } else { printf ("uint64_postdec" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_postdec_neg = (10000); static void * test_thread_uint64_postdec_neg (void *arg) { thread_ready = 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 125 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_postdec_neg --; } return 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 125 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_postdec_neg (void) { thread_ready = 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 125 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 125 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_postdec_neg, 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 125 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_postdec_neg --; sched_yield (); } pthread_join (thread_id, 
# 125 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 125 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_postdec_neg != ((uint64_t) -10000)) { printf ("uint64_postdec_neg" " failed\n"); return 1; } else { printf ("uint64_postdec_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_postdec = (0); static void * test_thread_float_postdec (void *arg) { thread_ready = 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 126 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_float_postdec --; } return 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 126 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_postdec (void) { thread_ready = 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 126 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 126 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_postdec, 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 126 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_float_postdec --; sched_yield (); } pthread_join (thread_id, 
# 126 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 126 "./atomic/c11-atomic-exec-4.c"
); if (var_float_postdec != (-20000)) { printf ("float_postdec" " failed\n"); return 1; } else { printf ("float_postdec" " passed\n"); return 0; } }
static volatile _Atomic double var_double_postdec = (0); static void * test_thread_double_postdec (void *arg) { thread_ready = 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 127 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_double_postdec --; } return 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_postdec (void) { thread_ready = 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 127 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_postdec, 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_double_postdec --; sched_yield (); } pthread_join (thread_id, 
# 127 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-4.c"
); if (var_double_postdec != (-20000)) { printf ("double_postdec" " failed\n"); return 1; } else { printf ("double_postdec" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_postdec = (0); static void * test_thread_long_double_postdec (void *arg) { thread_ready = 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 128 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_long_double_postdec --; } return 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 128 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_postdec (void) { thread_ready = 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 128 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 128 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_postdec, 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 128 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_long_double_postdec --; sched_yield (); } pthread_join (thread_id, 
# 128 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 128 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_postdec != (-20000)) { printf ("long_double_postdec" " failed\n"); return 1; } else { printf ("long_double_postdec" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_predec = (0); static void * test_thread_uint8_predec (void *arg) { thread_ready = 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 129 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_uint8_predec ; } return 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 129 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_predec (void) { thread_ready = 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 129 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 129 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_predec, 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 129 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_uint8_predec ; sched_yield (); } pthread_join (thread_id, 
# 129 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 129 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_predec != ((uint8_t) -20000)) { printf ("uint8_predec" " failed\n"); return 1; } else { printf ("uint8_predec" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_predec = (0); static void * test_thread_uint16_predec (void *arg) { thread_ready = 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 130 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_uint16_predec ; } return 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_predec (void) { thread_ready = 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 130 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_predec, 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_uint16_predec ; sched_yield (); } pthread_join (thread_id, 
# 130 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_predec != ((uint16_t) -20000)) { printf ("uint16_predec" " failed\n"); return 1; } else { printf ("uint16_predec" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_predec = (0); static void * test_thread_uint32_predec (void *arg) { thread_ready = 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 131 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_uint32_predec ; } return 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 131 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_predec (void) { thread_ready = 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 131 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 131 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_predec, 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 131 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_uint32_predec ; sched_yield (); } pthread_join (thread_id, 
# 131 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 131 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_predec != ((uint32_t) -20000)) { printf ("uint32_predec" " failed\n"); return 1; } else { printf ("uint32_predec" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_predec = (0); static void * test_thread_uint64_predec (void *arg) { thread_ready = 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 132 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_uint64_predec ; } return 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 132 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_predec (void) { thread_ready = 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 132 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 132 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_predec, 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 132 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_uint64_predec ; sched_yield (); } pthread_join (thread_id, 
# 132 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 132 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_predec != ((uint64_t) -20000)) { printf ("uint64_predec" " failed\n"); return 1; } else { printf ("uint64_predec" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_predec_neg = (10000); static void * test_thread_uint64_predec_neg (void *arg) { thread_ready = 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 133 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_uint64_predec_neg ; } return 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 133 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_predec_neg (void) { thread_ready = 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 133 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 133 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_predec_neg, 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 133 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_uint64_predec_neg ; sched_yield (); } pthread_join (thread_id, 
# 133 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 133 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_predec_neg != ((uint64_t) -10000)) { printf ("uint64_predec_neg" " failed\n"); return 1; } else { printf ("uint64_predec_neg" " passed\n"); return 0; } }
static volatile _Atomic float var_float_predec = (0); static void * test_thread_float_predec (void *arg) { thread_ready = 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 134 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_float_predec ; } return 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_float_predec (void) { thread_ready = 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 134 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-4.c"
, test_thread_float_predec, 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_float_predec ; sched_yield (); } pthread_join (thread_id, 
# 134 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-4.c"
); if (var_float_predec != (-20000)) { printf ("float_predec" " failed\n"); return 1; } else { printf ("float_predec" " passed\n"); return 0; } }
static volatile _Atomic double var_double_predec = (0); static void * test_thread_double_predec (void *arg) { thread_ready = 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 135 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_double_predec ; } return 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 135 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_double_predec (void) { thread_ready = 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 135 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 135 "./atomic/c11-atomic-exec-4.c"
, test_thread_double_predec, 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 135 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_double_predec ; sched_yield (); } pthread_join (thread_id, 
# 135 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 135 "./atomic/c11-atomic-exec-4.c"
); if (var_double_predec != (-20000)) { printf ("double_predec" " failed\n"); return 1; } else { printf ("double_predec" " passed\n"); return 0; } }
static volatile _Atomic long double var_long_double_predec = (0); static void * test_thread_long_double_predec (void *arg) { thread_ready = 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 136 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); -- var_long_double_predec ; } return 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 136 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_long_double_predec (void) { thread_ready = 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 136 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 136 "./atomic/c11-atomic-exec-4.c"
, test_thread_long_double_predec, 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 136 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { -- var_long_double_predec ; sched_yield (); } pthread_join (thread_id, 
# 136 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 136 "./atomic/c11-atomic-exec-4.c"
); if (var_long_double_predec != (-20000)) { printf ("long_double_predec" " failed\n"); return 1; } else { printf ("long_double_predec" " passed\n"); return 0; } }
static volatile _Atomic uint8_t var_uint8_mul = (1); static void * test_thread_uint8_mul (void *arg) { thread_ready = 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 137 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint8_mul *= 3; } return 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 137 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint8_mul (void) { thread_ready = 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 137 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 137 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint8_mul, 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 137 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint8_mul *= 3; sched_yield (); } pthread_join (thread_id, 
# 137 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 137 "./atomic/c11-atomic-exec-4.c"
); if (var_uint8_mul != ((uint8_t) 0x81)) { printf ("uint8_mul" " failed\n"); return 1; } else { printf ("uint8_mul" " passed\n"); return 0; } }
static volatile _Atomic uint16_t var_uint16_mul = (1); static void * test_thread_uint16_mul (void *arg) { thread_ready = 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 138 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint16_mul *= 3; } return 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint16_mul (void) { thread_ready = 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 138 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint16_mul, 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint16_mul *= 3; sched_yield (); } pthread_join (thread_id, 
# 138 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-4.c"
); if (var_uint16_mul != ((uint16_t) 0x9681)) { printf ("uint16_mul" " failed\n"); return 1; } else { printf ("uint16_mul" " passed\n"); return 0; } }
static volatile _Atomic uint32_t var_uint32_mul = (1); static void * test_thread_uint32_mul (void *arg) { thread_ready = 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 139 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint32_mul *= 3; } return 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 139 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint32_mul (void) { thread_ready = 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 139 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 139 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint32_mul, 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 139 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint32_mul *= 3; sched_yield (); } pthread_join (thread_id, 
# 139 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 139 "./atomic/c11-atomic-exec-4.c"
); if (var_uint32_mul != ((uint32_t) 0x62b49681U)) { printf ("uint32_mul" " failed\n"); return 1; } else { printf ("uint32_mul" " passed\n"); return 0; } }
static volatile _Atomic uint64_t var_uint64_mul = (1); static void * test_thread_uint64_mul (void *arg) { thread_ready = 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
1
# 140 "./atomic/c11-atomic-exec-4.c"
; for (int i = 0; i < 10000; i++) { sched_yield (); var_uint64_mul *= 3; } return 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 140 "./atomic/c11-atomic-exec-4.c"
; } static int test_main_uint64_mul (void) { thread_ready = 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
0
# 140 "./atomic/c11-atomic-exec-4.c"
; pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 140 "./atomic/c11-atomic-exec-4.c"
, test_thread_uint64_mul, 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 140 "./atomic/c11-atomic-exec-4.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { var_uint64_mul *= 3; sched_yield (); } pthread_join (thread_id, 
# 140 "./atomic/c11-atomic-exec-4.c" 3 4
((void *)0)
# 140 "./atomic/c11-atomic-exec-4.c"
); if (var_uint64_mul != ((uint64_t) 0xcd926beb62b49681ULL)) { printf ("uint64_mul" " failed\n"); return 1; } else { printf ("uint64_mul" " passed\n"); return 0; } }

int
main (void)
{
  int ret = 0;
  ret |= test_main_uint8_add ();
  ret |= test_main_uint8_add_3 ();
  ret |= test_main_uint16_add ();
  ret |= test_main_uint16_add_3 ();
  ret |= test_main_uint32_add ();
  ret |= test_main_uint32_add_3 ();
  ret |= test_main_uint64_add ();
  ret |= test_main_uint64_add_3 ();
  ret |= test_main_uint64_add_neg ();
  ret |= test_main_float_add ();
  ret |= test_main_double_add ();
  ret |= test_main_long_double_add ();
  ret |= test_main_complex_float_add ();
  ret |= test_main_complex_double_add ();
  ret |= test_main_complex_long_double_add ();
  ret |= test_main_uint8_postinc ();
  ret |= test_main_uint16_postinc ();
  ret |= test_main_uint32_postinc ();
  ret |= test_main_uint64_postinc ();
  ret |= test_main_uint64_postinc_neg ();
  ret |= test_main_float_postinc ();
  ret |= test_main_double_postinc ();
  ret |= test_main_long_double_postinc ();
  ret |= test_main_uint8_preinc ();
  ret |= test_main_uint16_preinc ();
  ret |= test_main_uint32_preinc ();
  ret |= test_main_uint64_preinc ();
  ret |= test_main_uint64_preinc_neg ();
  ret |= test_main_float_preinc ();
  ret |= test_main_double_preinc ();
  ret |= test_main_long_double_preinc ();
  ret |= test_main_uint8_sub ();
  ret |= test_main_uint8_sub_3 ();
  ret |= test_main_uint16_sub ();
  ret |= test_main_uint16_sub_3 ();
  ret |= test_main_uint32_sub ();
  ret |= test_main_uint32_sub_3 ();
  ret |= test_main_uint64_sub ();
  ret |= test_main_uint64_sub_3 ();
  ret |= test_main_uint64_sub_neg ();
  ret |= test_main_float_sub ();
  ret |= test_main_double_sub ();
  ret |= test_main_long_double_sub ();
  ret |= test_main_complex_float_sub ();
  ret |= test_main_complex_double_sub ();
  ret |= test_main_complex_long_double_sub ();
  ret |= test_main_uint8_postdec ();
  ret |= test_main_uint16_postdec ();
  ret |= test_main_uint32_postdec ();
  ret |= test_main_uint64_postdec ();
  ret |= test_main_uint64_postdec_neg ();
  ret |= test_main_float_postdec ();
  ret |= test_main_double_postdec ();
  ret |= test_main_long_double_postdec ();
  ret |= test_main_uint8_predec ();
  ret |= test_main_uint16_predec ();
  ret |= test_main_uint32_predec ();
  ret |= test_main_uint64_predec ();
  ret |= test_main_uint64_predec_neg ();
  ret |= test_main_float_predec ();
  ret |= test_main_double_predec ();
  ret |= test_main_long_double_predec ();
  ret |= test_main_uint8_mul ();
  ret |= test_main_uint16_mul ();
  ret |= test_main_uint32_mul ();
  ret |= test_main_uint64_mul ();
  if (ret)
    abort ();
  else
    exit (0);
}
