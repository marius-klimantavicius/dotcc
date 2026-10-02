//type: rp
//options: --c11
# 0 "./atomic/c11-atomic-exec-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/c11-atomic-exec-5.c"
# 15 "./atomic/c11-atomic-exec-5.c"
# 1 "/usr/include/fenv.h" 1 3 4
# 25 "/usr/include/fenv.h" 3 4
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
# 26 "/usr/include/fenv.h" 2 3 4
# 57 "/usr/include/fenv.h" 3 4
# 1 "/usr/include/bits/fenv.h" 1 3 4
# 24 "/usr/include/bits/fenv.h" 3 4

# 24 "/usr/include/bits/fenv.h" 3 4
enum
  {
    FE_INVALID =

      0x01,
    __FE_DENORM = 0x02,
    FE_DIVBYZERO =

      0x04,
    FE_OVERFLOW =

      0x08,
    FE_UNDERFLOW =

      0x10,
    FE_INEXACT =

      0x20
  };







enum
  {
    FE_TONEAREST =

      0,
    FE_DOWNWARD =

      0x400,
    FE_UPWARD =

      0x800,
    FE_TOWARDZERO =

      0xc00
  };



typedef unsigned short int fexcept_t;






typedef struct
  {
    unsigned short int __control_word;
    unsigned short int __unused1;
    unsigned short int __status_word;
    unsigned short int __unused2;
    unsigned short int __tags;
    unsigned short int __unused3;
    unsigned int __eip;
    unsigned short int __cs_selector;
    unsigned int __opcode:11;
    unsigned int __unused4:5;
    unsigned int __data_offset;
    unsigned short int __data_selector;
    unsigned short int __unused5;

    unsigned int __mxcsr;

  }
fenv_t;
# 58 "/usr/include/fenv.h" 2 3 4






extern int feclearexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fegetexceptflag (fexcept_t *__flagp, int __excepts) __attribute__ ((__nothrow__ , __leaf__));


extern int feraiseexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fesetexceptflag (const fexcept_t *__flagp, int __excepts) __attribute__ ((__nothrow__ , __leaf__));



extern int fetestexcept (int __excepts) __attribute__ ((__nothrow__ , __leaf__));





extern int fegetround (void) __attribute__ ((__nothrow__ , __leaf__));


extern int fesetround (int __rounding_direction) __attribute__ ((__nothrow__ , __leaf__));






extern int fegetenv (fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));




extern int feholdexcept (fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));



extern int fesetenv (const fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));




extern int feupdateenv (const fenv_t *__envp) __attribute__ ((__nothrow__ , __leaf__));
# 133 "/usr/include/fenv.h" 3 4

# 16 "./atomic/c11-atomic-exec-5.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 17 "./atomic/c11-atomic-exec-5.c" 2
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

# 18 "./atomic/c11-atomic-exec-5.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 19 "./atomic/c11-atomic-exec-5.c" 2
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

# 20 "./atomic/c11-atomic-exec-5.c" 2
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

# 21 "./atomic/c11-atomic-exec-5.c" 2
# 34 "./atomic/c11-atomic-exec-5.c"

# 34 "./atomic/c11-atomic-exec-5.c"
static volatile _Atomic 
# 34 "./atomic/c11-atomic-exec-5.c" 3 4
                       _Bool 
# 34 "./atomic/c11-atomic-exec-5.c"
                            thread_ready, thread_stop;
# 114 "./atomic/c11-atomic-exec-5.c"
static volatile _Atomic float var_float_add_invalid; static void * test_thread_float_add_invalid (void *arg) { thread_ready = 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 114 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_invalid = (0); sched_yield (); var_float_add_invalid = (-__builtin_inff ()); sched_yield (); } return 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_invalid (void) { thread_stop = 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 114 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 114 "./atomic/c11-atomic-exec-5.c"
; var_float_add_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_invalid, 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 114 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_invalid += __builtin_inff ()); int rexc = fetestexcept ((
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 114 "./atomic/c11-atomic-exec-5.c"
| 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 114 "./atomic/c11-atomic-exec-5.c"
| 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 114 "./atomic/c11-atomic-exec-5.c"
| 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 114 "./atomic/c11-atomic-exec-5.c"
| 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 114 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_float_add_invalid = (-__builtin_inff ()); } else { if (rexc == ((0) | (
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 114 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_float_add_invalid = (0); } } thread_stop = 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 114 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 114 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 114 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_add_invalid_prev; static void * test_thread_float_add_invalid_prev (void *arg) { thread_ready = 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 117 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_invalid_prev = (0); sched_yield (); var_float_add_invalid_prev = (-__builtin_inff ()); sched_yield (); } return 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_invalid_prev (void) { thread_stop = 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 117 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 117 "./atomic/c11-atomic-exec-5.c"
; var_float_add_invalid_prev = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_invalid_prev, 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 117 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 117 "./atomic/c11-atomic-exec-5.c"
); float r = ( var_float_add_invalid_prev += __builtin_inff ()); int rexc = fetestexcept ((
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 117 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 117 "./atomic/c11-atomic-exec-5.c"
) | (0))) num_1_pass++; else num_1_fail++; var_float_add_invalid_prev = (-__builtin_inff ()); } else { if (rexc == ((
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 117 "./atomic/c11-atomic-exec-5.c"
| 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 117 "./atomic/c11-atomic-exec-5.c"
) | (
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 117 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_float_add_invalid_prev = (0); } } thread_stop = 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 117 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 117 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 117 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_invalid_prev" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_add_overflow; static void * test_thread_float_add_overflow (void *arg) { thread_ready = 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 121 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 121 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_add_overflow = (0); sched_yield (); } return 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_overflow (void) { thread_stop = 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 121 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 121 "./atomic/c11-atomic-exec-5.c"
; var_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 121 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_overflow, 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 121 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_overflow += 3.40282346638528859811704183484516925e+38F
# 121 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 121 "./atomic/c11-atomic-exec-5.c"
| 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 121 "./atomic/c11-atomic-exec-5.c"
| 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 121 "./atomic/c11-atomic-exec-5.c"
| 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 121 "./atomic/c11-atomic-exec-5.c"
| 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 121 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 121 "./atomic/c11-atomic-exec-5.c"
| 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 121 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 121 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 121 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 121 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 121 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_add_overflow_prev; static void * test_thread_float_add_overflow_prev (void *arg) { thread_ready = 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 124 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_overflow_prev = (3.40282346638528859811704183484516925e+38F
# 124 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_add_overflow_prev = (0); sched_yield (); } return 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_overflow_prev (void) { thread_stop = 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 124 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 124 "./atomic/c11-atomic-exec-5.c"
; var_float_add_overflow_prev = (3.40282346638528859811704183484516925e+38F
# 124 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_overflow_prev, 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 124 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 124 "./atomic/c11-atomic-exec-5.c"
); float r = ( var_float_add_overflow_prev += 3.40282346638528859811704183484516925e+38F
# 124 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 124 "./atomic/c11-atomic-exec-5.c"
| 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 124 "./atomic/c11-atomic-exec-5.c"
| 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 124 "./atomic/c11-atomic-exec-5.c"
| 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 124 "./atomic/c11-atomic-exec-5.c"
| 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 124 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 124 "./atomic/c11-atomic-exec-5.c"
) | (
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 124 "./atomic/c11-atomic-exec-5.c"
| 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 124 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_overflow_prev = (0); } else { if (rexc == ((
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 124 "./atomic/c11-atomic-exec-5.c"
) | (0))) num_2_pass++; else num_2_fail++; var_float_add_overflow_prev = (3.40282346638528859811704183484516925e+38F
# 124 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 124 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 124 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 124 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_overflow_prev" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_add_overflow_double; static void * test_thread_float_add_overflow_double (void *arg) { thread_ready = 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 127 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_overflow_double = (3.40282346638528859811704183484516925e+38F
# 127 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_add_overflow_double = (0); sched_yield (); } return 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_overflow_double (void) { thread_stop = 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 127 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 127 "./atomic/c11-atomic-exec-5.c"
; var_float_add_overflow_double = (3.40282346638528859811704183484516925e+38F
# 127 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_overflow_double, 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 127 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_overflow_double += (double) 3.40282346638528859811704183484516925e+38F
# 127 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 127 "./atomic/c11-atomic-exec-5.c"
| 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 127 "./atomic/c11-atomic-exec-5.c"
| 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 127 "./atomic/c11-atomic-exec-5.c"
| 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 127 "./atomic/c11-atomic-exec-5.c"
| 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 127 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 127 "./atomic/c11-atomic-exec-5.c"
| 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 127 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_overflow_double = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_add_overflow_double = (3.40282346638528859811704183484516925e+38F
# 127 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 127 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 127 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 127 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_overflow_double" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_add_overflow_long_double; static void * test_thread_float_add_overflow_long_double (void *arg) { thread_ready = 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 130 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_overflow_long_double = (3.40282346638528859811704183484516925e+38F
# 130 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_add_overflow_long_double = (0); sched_yield (); } return 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_overflow_long_double (void) { thread_stop = 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 130 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 130 "./atomic/c11-atomic-exec-5.c"
; var_float_add_overflow_long_double = (3.40282346638528859811704183484516925e+38F
# 130 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_overflow_long_double, 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 130 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_overflow_long_double += (long double) 3.40282346638528859811704183484516925e+38F
# 130 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 130 "./atomic/c11-atomic-exec-5.c"
| 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 130 "./atomic/c11-atomic-exec-5.c"
| 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 130 "./atomic/c11-atomic-exec-5.c"
| 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 130 "./atomic/c11-atomic-exec-5.c"
| 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 130 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 130 "./atomic/c11-atomic-exec-5.c"
| 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 130 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_overflow_long_double = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_add_overflow_long_double = (3.40282346638528859811704183484516925e+38F
# 130 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 130 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 130 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 130 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_overflow_long_double" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_add_inexact; static void * test_thread_float_add_inexact (void *arg) { thread_ready = 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 134 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_inexact = (1.0f); sched_yield (); var_float_add_inexact = (0); sched_yield (); } return 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_inexact (void) { thread_stop = 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 134 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 134 "./atomic/c11-atomic-exec-5.c"
; var_float_add_inexact = (1.0f); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_inexact, 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 134 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_inexact += 1.19209289550781250000000000000000000e-7F 
# 134 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 134 "./atomic/c11-atomic-exec-5.c"
| 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 134 "./atomic/c11-atomic-exec-5.c"
| 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 134 "./atomic/c11-atomic-exec-5.c"
| 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 134 "./atomic/c11-atomic-exec-5.c"
| 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 134 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 1.19209289550781250000000000000000000e-7F 
# 134 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 134 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_add_inexact = (1.0f); } } thread_stop = 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 134 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 134 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 134 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_add_inexact_int; static void * test_thread_float_add_inexact_int (void *arg) { thread_ready = 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 138 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_add_inexact_int = (1.19209289550781250000000000000000000e-7F 
# 138 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_add_inexact_int = (-1); sched_yield (); } return 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_add_inexact_int (void) { thread_stop = 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 138 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 138 "./atomic/c11-atomic-exec-5.c"
; var_float_add_inexact_int = (1.19209289550781250000000000000000000e-7F 
# 138 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_add_inexact_int, 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 138 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_add_inexact_int += 1); int rexc = fetestexcept ((
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 138 "./atomic/c11-atomic-exec-5.c"
| 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 138 "./atomic/c11-atomic-exec-5.c"
| 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 138 "./atomic/c11-atomic-exec-5.c"
| 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 138 "./atomic/c11-atomic-exec-5.c"
| 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 138 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 138 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_add_inexact_int = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_add_inexact_int = (1.19209289550781250000000000000000000e-7F 
# 138 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 138 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 138 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 138 "./atomic/c11-atomic-exec-5.c"
); printf ("float_add_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_preinc_inexact; static void * test_thread_float_preinc_inexact (void *arg) { thread_ready = 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 141 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_preinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 141 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_preinc_inexact = (-1); sched_yield (); } return 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 141 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_preinc_inexact (void) { thread_stop = 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 141 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 141 "./atomic/c11-atomic-exec-5.c"
; var_float_preinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 141 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 141 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_preinc_inexact, 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 141 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 141 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = (++ var_float_preinc_inexact ); int rexc = fetestexcept ((
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 141 "./atomic/c11-atomic-exec-5.c"
| 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 141 "./atomic/c11-atomic-exec-5.c"
| 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 141 "./atomic/c11-atomic-exec-5.c"
| 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 141 "./atomic/c11-atomic-exec-5.c"
| 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 141 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 141 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_preinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_preinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 141 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 141 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 141 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 141 "./atomic/c11-atomic-exec-5.c"
); printf ("float_preinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_postinc_inexact; static void * test_thread_float_postinc_inexact (void *arg) { thread_ready = 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 145 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_postinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 145 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_postinc_inexact = (-1); sched_yield (); } return 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 145 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_postinc_inexact (void) { thread_stop = 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 145 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 145 "./atomic/c11-atomic-exec-5.c"
; var_float_postinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 145 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 145 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_postinc_inexact, 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 145 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 145 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_postinc_inexact ++); int rexc = fetestexcept ((
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 145 "./atomic/c11-atomic-exec-5.c"
| 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 145 "./atomic/c11-atomic-exec-5.c"
| 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 145 "./atomic/c11-atomic-exec-5.c"
| 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 145 "./atomic/c11-atomic-exec-5.c"
| 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 145 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -1)) { if (rexc == ((0) | (
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 145 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_postinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_postinc_inexact = (1.19209289550781250000000000000000000e-7F 
# 145 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 145 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 145 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 145 "./atomic/c11-atomic-exec-5.c"
); printf ("float_postinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long var_long_add_float_inexact; static void * test_thread_long_add_float_inexact (void *arg) { thread_ready = 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 149 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_add_float_inexact = (1); sched_yield (); var_long_add_float_inexact = (-2 / 1.19209289550781250000000000000000000e-7F
# 149 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 149 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_add_float_inexact (void) { thread_stop = 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 149 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 149 "./atomic/c11-atomic-exec-5.c"
; var_long_add_float_inexact = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 149 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_add_float_inexact, 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 149 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 149 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long r = ( var_long_add_float_inexact += 2 / 1.19209289550781250000000000000000000e-7F
# 149 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 149 "./atomic/c11-atomic-exec-5.c"
| 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 149 "./atomic/c11-atomic-exec-5.c"
| 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 149 "./atomic/c11-atomic-exec-5.c"
| 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 149 "./atomic/c11-atomic-exec-5.c"
| 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 149 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 149 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_add_float_inexact = (-2 / 1.19209289550781250000000000000000000e-7F
# 149 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_add_float_inexact = (1); } } thread_stop = 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 149 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 149 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 149 "./atomic/c11-atomic-exec-5.c"
); printf ("long_add_float_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }




static volatile _Atomic _Complex float var_complex_float_add_overflow; static void * test_thread_complex_float_add_overflow (void *arg) { thread_ready = 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 154 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 154 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_float_add_overflow = (0); sched_yield (); } return 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 154 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_float_add_overflow (void) { thread_stop = 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 154 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 154 "./atomic/c11-atomic-exec-5.c"
; var_complex_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 154 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 154 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_float_add_overflow, 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 154 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 154 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex float r = ( var_complex_float_add_overflow += 3.40282346638528859811704183484516925e+38F
# 154 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 154 "./atomic/c11-atomic-exec-5.c"
| 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 154 "./atomic/c11-atomic-exec-5.c"
| 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 154 "./atomic/c11-atomic-exec-5.c"
| 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 154 "./atomic/c11-atomic-exec-5.c"
| 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 154 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 154 "./atomic/c11-atomic-exec-5.c"
| 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 154 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_float_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_float_add_overflow = (3.40282346638528859811704183484516925e+38F
# 154 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 154 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 154 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 154 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_float_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_sub_invalid; static void * test_thread_float_sub_invalid (void *arg) { thread_ready = 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 157 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_sub_invalid = (0); sched_yield (); var_float_sub_invalid = (__builtin_inff ()); sched_yield (); } return 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 157 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_sub_invalid (void) { thread_stop = 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 157 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 157 "./atomic/c11-atomic-exec-5.c"
; var_float_sub_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 157 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_sub_invalid, 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 157 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 157 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_sub_invalid -= __builtin_inff ()); int rexc = fetestexcept ((
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 157 "./atomic/c11-atomic-exec-5.c"
| 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 157 "./atomic/c11-atomic-exec-5.c"
| 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 157 "./atomic/c11-atomic-exec-5.c"
| 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 157 "./atomic/c11-atomic-exec-5.c"
| 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 157 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_float_sub_invalid = (__builtin_inff ()); } else { if (rexc == ((0) | (
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 157 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_float_sub_invalid = (0); } } thread_stop = 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 157 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 157 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 157 "./atomic/c11-atomic-exec-5.c"
); printf ("float_sub_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_sub_overflow; static void * test_thread_float_sub_overflow (void *arg) { thread_ready = 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 160 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 160 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_sub_overflow = (0); sched_yield (); } return 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 160 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_sub_overflow (void) { thread_stop = 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 160 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 160 "./atomic/c11-atomic-exec-5.c"
; var_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 160 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 160 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_sub_overflow, 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 160 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 160 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_sub_overflow -= 3.40282346638528859811704183484516925e+38F
# 160 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 160 "./atomic/c11-atomic-exec-5.c"
| 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 160 "./atomic/c11-atomic-exec-5.c"
| 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 160 "./atomic/c11-atomic-exec-5.c"
| 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 160 "./atomic/c11-atomic-exec-5.c"
| 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 160 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 160 "./atomic/c11-atomic-exec-5.c"
| 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 160 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 160 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 160 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 160 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 160 "./atomic/c11-atomic-exec-5.c"
); printf ("float_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_sub_inexact; static void * test_thread_float_sub_inexact (void *arg) { thread_ready = 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 164 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_sub_inexact = (-1.0f); sched_yield (); var_float_sub_inexact = (0); sched_yield (); } return 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 164 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_sub_inexact (void) { thread_stop = 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 164 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 164 "./atomic/c11-atomic-exec-5.c"
; var_float_sub_inexact = (-1.0f); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 164 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_sub_inexact, 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 164 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 164 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_sub_inexact -= 1.19209289550781250000000000000000000e-7F 
# 164 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 164 "./atomic/c11-atomic-exec-5.c"
| 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 164 "./atomic/c11-atomic-exec-5.c"
| 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 164 "./atomic/c11-atomic-exec-5.c"
| 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 164 "./atomic/c11-atomic-exec-5.c"
| 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 164 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -1.19209289550781250000000000000000000e-7F 
# 164 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 164 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_sub_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_sub_inexact = (-1.0f); } } thread_stop = 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 164 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 164 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 164 "./atomic/c11-atomic-exec-5.c"
); printf ("float_sub_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_sub_inexact_int; static void * test_thread_float_sub_inexact_int (void *arg) { thread_ready = 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 168 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_sub_inexact_int = (-1.19209289550781250000000000000000000e-7F 
# 168 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_sub_inexact_int = (1); sched_yield (); } return 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 168 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_sub_inexact_int (void) { thread_stop = 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 168 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 168 "./atomic/c11-atomic-exec-5.c"
; var_float_sub_inexact_int = (-1.19209289550781250000000000000000000e-7F 
# 168 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 168 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_sub_inexact_int, 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 168 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 168 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_sub_inexact_int -= 1); int rexc = fetestexcept ((
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 168 "./atomic/c11-atomic-exec-5.c"
| 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 168 "./atomic/c11-atomic-exec-5.c"
| 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 168 "./atomic/c11-atomic-exec-5.c"
| 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 168 "./atomic/c11-atomic-exec-5.c"
| 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 168 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 168 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_sub_inexact_int = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_sub_inexact_int = (-1.19209289550781250000000000000000000e-7F 
# 168 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 168 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 168 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 168 "./atomic/c11-atomic-exec-5.c"
); printf ("float_sub_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_predec_inexact; static void * test_thread_float_predec_inexact (void *arg) { thread_ready = 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 171 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_predec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 171 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_predec_inexact = (1); sched_yield (); } return 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 171 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_predec_inexact (void) { thread_stop = 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 171 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 171 "./atomic/c11-atomic-exec-5.c"
; var_float_predec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 171 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 171 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_predec_inexact, 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 171 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 171 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = (-- var_float_predec_inexact ); int rexc = fetestexcept ((
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 171 "./atomic/c11-atomic-exec-5.c"
| 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 171 "./atomic/c11-atomic-exec-5.c"
| 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 171 "./atomic/c11-atomic-exec-5.c"
| 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 171 "./atomic/c11-atomic-exec-5.c"
| 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 171 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 171 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_predec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_predec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 171 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 171 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 171 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 171 "./atomic/c11-atomic-exec-5.c"
); printf ("float_predec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_postdec_inexact; static void * test_thread_float_postdec_inexact (void *arg) { thread_ready = 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 175 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_postdec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 175 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_float_postdec_inexact = (1); sched_yield (); } return 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 175 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_postdec_inexact (void) { thread_stop = 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 175 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 175 "./atomic/c11-atomic-exec-5.c"
; var_float_postdec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 175 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 175 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_postdec_inexact, 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 175 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 175 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_postdec_inexact --); int rexc = fetestexcept ((
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 175 "./atomic/c11-atomic-exec-5.c"
| 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 175 "./atomic/c11-atomic-exec-5.c"
| 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 175 "./atomic/c11-atomic-exec-5.c"
| 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 175 "./atomic/c11-atomic-exec-5.c"
| 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 175 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 1)) { if (rexc == ((0) | (
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 175 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_postdec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_postdec_inexact = (-1.19209289550781250000000000000000000e-7F 
# 175 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 175 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 175 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 175 "./atomic/c11-atomic-exec-5.c"
); printf ("float_postdec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long var_long_sub_float_inexact; static void * test_thread_long_sub_float_inexact (void *arg) { thread_ready = 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 179 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_sub_float_inexact = (-1); sched_yield (); var_long_sub_float_inexact = (2 / 1.19209289550781250000000000000000000e-7F
# 179 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 179 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_sub_float_inexact (void) { thread_stop = 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 179 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 179 "./atomic/c11-atomic-exec-5.c"
; var_long_sub_float_inexact = (-1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 179 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_sub_float_inexact, 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 179 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 179 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long r = ( var_long_sub_float_inexact -= 2 / 1.19209289550781250000000000000000000e-7F
# 179 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 179 "./atomic/c11-atomic-exec-5.c"
| 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 179 "./atomic/c11-atomic-exec-5.c"
| 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 179 "./atomic/c11-atomic-exec-5.c"
| 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 179 "./atomic/c11-atomic-exec-5.c"
| 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 179 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 179 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_sub_float_inexact = (2 / 1.19209289550781250000000000000000000e-7F
# 179 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_sub_float_inexact = (-1); } } thread_stop = 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 179 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 179 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 179 "./atomic/c11-atomic-exec-5.c"
); printf ("long_sub_float_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex float var_complex_float_sub_overflow; static void * test_thread_complex_float_sub_overflow (void *arg) { thread_ready = 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 183 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 183 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_float_sub_overflow = (0); sched_yield (); } return 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 183 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_float_sub_overflow (void) { thread_stop = 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 183 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 183 "./atomic/c11-atomic-exec-5.c"
; var_complex_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 183 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 183 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_float_sub_overflow, 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 183 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 183 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex float r = ( var_complex_float_sub_overflow -= 3.40282346638528859811704183484516925e+38F
# 183 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 183 "./atomic/c11-atomic-exec-5.c"
| 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 183 "./atomic/c11-atomic-exec-5.c"
| 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 183 "./atomic/c11-atomic-exec-5.c"
| 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 183 "./atomic/c11-atomic-exec-5.c"
| 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 183 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 183 "./atomic/c11-atomic-exec-5.c"
| 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 183 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_float_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_float_sub_overflow = (-3.40282346638528859811704183484516925e+38F
# 183 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 183 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 183 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 183 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_float_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_mul_invalid; static void * test_thread_float_mul_invalid (void *arg) { thread_ready = 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 186 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_mul_invalid = (__builtin_inff ()); sched_yield (); var_float_mul_invalid = (0); sched_yield (); } return 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 186 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_mul_invalid (void) { thread_stop = 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 186 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 186 "./atomic/c11-atomic-exec-5.c"
; var_float_mul_invalid = (__builtin_inff ()); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 186 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_mul_invalid, 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 186 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 186 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_mul_invalid *= __builtin_inff ()); int rexc = fetestexcept ((
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 186 "./atomic/c11-atomic-exec-5.c"
| 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 186 "./atomic/c11-atomic-exec-5.c"
| 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 186 "./atomic/c11-atomic-exec-5.c"
| 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 186 "./atomic/c11-atomic-exec-5.c"
| 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 186 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_float_mul_invalid = (0); } else { if (rexc == ((0) | (
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 186 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_float_mul_invalid = (__builtin_inff ()); } } thread_stop = 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 186 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 186 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 186 "./atomic/c11-atomic-exec-5.c"
); printf ("float_mul_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_mul_overflow; static void * test_thread_float_mul_overflow (void *arg) { thread_ready = 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 189 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 189 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_mul_overflow = (0); sched_yield (); } return 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 189 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_mul_overflow (void) { thread_stop = 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 189 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 189 "./atomic/c11-atomic-exec-5.c"
; var_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 189 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 189 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_mul_overflow, 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 189 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 189 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_mul_overflow *= 3.40282346638528859811704183484516925e+38F
# 189 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 189 "./atomic/c11-atomic-exec-5.c"
| 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 189 "./atomic/c11-atomic-exec-5.c"
| 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 189 "./atomic/c11-atomic-exec-5.c"
| 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 189 "./atomic/c11-atomic-exec-5.c"
| 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 189 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 189 "./atomic/c11-atomic-exec-5.c"
| 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 189 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 189 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 189 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 189 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 189 "./atomic/c11-atomic-exec-5.c"
); printf ("float_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic float var_float_mul_underflow; static void * test_thread_float_mul_underflow (void *arg) { thread_ready = 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 193 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_mul_underflow = (1.17549435082228750796873653722224568e-38F
# 193 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_mul_underflow = (1); sched_yield (); } return 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 193 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_mul_underflow (void) { thread_stop = 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 193 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 193 "./atomic/c11-atomic-exec-5.c"
; var_float_mul_underflow = (1.17549435082228750796873653722224568e-38F
# 193 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 193 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_mul_underflow, 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 193 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 193 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_mul_underflow *= 1.17549435082228750796873653722224568e-38F
# 193 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 193 "./atomic/c11-atomic-exec-5.c"
| 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 193 "./atomic/c11-atomic-exec-5.c"
| 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 193 "./atomic/c11-atomic-exec-5.c"
| 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 193 "./atomic/c11-atomic-exec-5.c"
| 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 193 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 193 "./atomic/c11-atomic-exec-5.c"
| 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 193 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_mul_underflow = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_mul_underflow = (1.17549435082228750796873653722224568e-38F
# 193 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 193 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 193 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 193 "./atomic/c11-atomic-exec-5.c"
); printf ("float_mul_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_mul_inexact; static void * test_thread_float_mul_inexact (void *arg) { thread_ready = 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 196 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_mul_inexact = (1 + 1.19209289550781250000000000000000000e-7F
# 196 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_mul_inexact = (0); sched_yield (); } return 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 196 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_mul_inexact (void) { thread_stop = 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 196 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 196 "./atomic/c11-atomic-exec-5.c"
; var_float_mul_inexact = (1 + 1.19209289550781250000000000000000000e-7F
# 196 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 196 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_mul_inexact, 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 196 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 196 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_mul_inexact *= 1 + 1.19209289550781250000000000000000000e-7F
# 196 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 196 "./atomic/c11-atomic-exec-5.c"
| 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 196 "./atomic/c11-atomic-exec-5.c"
| 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 196 "./atomic/c11-atomic-exec-5.c"
| 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 196 "./atomic/c11-atomic-exec-5.c"
| 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 196 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 196 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_mul_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_mul_inexact = (1 + 1.19209289550781250000000000000000000e-7F
# 196 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 196 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 196 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 196 "./atomic/c11-atomic-exec-5.c"
); printf ("float_mul_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_mul_inexact_int; static void * test_thread_float_mul_inexact_int (void *arg) { thread_ready = 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 199 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_mul_inexact_int = (1 + 1.19209289550781250000000000000000000e-7F
# 199 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_mul_inexact_int = (0); sched_yield (); } return 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 199 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_mul_inexact_int (void) { thread_stop = 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 199 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 199 "./atomic/c11-atomic-exec-5.c"
; var_float_mul_inexact_int = (1 + 1.19209289550781250000000000000000000e-7F
# 199 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 199 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_mul_inexact_int, 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 199 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 199 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_mul_inexact_int *= 3); int rexc = fetestexcept ((
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 199 "./atomic/c11-atomic-exec-5.c"
| 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 199 "./atomic/c11-atomic-exec-5.c"
| 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 199 "./atomic/c11-atomic-exec-5.c"
| 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 199 "./atomic/c11-atomic-exec-5.c"
| 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 199 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 199 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_mul_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_mul_inexact_int = (1 + 1.19209289550781250000000000000000000e-7F
# 199 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 199 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 199 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 199 "./atomic/c11-atomic-exec-5.c"
); printf ("float_mul_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long var_long_mul_float_inexact; static void * test_thread_long_mul_float_inexact (void *arg) { thread_ready = 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 203 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_mul_float_inexact = (1 + 1 / 1.19209289550781250000000000000000000e-7F
# 203 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_mul_float_inexact = (0); sched_yield (); } return 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 203 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_mul_float_inexact (void) { thread_stop = 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 203 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 203 "./atomic/c11-atomic-exec-5.c"
; var_long_mul_float_inexact = (1 + 1 / 1.19209289550781250000000000000000000e-7F
# 203 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 203 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_mul_float_inexact, 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 203 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 203 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long r = ( var_long_mul_float_inexact *= 3.0f); int rexc = fetestexcept ((
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 203 "./atomic/c11-atomic-exec-5.c"
| 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 203 "./atomic/c11-atomic-exec-5.c"
| 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 203 "./atomic/c11-atomic-exec-5.c"
| 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 203 "./atomic/c11-atomic-exec-5.c"
| 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 203 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 203 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_mul_float_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_mul_float_inexact = (1 + 1 / 1.19209289550781250000000000000000000e-7F
# 203 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 203 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 203 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 203 "./atomic/c11-atomic-exec-5.c"
); printf ("long_mul_float_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex float var_complex_float_mul_overflow; static void * test_thread_complex_float_mul_overflow (void *arg) { thread_ready = 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 207 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 207 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_float_mul_overflow = (0); sched_yield (); } return 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 207 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_float_mul_overflow (void) { thread_stop = 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 207 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 207 "./atomic/c11-atomic-exec-5.c"
; var_complex_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 207 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 207 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_float_mul_overflow, 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 207 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 207 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex float r = ( var_complex_float_mul_overflow *= 3.40282346638528859811704183484516925e+38F
# 207 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 207 "./atomic/c11-atomic-exec-5.c"
| 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 207 "./atomic/c11-atomic-exec-5.c"
| 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 207 "./atomic/c11-atomic-exec-5.c"
| 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 207 "./atomic/c11-atomic-exec-5.c"
| 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 207 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 207 "./atomic/c11-atomic-exec-5.c"
| 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 207 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_float_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_float_mul_overflow = (3.40282346638528859811704183484516925e+38F
# 207 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 207 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 207 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 207 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_float_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_div_invalid_divbyzero; static void * test_thread_float_div_invalid_divbyzero (void *arg) { thread_ready = 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 210 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_div_invalid_divbyzero = (1); sched_yield (); var_float_div_invalid_divbyzero = (0); sched_yield (); } return 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 210 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_div_invalid_divbyzero (void) { thread_stop = 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 210 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 210 "./atomic/c11-atomic-exec-5.c"
; var_float_div_invalid_divbyzero = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 210 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_div_invalid_divbyzero, 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 210 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 210 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_div_invalid_divbyzero /= 0.0f); int rexc = fetestexcept ((
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 210 "./atomic/c11-atomic-exec-5.c"
| 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 210 "./atomic/c11-atomic-exec-5.c"
| 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 210 "./atomic/c11-atomic-exec-5.c"
| 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 210 "./atomic/c11-atomic-exec-5.c"
| 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 210 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x04
# 210 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_div_invalid_divbyzero = (0); } else { if (rexc == ((0) | (
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 210 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_float_div_invalid_divbyzero = (1); } } thread_stop = 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 210 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 210 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 210 "./atomic/c11-atomic-exec-5.c"
); printf ("float_div_invalid_divbyzero" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_div_overflow; static void * test_thread_float_div_overflow (void *arg) { thread_ready = 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 213 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 213 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_div_overflow = (0); sched_yield (); } return 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 213 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_div_overflow (void) { thread_stop = 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 213 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 213 "./atomic/c11-atomic-exec-5.c"
; var_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 213 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 213 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_div_overflow, 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 213 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 213 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_div_overflow /= 1.17549435082228750796873653722224568e-38F
# 213 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 213 "./atomic/c11-atomic-exec-5.c"
| 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 213 "./atomic/c11-atomic-exec-5.c"
| 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 213 "./atomic/c11-atomic-exec-5.c"
| 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 213 "./atomic/c11-atomic-exec-5.c"
| 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 213 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 213 "./atomic/c11-atomic-exec-5.c"
| 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 213 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 213 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 213 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 213 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 213 "./atomic/c11-atomic-exec-5.c"
); printf ("float_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_div_underflow; static void * test_thread_float_div_underflow (void *arg) { thread_ready = 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 216 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_div_underflow = (1.17549435082228750796873653722224568e-38F
# 216 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_float_div_underflow = (3.40282346638528859811704183484516925e+38F
# 216 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 216 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_div_underflow (void) { thread_stop = 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 216 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 216 "./atomic/c11-atomic-exec-5.c"
; var_float_div_underflow = (1.17549435082228750796873653722224568e-38F
# 216 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 216 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_div_underflow, 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 216 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 216 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_div_underflow /= 3.40282346638528859811704183484516925e+38F
# 216 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 216 "./atomic/c11-atomic-exec-5.c"
| 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 216 "./atomic/c11-atomic-exec-5.c"
| 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 216 "./atomic/c11-atomic-exec-5.c"
| 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 216 "./atomic/c11-atomic-exec-5.c"
| 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 216 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 216 "./atomic/c11-atomic-exec-5.c"
| 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 216 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_div_underflow = (3.40282346638528859811704183484516925e+38F
# 216 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_div_underflow = (1.17549435082228750796873653722224568e-38F
# 216 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 216 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 216 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 216 "./atomic/c11-atomic-exec-5.c"
); printf ("float_div_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_div_inexact; static void * test_thread_float_div_inexact (void *arg) { thread_ready = 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 219 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_div_inexact = (1); sched_yield (); var_float_div_inexact = (0); sched_yield (); } return 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 219 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_div_inexact (void) { thread_stop = 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 219 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 219 "./atomic/c11-atomic-exec-5.c"
; var_float_div_inexact = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 219 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_div_inexact, 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 219 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 219 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_div_inexact /= 3.0f); int rexc = fetestexcept ((
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 219 "./atomic/c11-atomic-exec-5.c"
| 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 219 "./atomic/c11-atomic-exec-5.c"
| 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 219 "./atomic/c11-atomic-exec-5.c"
| 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 219 "./atomic/c11-atomic-exec-5.c"
| 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 219 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 219 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_div_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_div_inexact = (1); } } thread_stop = 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 219 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 219 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 219 "./atomic/c11-atomic-exec-5.c"
); printf ("float_div_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic float var_float_div_inexact_int; static void * test_thread_float_div_inexact_int (void *arg) { thread_ready = 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 222 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_float_div_inexact_int = (1); sched_yield (); var_float_div_inexact_int = (0); sched_yield (); } return 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 222 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_float_div_inexact_int (void) { thread_stop = 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 222 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 222 "./atomic/c11-atomic-exec-5.c"
; var_float_div_inexact_int = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 222 "./atomic/c11-atomic-exec-5.c"
, test_thread_float_div_inexact_int, 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 222 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 222 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); float r = ( var_float_div_inexact_int /= 3); int rexc = fetestexcept ((
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 222 "./atomic/c11-atomic-exec-5.c"
| 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 222 "./atomic/c11-atomic-exec-5.c"
| 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 222 "./atomic/c11-atomic-exec-5.c"
| 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 222 "./atomic/c11-atomic-exec-5.c"
| 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 222 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 222 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_float_div_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_float_div_inexact_int = (1); } } thread_stop = 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 222 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 222 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 222 "./atomic/c11-atomic-exec-5.c"
); printf ("float_div_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic int var_int_div_float_inexact; static void * test_thread_int_div_float_inexact (void *arg) { thread_ready = 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 225 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_int_div_float_inexact = (4); sched_yield (); var_int_div_float_inexact = (0); sched_yield (); } return 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 225 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_int_div_float_inexact (void) { thread_stop = 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 225 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 225 "./atomic/c11-atomic-exec-5.c"
; var_int_div_float_inexact = (4); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 225 "./atomic/c11-atomic-exec-5.c"
, test_thread_int_div_float_inexact, 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 225 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 225 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); int r = ( var_int_div_float_inexact /= 3.0f); int rexc = fetestexcept ((
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 225 "./atomic/c11-atomic-exec-5.c"
| 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 225 "./atomic/c11-atomic-exec-5.c"
| 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 225 "./atomic/c11-atomic-exec-5.c"
| 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 225 "./atomic/c11-atomic-exec-5.c"
| 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 225 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 225 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_int_div_float_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_int_div_float_inexact = (4); } } thread_stop = 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 225 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 225 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 225 "./atomic/c11-atomic-exec-5.c"
); printf ("int_div_float_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic _Complex float var_complex_float_div_overflow; static void * test_thread_complex_float_div_overflow (void *arg) { thread_ready = 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 228 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 228 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_float_div_overflow = (0); sched_yield (); } return 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 228 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_float_div_overflow (void) { thread_stop = 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 228 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 228 "./atomic/c11-atomic-exec-5.c"
; var_complex_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 228 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 228 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_float_div_overflow, 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 228 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 228 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex float r = ( var_complex_float_div_overflow /= 1.17549435082228750796873653722224568e-38F
# 228 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 228 "./atomic/c11-atomic-exec-5.c"
| 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 228 "./atomic/c11-atomic-exec-5.c"
| 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 228 "./atomic/c11-atomic-exec-5.c"
| 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 228 "./atomic/c11-atomic-exec-5.c"
| 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 228 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 228 "./atomic/c11-atomic-exec-5.c"
| 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 228 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_float_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_float_div_overflow = (3.40282346638528859811704183484516925e+38F
# 228 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 228 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 228 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 228 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_float_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic double var_double_add_invalid; static void * test_thread_double_add_invalid (void *arg) { thread_ready = 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 232 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_add_invalid = (0); sched_yield (); var_double_add_invalid = (-__builtin_inf ()); sched_yield (); } return 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 232 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_add_invalid (void) { thread_stop = 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 232 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 232 "./atomic/c11-atomic-exec-5.c"
; var_double_add_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 232 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_add_invalid, 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 232 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 232 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_add_invalid += __builtin_inf ()); int rexc = fetestexcept ((
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 232 "./atomic/c11-atomic-exec-5.c"
| 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 232 "./atomic/c11-atomic-exec-5.c"
| 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 232 "./atomic/c11-atomic-exec-5.c"
| 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 232 "./atomic/c11-atomic-exec-5.c"
| 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 232 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_double_add_invalid = (-__builtin_inf ()); } else { if (rexc == ((0) | (
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 232 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_double_add_invalid = (0); } } thread_stop = 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 232 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 232 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 232 "./atomic/c11-atomic-exec-5.c"
); printf ("double_add_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_add_overflow; static void * test_thread_double_add_overflow (void *arg) { thread_ready = 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 235 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 235 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_add_overflow = (0); sched_yield (); } return 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 235 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_add_overflow (void) { thread_stop = 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 235 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 235 "./atomic/c11-atomic-exec-5.c"
; var_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 235 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 235 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_add_overflow, 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 235 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 235 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_add_overflow += ((double)1.79769313486231570814527423731704357e+308L)
# 235 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 235 "./atomic/c11-atomic-exec-5.c"
| 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 235 "./atomic/c11-atomic-exec-5.c"
| 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 235 "./atomic/c11-atomic-exec-5.c"
| 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 235 "./atomic/c11-atomic-exec-5.c"
| 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 235 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 235 "./atomic/c11-atomic-exec-5.c"
| 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 235 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 235 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 235 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 235 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 235 "./atomic/c11-atomic-exec-5.c"
); printf ("double_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_add_overflow_long_double; static void * test_thread_double_add_overflow_long_double (void *arg) { thread_ready = 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 238 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_add_overflow_long_double = (((double)1.79769313486231570814527423731704357e+308L)
# 238 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_add_overflow_long_double = (0); sched_yield (); } return 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 238 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_add_overflow_long_double (void) { thread_stop = 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 238 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 238 "./atomic/c11-atomic-exec-5.c"
; var_double_add_overflow_long_double = (((double)1.79769313486231570814527423731704357e+308L)
# 238 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 238 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_add_overflow_long_double, 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 238 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 238 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_add_overflow_long_double += (long double) ((double)1.79769313486231570814527423731704357e+308L)
# 238 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 238 "./atomic/c11-atomic-exec-5.c"
| 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 238 "./atomic/c11-atomic-exec-5.c"
| 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 238 "./atomic/c11-atomic-exec-5.c"
| 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 238 "./atomic/c11-atomic-exec-5.c"
| 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 238 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 238 "./atomic/c11-atomic-exec-5.c"
| 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 238 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_add_overflow_long_double = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_add_overflow_long_double = (((double)1.79769313486231570814527423731704357e+308L)
# 238 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 238 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 238 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 238 "./atomic/c11-atomic-exec-5.c"
); printf ("double_add_overflow_long_double" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic double var_double_add_inexact; static void * test_thread_double_add_inexact (void *arg) { thread_ready = 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 242 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_add_inexact = (1.0); sched_yield (); var_double_add_inexact = (0); sched_yield (); } return 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 242 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_add_inexact (void) { thread_stop = 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 242 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 242 "./atomic/c11-atomic-exec-5.c"
; var_double_add_inexact = (1.0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 242 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_add_inexact, 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 242 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 242 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_add_inexact += ((double)2.22044604925031308084726333618164062e-16L) 
# 242 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 242 "./atomic/c11-atomic-exec-5.c"
| 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 242 "./atomic/c11-atomic-exec-5.c"
| 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 242 "./atomic/c11-atomic-exec-5.c"
| 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 242 "./atomic/c11-atomic-exec-5.c"
| 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 242 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != ((double)2.22044604925031308084726333618164062e-16L) 
# 242 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 242 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_add_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_add_inexact = (1.0); } } thread_stop = 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 242 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 242 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 242 "./atomic/c11-atomic-exec-5.c"
); printf ("double_add_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_add_inexact_int; static void * test_thread_double_add_inexact_int (void *arg) { thread_ready = 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 245 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_add_inexact_int = (((double)2.22044604925031308084726333618164062e-16L) 
# 245 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_add_inexact_int = (-1); sched_yield (); } return 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 245 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_add_inexact_int (void) { thread_stop = 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 245 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 245 "./atomic/c11-atomic-exec-5.c"
; var_double_add_inexact_int = (((double)2.22044604925031308084726333618164062e-16L) 
# 245 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 245 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_add_inexact_int, 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 245 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 245 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_add_inexact_int += 1); int rexc = fetestexcept ((
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 245 "./atomic/c11-atomic-exec-5.c"
| 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 245 "./atomic/c11-atomic-exec-5.c"
| 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 245 "./atomic/c11-atomic-exec-5.c"
| 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 245 "./atomic/c11-atomic-exec-5.c"
| 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 245 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 245 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_add_inexact_int = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_add_inexact_int = (((double)2.22044604925031308084726333618164062e-16L) 
# 245 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 245 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 245 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 245 "./atomic/c11-atomic-exec-5.c"
); printf ("double_add_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_preinc_inexact; static void * test_thread_double_preinc_inexact (void *arg) { thread_ready = 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 248 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_preinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 248 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_preinc_inexact = (-1); sched_yield (); } return 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 248 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_preinc_inexact (void) { thread_stop = 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 248 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 248 "./atomic/c11-atomic-exec-5.c"
; var_double_preinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 248 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 248 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_preinc_inexact, 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 248 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 248 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = (++ var_double_preinc_inexact ); int rexc = fetestexcept ((
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 248 "./atomic/c11-atomic-exec-5.c"
| 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 248 "./atomic/c11-atomic-exec-5.c"
| 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 248 "./atomic/c11-atomic-exec-5.c"
| 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 248 "./atomic/c11-atomic-exec-5.c"
| 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 248 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 248 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_preinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_preinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 248 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 248 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 248 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 248 "./atomic/c11-atomic-exec-5.c"
); printf ("double_preinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_postinc_inexact; static void * test_thread_double_postinc_inexact (void *arg) { thread_ready = 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 251 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_postinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 251 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_postinc_inexact = (-1); sched_yield (); } return 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 251 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_postinc_inexact (void) { thread_stop = 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 251 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 251 "./atomic/c11-atomic-exec-5.c"
; var_double_postinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 251 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 251 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_postinc_inexact, 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 251 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 251 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_postinc_inexact ++); int rexc = fetestexcept ((
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 251 "./atomic/c11-atomic-exec-5.c"
| 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 251 "./atomic/c11-atomic-exec-5.c"
| 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 251 "./atomic/c11-atomic-exec-5.c"
| 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 251 "./atomic/c11-atomic-exec-5.c"
| 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 251 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -1)) { if (rexc == ((0) | (
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 251 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_postinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_postinc_inexact = (((double)2.22044604925031308084726333618164062e-16L) 
# 251 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 251 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 251 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 251 "./atomic/c11-atomic-exec-5.c"
); printf ("double_postinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long long var_long_long_add_double_inexact; static void * test_thread_long_long_add_double_inexact (void *arg) { thread_ready = 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 255 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_long_add_double_inexact = (1); sched_yield (); var_long_long_add_double_inexact = (-2 / ((double)2.22044604925031308084726333618164062e-16L)
# 255 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 255 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_long_add_double_inexact (void) { thread_stop = 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 255 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 255 "./atomic/c11-atomic-exec-5.c"
; var_long_long_add_double_inexact = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 255 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_long_add_double_inexact, 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 255 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 255 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long long r = ( var_long_long_add_double_inexact += 2 / ((double)2.22044604925031308084726333618164062e-16L)
# 255 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 255 "./atomic/c11-atomic-exec-5.c"
| 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 255 "./atomic/c11-atomic-exec-5.c"
| 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 255 "./atomic/c11-atomic-exec-5.c"
| 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 255 "./atomic/c11-atomic-exec-5.c"
| 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 255 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 255 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_long_add_double_inexact = (-2 / ((double)2.22044604925031308084726333618164062e-16L)
# 255 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_long_add_double_inexact = (1); } } thread_stop = 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 255 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 255 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 255 "./atomic/c11-atomic-exec-5.c"
); printf ("long_long_add_double_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex double var_complex_double_add_overflow; static void * test_thread_complex_double_add_overflow (void *arg) { thread_ready = 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 259 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 259 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_double_add_overflow = (0); sched_yield (); } return 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 259 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_double_add_overflow (void) { thread_stop = 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 259 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 259 "./atomic/c11-atomic-exec-5.c"
; var_complex_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 259 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 259 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_double_add_overflow, 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 259 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 259 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex double r = ( var_complex_double_add_overflow += ((double)1.79769313486231570814527423731704357e+308L)
# 259 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 259 "./atomic/c11-atomic-exec-5.c"
| 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 259 "./atomic/c11-atomic-exec-5.c"
| 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 259 "./atomic/c11-atomic-exec-5.c"
| 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 259 "./atomic/c11-atomic-exec-5.c"
| 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 259 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 259 "./atomic/c11-atomic-exec-5.c"
| 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 259 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_double_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_double_add_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 259 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 259 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 259 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 259 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_double_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_sub_invalid; static void * test_thread_double_sub_invalid (void *arg) { thread_ready = 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 262 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_sub_invalid = (0); sched_yield (); var_double_sub_invalid = (__builtin_inf ()); sched_yield (); } return 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 262 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_sub_invalid (void) { thread_stop = 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 262 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 262 "./atomic/c11-atomic-exec-5.c"
; var_double_sub_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 262 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_sub_invalid, 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 262 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 262 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_sub_invalid -= __builtin_inf ()); int rexc = fetestexcept ((
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 262 "./atomic/c11-atomic-exec-5.c"
| 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 262 "./atomic/c11-atomic-exec-5.c"
| 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 262 "./atomic/c11-atomic-exec-5.c"
| 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 262 "./atomic/c11-atomic-exec-5.c"
| 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 262 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_double_sub_invalid = (__builtin_inf ()); } else { if (rexc == ((0) | (
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 262 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_double_sub_invalid = (0); } } thread_stop = 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 262 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 262 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 262 "./atomic/c11-atomic-exec-5.c"
); printf ("double_sub_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_sub_overflow; static void * test_thread_double_sub_overflow (void *arg) { thread_ready = 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 265 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 265 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_sub_overflow = (0); sched_yield (); } return 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 265 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_sub_overflow (void) { thread_stop = 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 265 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 265 "./atomic/c11-atomic-exec-5.c"
; var_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 265 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 265 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_sub_overflow, 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 265 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 265 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_sub_overflow -= ((double)1.79769313486231570814527423731704357e+308L)
# 265 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 265 "./atomic/c11-atomic-exec-5.c"
| 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 265 "./atomic/c11-atomic-exec-5.c"
| 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 265 "./atomic/c11-atomic-exec-5.c"
| 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 265 "./atomic/c11-atomic-exec-5.c"
| 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 265 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 265 "./atomic/c11-atomic-exec-5.c"
| 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 265 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 265 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 265 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 265 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 265 "./atomic/c11-atomic-exec-5.c"
); printf ("double_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic double var_double_sub_inexact; static void * test_thread_double_sub_inexact (void *arg) { thread_ready = 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 269 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_sub_inexact = (-1.0); sched_yield (); var_double_sub_inexact = (0); sched_yield (); } return 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 269 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_sub_inexact (void) { thread_stop = 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 269 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 269 "./atomic/c11-atomic-exec-5.c"
; var_double_sub_inexact = (-1.0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 269 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_sub_inexact, 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 269 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 269 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_sub_inexact -= ((double)2.22044604925031308084726333618164062e-16L) 
# 269 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 269 "./atomic/c11-atomic-exec-5.c"
| 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 269 "./atomic/c11-atomic-exec-5.c"
| 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 269 "./atomic/c11-atomic-exec-5.c"
| 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 269 "./atomic/c11-atomic-exec-5.c"
| 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 269 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -((double)2.22044604925031308084726333618164062e-16L) 
# 269 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 269 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_sub_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_sub_inexact = (-1.0); } } thread_stop = 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 269 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 269 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 269 "./atomic/c11-atomic-exec-5.c"
); printf ("double_sub_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_sub_inexact_int; static void * test_thread_double_sub_inexact_int (void *arg) { thread_ready = 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 272 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_sub_inexact_int = (-((double)2.22044604925031308084726333618164062e-16L) 
# 272 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_sub_inexact_int = (1); sched_yield (); } return 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 272 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_sub_inexact_int (void) { thread_stop = 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 272 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 272 "./atomic/c11-atomic-exec-5.c"
; var_double_sub_inexact_int = (-((double)2.22044604925031308084726333618164062e-16L) 
# 272 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 272 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_sub_inexact_int, 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 272 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 272 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_sub_inexact_int -= 1); int rexc = fetestexcept ((
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 272 "./atomic/c11-atomic-exec-5.c"
| 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 272 "./atomic/c11-atomic-exec-5.c"
| 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 272 "./atomic/c11-atomic-exec-5.c"
| 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 272 "./atomic/c11-atomic-exec-5.c"
| 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 272 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 272 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_sub_inexact_int = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_sub_inexact_int = (-((double)2.22044604925031308084726333618164062e-16L) 
# 272 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 272 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 272 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 272 "./atomic/c11-atomic-exec-5.c"
); printf ("double_sub_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_predec_inexact; static void * test_thread_double_predec_inexact (void *arg) { thread_ready = 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 275 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_predec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 275 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_predec_inexact = (1); sched_yield (); } return 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 275 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_predec_inexact (void) { thread_stop = 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 275 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 275 "./atomic/c11-atomic-exec-5.c"
; var_double_predec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 275 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 275 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_predec_inexact, 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 275 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 275 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = (-- var_double_predec_inexact ); int rexc = fetestexcept ((
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 275 "./atomic/c11-atomic-exec-5.c"
| 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 275 "./atomic/c11-atomic-exec-5.c"
| 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 275 "./atomic/c11-atomic-exec-5.c"
| 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 275 "./atomic/c11-atomic-exec-5.c"
| 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 275 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 275 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_predec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_predec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 275 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 275 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 275 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 275 "./atomic/c11-atomic-exec-5.c"
); printf ("double_predec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_postdec_inexact; static void * test_thread_double_postdec_inexact (void *arg) { thread_ready = 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 278 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_postdec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 278 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_double_postdec_inexact = (1); sched_yield (); } return 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 278 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_postdec_inexact (void) { thread_stop = 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 278 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 278 "./atomic/c11-atomic-exec-5.c"
; var_double_postdec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 278 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 278 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_postdec_inexact, 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 278 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 278 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_postdec_inexact --); int rexc = fetestexcept ((
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 278 "./atomic/c11-atomic-exec-5.c"
| 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 278 "./atomic/c11-atomic-exec-5.c"
| 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 278 "./atomic/c11-atomic-exec-5.c"
| 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 278 "./atomic/c11-atomic-exec-5.c"
| 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 278 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 1)) { if (rexc == ((0) | (
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 278 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_postdec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_postdec_inexact = (-((double)2.22044604925031308084726333618164062e-16L) 
# 278 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 278 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 278 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 278 "./atomic/c11-atomic-exec-5.c"
); printf ("double_postdec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long long var_long_long_sub_double_inexact; static void * test_thread_long_long_sub_double_inexact (void *arg) { thread_ready = 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 282 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_long_sub_double_inexact = (-1); sched_yield (); var_long_long_sub_double_inexact = (2 / ((double)2.22044604925031308084726333618164062e-16L)
# 282 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 282 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_long_sub_double_inexact (void) { thread_stop = 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 282 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 282 "./atomic/c11-atomic-exec-5.c"
; var_long_long_sub_double_inexact = (-1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 282 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_long_sub_double_inexact, 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 282 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 282 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long long r = ( var_long_long_sub_double_inexact -= 2 / ((double)2.22044604925031308084726333618164062e-16L)
# 282 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 282 "./atomic/c11-atomic-exec-5.c"
| 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 282 "./atomic/c11-atomic-exec-5.c"
| 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 282 "./atomic/c11-atomic-exec-5.c"
| 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 282 "./atomic/c11-atomic-exec-5.c"
| 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 282 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 282 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_long_sub_double_inexact = (2 / ((double)2.22044604925031308084726333618164062e-16L)
# 282 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_long_sub_double_inexact = (-1); } } thread_stop = 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 282 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 282 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 282 "./atomic/c11-atomic-exec-5.c"
); printf ("long_long_sub_double_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex double var_complex_double_sub_overflow; static void * test_thread_complex_double_sub_overflow (void *arg) { thread_ready = 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 286 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 286 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_double_sub_overflow = (0); sched_yield (); } return 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 286 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_double_sub_overflow (void) { thread_stop = 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 286 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 286 "./atomic/c11-atomic-exec-5.c"
; var_complex_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 286 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 286 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_double_sub_overflow, 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 286 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 286 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex double r = ( var_complex_double_sub_overflow -= ((double)1.79769313486231570814527423731704357e+308L)
# 286 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 286 "./atomic/c11-atomic-exec-5.c"
| 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 286 "./atomic/c11-atomic-exec-5.c"
| 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 286 "./atomic/c11-atomic-exec-5.c"
| 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 286 "./atomic/c11-atomic-exec-5.c"
| 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 286 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 286 "./atomic/c11-atomic-exec-5.c"
| 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 286 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_double_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_double_sub_overflow = (-((double)1.79769313486231570814527423731704357e+308L)
# 286 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 286 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 286 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 286 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_double_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_invalid; static void * test_thread_double_mul_invalid (void *arg) { thread_ready = 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 289 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_invalid = (__builtin_inf ()); sched_yield (); var_double_mul_invalid = (0); sched_yield (); } return 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 289 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_invalid (void) { thread_stop = 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 289 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 289 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_invalid = (__builtin_inf ()); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 289 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_invalid, 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 289 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 289 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_invalid *= __builtin_inf ()); int rexc = fetestexcept ((
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 289 "./atomic/c11-atomic-exec-5.c"
| 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 289 "./atomic/c11-atomic-exec-5.c"
| 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 289 "./atomic/c11-atomic-exec-5.c"
| 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 289 "./atomic/c11-atomic-exec-5.c"
| 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 289 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_double_mul_invalid = (0); } else { if (rexc == ((0) | (
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 289 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_double_mul_invalid = (__builtin_inf ()); } } thread_stop = 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 289 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 289 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 289 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_overflow; static void * test_thread_double_mul_overflow (void *arg) { thread_ready = 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 292 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 292 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_mul_overflow = (0); sched_yield (); } return 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 292 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_overflow (void) { thread_stop = 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 292 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 292 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 292 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 292 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_overflow, 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 292 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 292 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_overflow *= ((double)1.79769313486231570814527423731704357e+308L)
# 292 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 292 "./atomic/c11-atomic-exec-5.c"
| 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 292 "./atomic/c11-atomic-exec-5.c"
| 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 292 "./atomic/c11-atomic-exec-5.c"
| 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 292 "./atomic/c11-atomic-exec-5.c"
| 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 292 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 292 "./atomic/c11-atomic-exec-5.c"
| 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 292 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 292 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 292 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 292 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 292 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_overflow_float; static void * test_thread_double_mul_overflow_float (void *arg) { thread_ready = 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 295 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_overflow_float = (((double)1.79769313486231570814527423731704357e+308L)
# 295 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_mul_overflow_float = (0); sched_yield (); } return 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 295 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_overflow_float (void) { thread_stop = 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 295 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 295 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_overflow_float = (((double)1.79769313486231570814527423731704357e+308L)
# 295 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 295 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_overflow_float, 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 295 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 295 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_overflow_float *= 3.40282346638528859811704183484516925e+38F
# 295 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 295 "./atomic/c11-atomic-exec-5.c"
| 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 295 "./atomic/c11-atomic-exec-5.c"
| 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 295 "./atomic/c11-atomic-exec-5.c"
| 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 295 "./atomic/c11-atomic-exec-5.c"
| 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 295 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 295 "./atomic/c11-atomic-exec-5.c"
| 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 295 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_mul_overflow_float = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_mul_overflow_float = (((double)1.79769313486231570814527423731704357e+308L)
# 295 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 295 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 295 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 295 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_overflow_float" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_underflow; static void * test_thread_double_mul_underflow (void *arg) { thread_ready = 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 298 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 298 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_mul_underflow = (1); sched_yield (); } return 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 298 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_underflow (void) { thread_stop = 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 298 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 298 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 298 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 298 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_underflow, 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 298 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 298 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_underflow *= ((double)2.22507385850720138309023271733240406e-308L)
# 298 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 298 "./atomic/c11-atomic-exec-5.c"
| 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 298 "./atomic/c11-atomic-exec-5.c"
| 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 298 "./atomic/c11-atomic-exec-5.c"
| 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 298 "./atomic/c11-atomic-exec-5.c"
| 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 298 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 298 "./atomic/c11-atomic-exec-5.c"
| 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 298 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_mul_underflow = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_mul_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 298 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 298 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 298 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 298 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_inexact; static void * test_thread_double_mul_inexact (void *arg) { thread_ready = 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 301 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_inexact = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 301 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_mul_inexact = (0); sched_yield (); } return 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 301 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_inexact (void) { thread_stop = 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 301 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 301 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_inexact = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 301 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 301 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_inexact, 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 301 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 301 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_inexact *= 1 + ((double)2.22044604925031308084726333618164062e-16L)
# 301 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 301 "./atomic/c11-atomic-exec-5.c"
| 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 301 "./atomic/c11-atomic-exec-5.c"
| 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 301 "./atomic/c11-atomic-exec-5.c"
| 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 301 "./atomic/c11-atomic-exec-5.c"
| 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 301 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 301 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_mul_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_mul_inexact = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 301 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 301 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 301 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 301 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_mul_inexact_int; static void * test_thread_double_mul_inexact_int (void *arg) { thread_ready = 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 304 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_mul_inexact_int = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 304 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_mul_inexact_int = (0); sched_yield (); } return 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 304 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_mul_inexact_int (void) { thread_stop = 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 304 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 304 "./atomic/c11-atomic-exec-5.c"
; var_double_mul_inexact_int = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 304 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 304 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_mul_inexact_int, 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 304 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 304 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_mul_inexact_int *= 3); int rexc = fetestexcept ((
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 304 "./atomic/c11-atomic-exec-5.c"
| 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 304 "./atomic/c11-atomic-exec-5.c"
| 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 304 "./atomic/c11-atomic-exec-5.c"
| 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 304 "./atomic/c11-atomic-exec-5.c"
| 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 304 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 304 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_mul_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_mul_inexact_int = (1 + ((double)2.22044604925031308084726333618164062e-16L)
# 304 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 304 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 304 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 304 "./atomic/c11-atomic-exec-5.c"
); printf ("double_mul_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long long var_long_long_mul_double_inexact; static void * test_thread_long_long_mul_double_inexact (void *arg) { thread_ready = 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 308 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_long_mul_double_inexact = (1 + 1 / ((double)2.22044604925031308084726333618164062e-16L)
# 308 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_long_mul_double_inexact = (0); sched_yield (); } return 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 308 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_long_mul_double_inexact (void) { thread_stop = 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 308 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 308 "./atomic/c11-atomic-exec-5.c"
; var_long_long_mul_double_inexact = (1 + 1 / ((double)2.22044604925031308084726333618164062e-16L)
# 308 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 308 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_long_mul_double_inexact, 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 308 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 308 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long long r = ( var_long_long_mul_double_inexact *= 3.0); int rexc = fetestexcept ((
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 308 "./atomic/c11-atomic-exec-5.c"
| 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 308 "./atomic/c11-atomic-exec-5.c"
| 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 308 "./atomic/c11-atomic-exec-5.c"
| 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 308 "./atomic/c11-atomic-exec-5.c"
| 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 308 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 308 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_long_mul_double_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_long_mul_double_inexact = (1 + 1 / ((double)2.22044604925031308084726333618164062e-16L)
# 308 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 308 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 308 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 308 "./atomic/c11-atomic-exec-5.c"
); printf ("long_long_mul_double_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex double var_complex_double_mul_overflow; static void * test_thread_complex_double_mul_overflow (void *arg) { thread_ready = 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 312 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 312 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_double_mul_overflow = (0); sched_yield (); } return 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 312 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_double_mul_overflow (void) { thread_stop = 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 312 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 312 "./atomic/c11-atomic-exec-5.c"
; var_complex_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 312 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 312 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_double_mul_overflow, 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 312 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 312 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex double r = ( var_complex_double_mul_overflow *= ((double)1.79769313486231570814527423731704357e+308L)
# 312 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 312 "./atomic/c11-atomic-exec-5.c"
| 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 312 "./atomic/c11-atomic-exec-5.c"
| 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 312 "./atomic/c11-atomic-exec-5.c"
| 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 312 "./atomic/c11-atomic-exec-5.c"
| 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 312 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 312 "./atomic/c11-atomic-exec-5.c"
| 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 312 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_double_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_double_mul_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 312 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 312 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 312 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 312 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_double_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_div_invalid_divbyzero; static void * test_thread_double_div_invalid_divbyzero (void *arg) { thread_ready = 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 315 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_div_invalid_divbyzero = (1); sched_yield (); var_double_div_invalid_divbyzero = (0); sched_yield (); } return 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 315 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_div_invalid_divbyzero (void) { thread_stop = 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 315 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 315 "./atomic/c11-atomic-exec-5.c"
; var_double_div_invalid_divbyzero = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 315 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_div_invalid_divbyzero, 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 315 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 315 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_div_invalid_divbyzero /= 0.0); int rexc = fetestexcept ((
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 315 "./atomic/c11-atomic-exec-5.c"
| 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 315 "./atomic/c11-atomic-exec-5.c"
| 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 315 "./atomic/c11-atomic-exec-5.c"
| 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 315 "./atomic/c11-atomic-exec-5.c"
| 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 315 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x04
# 315 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_div_invalid_divbyzero = (0); } else { if (rexc == ((0) | (
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 315 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_double_div_invalid_divbyzero = (1); } } thread_stop = 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 315 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 315 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 315 "./atomic/c11-atomic-exec-5.c"
); printf ("double_div_invalid_divbyzero" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_div_overflow; static void * test_thread_double_div_overflow (void *arg) { thread_ready = 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 318 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 318 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_div_overflow = (0); sched_yield (); } return 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 318 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_div_overflow (void) { thread_stop = 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 318 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 318 "./atomic/c11-atomic-exec-5.c"
; var_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 318 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 318 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_div_overflow, 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 318 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 318 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_div_overflow /= ((double)2.22507385850720138309023271733240406e-308L)
# 318 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 318 "./atomic/c11-atomic-exec-5.c"
| 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 318 "./atomic/c11-atomic-exec-5.c"
| 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 318 "./atomic/c11-atomic-exec-5.c"
| 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 318 "./atomic/c11-atomic-exec-5.c"
| 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 318 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 318 "./atomic/c11-atomic-exec-5.c"
| 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 318 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 318 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 318 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 318 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 318 "./atomic/c11-atomic-exec-5.c"
); printf ("double_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_div_underflow; static void * test_thread_double_div_underflow (void *arg) { thread_ready = 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 321 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_div_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_double_div_underflow = (((double)1.79769313486231570814527423731704357e+308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 321 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_div_underflow (void) { thread_stop = 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 321 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 321 "./atomic/c11-atomic-exec-5.c"
; var_double_div_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 321 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_div_underflow, 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 321 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 321 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_div_underflow /= ((double)1.79769313486231570814527423731704357e+308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 321 "./atomic/c11-atomic-exec-5.c"
| 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 321 "./atomic/c11-atomic-exec-5.c"
| 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 321 "./atomic/c11-atomic-exec-5.c"
| 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 321 "./atomic/c11-atomic-exec-5.c"
| 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 321 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 321 "./atomic/c11-atomic-exec-5.c"
| 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 321 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_div_underflow = (((double)1.79769313486231570814527423731704357e+308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_div_underflow = (((double)2.22507385850720138309023271733240406e-308L)
# 321 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 321 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 321 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 321 "./atomic/c11-atomic-exec-5.c"
); printf ("double_div_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_div_inexact; static void * test_thread_double_div_inexact (void *arg) { thread_ready = 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 324 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_div_inexact = (1); sched_yield (); var_double_div_inexact = (0); sched_yield (); } return 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 324 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_div_inexact (void) { thread_stop = 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 324 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 324 "./atomic/c11-atomic-exec-5.c"
; var_double_div_inexact = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 324 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_div_inexact, 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 324 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 324 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_div_inexact /= 3.0); int rexc = fetestexcept ((
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 324 "./atomic/c11-atomic-exec-5.c"
| 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 324 "./atomic/c11-atomic-exec-5.c"
| 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 324 "./atomic/c11-atomic-exec-5.c"
| 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 324 "./atomic/c11-atomic-exec-5.c"
| 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 324 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 324 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_div_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_div_inexact = (1); } } thread_stop = 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 324 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 324 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 324 "./atomic/c11-atomic-exec-5.c"
); printf ("double_div_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic double var_double_div_inexact_int; static void * test_thread_double_div_inexact_int (void *arg) { thread_ready = 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 327 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_double_div_inexact_int = (1); sched_yield (); var_double_div_inexact_int = (0); sched_yield (); } return 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 327 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_double_div_inexact_int (void) { thread_stop = 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 327 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 327 "./atomic/c11-atomic-exec-5.c"
; var_double_div_inexact_int = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 327 "./atomic/c11-atomic-exec-5.c"
, test_thread_double_div_inexact_int, 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 327 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 327 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); double r = ( var_double_div_inexact_int /= 3); int rexc = fetestexcept ((
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 327 "./atomic/c11-atomic-exec-5.c"
| 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 327 "./atomic/c11-atomic-exec-5.c"
| 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 327 "./atomic/c11-atomic-exec-5.c"
| 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 327 "./atomic/c11-atomic-exec-5.c"
| 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 327 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 327 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_double_div_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_double_div_inexact_int = (1); } } thread_stop = 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 327 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 327 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 327 "./atomic/c11-atomic-exec-5.c"
); printf ("double_div_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic int var_int_div_double_inexact; static void * test_thread_int_div_double_inexact (void *arg) { thread_ready = 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 330 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_int_div_double_inexact = (4); sched_yield (); var_int_div_double_inexact = (0); sched_yield (); } return 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 330 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_int_div_double_inexact (void) { thread_stop = 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 330 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 330 "./atomic/c11-atomic-exec-5.c"
; var_int_div_double_inexact = (4); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 330 "./atomic/c11-atomic-exec-5.c"
, test_thread_int_div_double_inexact, 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 330 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 330 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); int r = ( var_int_div_double_inexact /= 3.0); int rexc = fetestexcept ((
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 330 "./atomic/c11-atomic-exec-5.c"
| 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 330 "./atomic/c11-atomic-exec-5.c"
| 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 330 "./atomic/c11-atomic-exec-5.c"
| 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 330 "./atomic/c11-atomic-exec-5.c"
| 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 330 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 330 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_int_div_double_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_int_div_double_inexact = (4); } } thread_stop = 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 330 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 330 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 330 "./atomic/c11-atomic-exec-5.c"
); printf ("int_div_double_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic _Complex double var_complex_double_div_overflow; static void * test_thread_complex_double_div_overflow (void *arg) { thread_ready = 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 333 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 333 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_double_div_overflow = (0); sched_yield (); } return 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 333 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_double_div_overflow (void) { thread_stop = 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 333 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 333 "./atomic/c11-atomic-exec-5.c"
; var_complex_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 333 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 333 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_double_div_overflow, 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 333 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 333 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex double r = ( var_complex_double_div_overflow /= ((double)2.22507385850720138309023271733240406e-308L)
# 333 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 333 "./atomic/c11-atomic-exec-5.c"
| 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 333 "./atomic/c11-atomic-exec-5.c"
| 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 333 "./atomic/c11-atomic-exec-5.c"
| 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 333 "./atomic/c11-atomic-exec-5.c"
| 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 333 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 333 "./atomic/c11-atomic-exec-5.c"
| 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 333 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_double_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_double_div_overflow = (((double)1.79769313486231570814527423731704357e+308L)
# 333 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 333 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 333 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 333 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_double_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_add_invalid; static void * test_thread_long_double_add_invalid (void *arg) { thread_ready = 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 337 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_add_invalid = (0); sched_yield (); var_long_double_add_invalid = (-__builtin_infl ()); sched_yield (); } return 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 337 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_add_invalid (void) { thread_stop = 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 337 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 337 "./atomic/c11-atomic-exec-5.c"
; var_long_double_add_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 337 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_add_invalid, 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 337 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 337 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_add_invalid += __builtin_infl ()); int rexc = fetestexcept ((
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 337 "./atomic/c11-atomic-exec-5.c"
| 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 337 "./atomic/c11-atomic-exec-5.c"
| 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 337 "./atomic/c11-atomic-exec-5.c"
| 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 337 "./atomic/c11-atomic-exec-5.c"
| 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 337 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_long_double_add_invalid = (-__builtin_infl ()); } else { if (rexc == ((0) | (
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 337 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_long_double_add_invalid = (0); } } thread_stop = 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 337 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 337 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 337 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_add_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_add_overflow; static void * test_thread_long_double_add_overflow (void *arg) { thread_ready = 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 341 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 341 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_add_overflow = (0); sched_yield (); } return 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 341 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_add_overflow (void) { thread_stop = 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 341 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 341 "./atomic/c11-atomic-exec-5.c"
; var_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 341 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 341 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_add_overflow, 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 341 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 341 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_add_overflow += 1.18973149535723176502126385303097021e+4932L
# 341 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 341 "./atomic/c11-atomic-exec-5.c"
| 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 341 "./atomic/c11-atomic-exec-5.c"
| 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 341 "./atomic/c11-atomic-exec-5.c"
| 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 341 "./atomic/c11-atomic-exec-5.c"
| 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 341 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 341 "./atomic/c11-atomic-exec-5.c"
| 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 341 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 341 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 341 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 341 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 341 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_add_inexact; static void * test_thread_long_double_add_inexact (void *arg) { thread_ready = 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 345 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_add_inexact = (1.0L); sched_yield (); var_long_double_add_inexact = (0); sched_yield (); } return 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 345 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_add_inexact (void) { thread_stop = 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 345 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 345 "./atomic/c11-atomic-exec-5.c"
; var_long_double_add_inexact = (1.0L); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 345 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_add_inexact, 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 345 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 345 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_add_inexact += 1.08420217248550443400745280086994171e-19L 
# 345 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 345 "./atomic/c11-atomic-exec-5.c"
| 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 345 "./atomic/c11-atomic-exec-5.c"
| 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 345 "./atomic/c11-atomic-exec-5.c"
| 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 345 "./atomic/c11-atomic-exec-5.c"
| 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 345 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 1.08420217248550443400745280086994171e-19L 
# 345 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 345 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_add_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_add_inexact = (1.0L); } } thread_stop = 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 345 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 345 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 345 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_add_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_add_inexact_int; static void * test_thread_long_double_add_inexact_int (void *arg) { thread_ready = 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 348 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_add_inexact_int = (1.08420217248550443400745280086994171e-19L 
# 348 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_add_inexact_int = (-1); sched_yield (); } return 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 348 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_add_inexact_int (void) { thread_stop = 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 348 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 348 "./atomic/c11-atomic-exec-5.c"
; var_long_double_add_inexact_int = (1.08420217248550443400745280086994171e-19L 
# 348 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 348 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_add_inexact_int, 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 348 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 348 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_add_inexact_int += 1); int rexc = fetestexcept ((
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 348 "./atomic/c11-atomic-exec-5.c"
| 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 348 "./atomic/c11-atomic-exec-5.c"
| 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 348 "./atomic/c11-atomic-exec-5.c"
| 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 348 "./atomic/c11-atomic-exec-5.c"
| 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 348 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 348 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_add_inexact_int = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_add_inexact_int = (1.08420217248550443400745280086994171e-19L 
# 348 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 348 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 348 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 348 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_add_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_preinc_inexact; static void * test_thread_long_double_preinc_inexact (void *arg) { thread_ready = 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 351 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_preinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 351 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_preinc_inexact = (-1); sched_yield (); } return 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 351 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_preinc_inexact (void) { thread_stop = 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 351 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 351 "./atomic/c11-atomic-exec-5.c"
; var_long_double_preinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 351 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 351 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_preinc_inexact, 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 351 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 351 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = (++ var_long_double_preinc_inexact ); int rexc = fetestexcept ((
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 351 "./atomic/c11-atomic-exec-5.c"
| 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 351 "./atomic/c11-atomic-exec-5.c"
| 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 351 "./atomic/c11-atomic-exec-5.c"
| 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 351 "./atomic/c11-atomic-exec-5.c"
| 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 351 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 351 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_preinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_preinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 351 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 351 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 351 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 351 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_preinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_postinc_inexact; static void * test_thread_long_double_postinc_inexact (void *arg) { thread_ready = 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 354 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_postinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 354 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_postinc_inexact = (-1); sched_yield (); } return 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 354 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_postinc_inexact (void) { thread_stop = 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 354 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 354 "./atomic/c11-atomic-exec-5.c"
; var_long_double_postinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 354 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 354 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_postinc_inexact, 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 354 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 354 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_postinc_inexact ++); int rexc = fetestexcept ((
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 354 "./atomic/c11-atomic-exec-5.c"
| 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 354 "./atomic/c11-atomic-exec-5.c"
| 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 354 "./atomic/c11-atomic-exec-5.c"
| 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 354 "./atomic/c11-atomic-exec-5.c"
| 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 354 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -1)) { if (rexc == ((0) | (
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 354 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_postinc_inexact = (-1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_postinc_inexact = (1.08420217248550443400745280086994171e-19L 
# 354 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 354 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 354 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 354 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_postinc_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic _Complex long double var_complex_long_double_add_overflow; static void * test_thread_complex_long_double_add_overflow (void *arg) { thread_ready = 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 357 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 357 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_long_double_add_overflow = (0); sched_yield (); } return 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 357 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_long_double_add_overflow (void) { thread_stop = 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 357 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 357 "./atomic/c11-atomic-exec-5.c"
; var_complex_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 357 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 357 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_long_double_add_overflow, 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 357 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 357 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex long double r = ( var_complex_long_double_add_overflow += 1.18973149535723176502126385303097021e+4932L
# 357 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 357 "./atomic/c11-atomic-exec-5.c"
| 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 357 "./atomic/c11-atomic-exec-5.c"
| 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 357 "./atomic/c11-atomic-exec-5.c"
| 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 357 "./atomic/c11-atomic-exec-5.c"
| 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 357 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 357 "./atomic/c11-atomic-exec-5.c"
| 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 357 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_long_double_add_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_long_double_add_overflow = (1.18973149535723176502126385303097021e+4932L
# 357 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 357 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 357 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 357 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_long_double_add_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_sub_invalid; static void * test_thread_long_double_sub_invalid (void *arg) { thread_ready = 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 361 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_sub_invalid = (0); sched_yield (); var_long_double_sub_invalid = (__builtin_infl ()); sched_yield (); } return 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 361 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_sub_invalid (void) { thread_stop = 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 361 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 361 "./atomic/c11-atomic-exec-5.c"
; var_long_double_sub_invalid = (0); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 361 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_sub_invalid, 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 361 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 361 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_sub_invalid -= __builtin_infl ()); int rexc = fetestexcept ((
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 361 "./atomic/c11-atomic-exec-5.c"
| 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 361 "./atomic/c11-atomic-exec-5.c"
| 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 361 "./atomic/c11-atomic-exec-5.c"
| 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 361 "./atomic/c11-atomic-exec-5.c"
| 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 361 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_long_double_sub_invalid = (__builtin_infl ()); } else { if (rexc == ((0) | (
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 361 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_long_double_sub_invalid = (0); } } thread_stop = 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 361 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 361 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 361 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_sub_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_sub_overflow; static void * test_thread_long_double_sub_overflow (void *arg) { thread_ready = 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 365 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 365 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_sub_overflow = (0); sched_yield (); } return 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 365 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_sub_overflow (void) { thread_stop = 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 365 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 365 "./atomic/c11-atomic-exec-5.c"
; var_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 365 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 365 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_sub_overflow, 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 365 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 365 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_sub_overflow -= 1.18973149535723176502126385303097021e+4932L
# 365 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 365 "./atomic/c11-atomic-exec-5.c"
| 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 365 "./atomic/c11-atomic-exec-5.c"
| 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 365 "./atomic/c11-atomic-exec-5.c"
| 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 365 "./atomic/c11-atomic-exec-5.c"
| 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 365 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 365 "./atomic/c11-atomic-exec-5.c"
| 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 365 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 365 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 365 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 365 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 365 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_sub_inexact; static void * test_thread_long_double_sub_inexact (void *arg) { thread_ready = 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 369 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_sub_inexact = (-1.0L); sched_yield (); var_long_double_sub_inexact = (0); sched_yield (); } return 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 369 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_sub_inexact (void) { thread_stop = 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 369 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 369 "./atomic/c11-atomic-exec-5.c"
; var_long_double_sub_inexact = (-1.0L); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 369 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_sub_inexact, 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 369 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 369 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_sub_inexact -= 1.08420217248550443400745280086994171e-19L 
# 369 "./atomic/c11-atomic-exec-5.c"
/ 2); int rexc = fetestexcept ((
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 369 "./atomic/c11-atomic-exec-5.c"
| 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 369 "./atomic/c11-atomic-exec-5.c"
| 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 369 "./atomic/c11-atomic-exec-5.c"
| 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 369 "./atomic/c11-atomic-exec-5.c"
| 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 369 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != -1.08420217248550443400745280086994171e-19L 
# 369 "./atomic/c11-atomic-exec-5.c"
/ 2)) { if (rexc == ((0) | (
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 369 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_sub_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_sub_inexact = (-1.0L); } } thread_stop = 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 369 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 369 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 369 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_sub_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_sub_inexact_int; static void * test_thread_long_double_sub_inexact_int (void *arg) { thread_ready = 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 372 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_sub_inexact_int = (-1.08420217248550443400745280086994171e-19L 
# 372 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_sub_inexact_int = (1); sched_yield (); } return 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 372 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_sub_inexact_int (void) { thread_stop = 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 372 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 372 "./atomic/c11-atomic-exec-5.c"
; var_long_double_sub_inexact_int = (-1.08420217248550443400745280086994171e-19L 
# 372 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 372 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_sub_inexact_int, 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 372 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 372 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_sub_inexact_int -= 1); int rexc = fetestexcept ((
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 372 "./atomic/c11-atomic-exec-5.c"
| 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 372 "./atomic/c11-atomic-exec-5.c"
| 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 372 "./atomic/c11-atomic-exec-5.c"
| 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 372 "./atomic/c11-atomic-exec-5.c"
| 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 372 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 372 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_sub_inexact_int = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_sub_inexact_int = (-1.08420217248550443400745280086994171e-19L 
# 372 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 372 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 372 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 372 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_sub_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_predec_inexact; static void * test_thread_long_double_predec_inexact (void *arg) { thread_ready = 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 375 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_predec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 375 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_predec_inexact = (1); sched_yield (); } return 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 375 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_predec_inexact (void) { thread_stop = 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 375 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 375 "./atomic/c11-atomic-exec-5.c"
; var_long_double_predec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 375 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 375 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_predec_inexact, 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 375 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 375 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = (-- var_long_double_predec_inexact ); int rexc = fetestexcept ((
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 375 "./atomic/c11-atomic-exec-5.c"
| 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 375 "./atomic/c11-atomic-exec-5.c"
| 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 375 "./atomic/c11-atomic-exec-5.c"
| 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 375 "./atomic/c11-atomic-exec-5.c"
| 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 375 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 375 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_predec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_predec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 375 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 375 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 375 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 375 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_predec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_postdec_inexact; static void * test_thread_long_double_postdec_inexact (void *arg) { thread_ready = 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 378 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_postdec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 378 "./atomic/c11-atomic-exec-5.c"
/ 2); sched_yield (); var_long_double_postdec_inexact = (1); sched_yield (); } return 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 378 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_postdec_inexact (void) { thread_stop = 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 378 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 378 "./atomic/c11-atomic-exec-5.c"
; var_long_double_postdec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 378 "./atomic/c11-atomic-exec-5.c"
/ 2); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 378 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_postdec_inexact, 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 378 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 378 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_postdec_inexact --); int rexc = fetestexcept ((
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 378 "./atomic/c11-atomic-exec-5.c"
| 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 378 "./atomic/c11-atomic-exec-5.c"
| 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 378 "./atomic/c11-atomic-exec-5.c"
| 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 378 "./atomic/c11-atomic-exec-5.c"
| 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 378 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 1)) { if (rexc == ((0) | (
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 378 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_postdec_inexact = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_postdec_inexact = (-1.08420217248550443400745280086994171e-19L 
# 378 "./atomic/c11-atomic-exec-5.c"
/ 2); } } thread_stop = 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 378 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 378 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 378 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_postdec_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic _Complex long double var_complex_long_double_sub_overflow; static void * test_thread_complex_long_double_sub_overflow (void *arg) { thread_ready = 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 381 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 381 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_long_double_sub_overflow = (0); sched_yield (); } return 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 381 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_long_double_sub_overflow (void) { thread_stop = 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 381 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 381 "./atomic/c11-atomic-exec-5.c"
; var_complex_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 381 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 381 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_long_double_sub_overflow, 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 381 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 381 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex long double r = ( var_complex_long_double_sub_overflow -= 1.18973149535723176502126385303097021e+4932L
# 381 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 381 "./atomic/c11-atomic-exec-5.c"
| 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 381 "./atomic/c11-atomic-exec-5.c"
| 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 381 "./atomic/c11-atomic-exec-5.c"
| 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 381 "./atomic/c11-atomic-exec-5.c"
| 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 381 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 381 "./atomic/c11-atomic-exec-5.c"
| 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 381 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_long_double_sub_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_long_double_sub_overflow = (-1.18973149535723176502126385303097021e+4932L
# 381 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 381 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 381 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 381 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_long_double_sub_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_mul_invalid; static void * test_thread_long_double_mul_invalid (void *arg) { thread_ready = 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 385 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_invalid = (__builtin_infl ()); sched_yield (); var_long_double_mul_invalid = (0); sched_yield (); } return 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 385 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_invalid (void) { thread_stop = 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 385 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 385 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_invalid = (__builtin_infl ()); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 385 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_invalid, 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 385 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 385 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_invalid *= __builtin_infl ()); int rexc = fetestexcept ((
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 385 "./atomic/c11-atomic-exec-5.c"
| 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 385 "./atomic/c11-atomic-exec-5.c"
| 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 385 "./atomic/c11-atomic-exec-5.c"
| 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 385 "./atomic/c11-atomic-exec-5.c"
| 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 385 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (0))) num_1_pass++; else num_1_fail++; var_long_double_mul_invalid = (0); } else { if (rexc == ((0) | (
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 385 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_long_double_mul_invalid = (__builtin_infl ()); } } thread_stop = 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 385 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 385 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 385 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_invalid" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_mul_overflow; static void * test_thread_long_double_mul_overflow (void *arg) { thread_ready = 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 388 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 388 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_overflow = (0); sched_yield (); } return 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 388 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_overflow (void) { thread_stop = 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 388 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 388 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 388 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 388 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_overflow, 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 388 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 388 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_overflow *= 1.18973149535723176502126385303097021e+4932L
# 388 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 388 "./atomic/c11-atomic-exec-5.c"
| 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 388 "./atomic/c11-atomic-exec-5.c"
| 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 388 "./atomic/c11-atomic-exec-5.c"
| 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 388 "./atomic/c11-atomic-exec-5.c"
| 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 388 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 388 "./atomic/c11-atomic-exec-5.c"
| 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 388 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 388 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 388 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 388 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 388 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_mul_overflow_float; static void * test_thread_long_double_mul_overflow_float (void *arg) { thread_ready = 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 391 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_overflow_float = (1.18973149535723176502126385303097021e+4932L
# 391 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_overflow_float = (0); sched_yield (); } return 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 391 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_overflow_float (void) { thread_stop = 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 391 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 391 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_overflow_float = (1.18973149535723176502126385303097021e+4932L
# 391 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 391 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_overflow_float, 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 391 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 391 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_overflow_float *= 3.40282346638528859811704183484516925e+38F
# 391 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 391 "./atomic/c11-atomic-exec-5.c"
| 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 391 "./atomic/c11-atomic-exec-5.c"
| 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 391 "./atomic/c11-atomic-exec-5.c"
| 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 391 "./atomic/c11-atomic-exec-5.c"
| 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 391 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 391 "./atomic/c11-atomic-exec-5.c"
| 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 391 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_overflow_float = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_overflow_float = (1.18973149535723176502126385303097021e+4932L
# 391 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 391 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 391 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 391 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_overflow_float" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_mul_overflow_double; static void * test_thread_long_double_mul_overflow_double (void *arg) { thread_ready = 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 394 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_overflow_double = (1.18973149535723176502126385303097021e+4932L
# 394 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_overflow_double = (0); sched_yield (); } return 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 394 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_overflow_double (void) { thread_stop = 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 394 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 394 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_overflow_double = (1.18973149535723176502126385303097021e+4932L
# 394 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 394 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_overflow_double, 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 394 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 394 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_overflow_double *= ((double)1.79769313486231570814527423731704357e+308L)
# 394 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 394 "./atomic/c11-atomic-exec-5.c"
| 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 394 "./atomic/c11-atomic-exec-5.c"
| 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 394 "./atomic/c11-atomic-exec-5.c"
| 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 394 "./atomic/c11-atomic-exec-5.c"
| 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 394 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 394 "./atomic/c11-atomic-exec-5.c"
| 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 394 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_overflow_double = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_overflow_double = (1.18973149535723176502126385303097021e+4932L
# 394 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 394 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 394 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 394 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_overflow_double" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_mul_underflow; static void * test_thread_long_double_mul_underflow (void *arg) { thread_ready = 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 397 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_underflow = (3.36210314311209350626267781732175260e-4932L
# 397 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_underflow = (1); sched_yield (); } return 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 397 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_underflow (void) { thread_stop = 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 397 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 397 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_underflow = (3.36210314311209350626267781732175260e-4932L
# 397 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 397 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_underflow, 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 397 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 397 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_underflow *= 3.36210314311209350626267781732175260e-4932L
# 397 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 397 "./atomic/c11-atomic-exec-5.c"
| 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 397 "./atomic/c11-atomic-exec-5.c"
| 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 397 "./atomic/c11-atomic-exec-5.c"
| 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 397 "./atomic/c11-atomic-exec-5.c"
| 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 397 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 397 "./atomic/c11-atomic-exec-5.c"
| 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 397 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_underflow = (1); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_underflow = (3.36210314311209350626267781732175260e-4932L
# 397 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 397 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 397 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 397 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic long double var_long_double_mul_inexact; static void * test_thread_long_double_mul_inexact (void *arg) { thread_ready = 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 401 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_inexact = (1 + 1.08420217248550443400745280086994171e-19L
# 401 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_inexact = (0); sched_yield (); } return 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 401 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_inexact (void) { thread_stop = 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 401 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 401 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_inexact = (1 + 1.08420217248550443400745280086994171e-19L
# 401 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 401 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_inexact, 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 401 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 401 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_inexact *= 1 + 1.08420217248550443400745280086994171e-19L
# 401 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 401 "./atomic/c11-atomic-exec-5.c"
| 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 401 "./atomic/c11-atomic-exec-5.c"
| 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 401 "./atomic/c11-atomic-exec-5.c"
| 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 401 "./atomic/c11-atomic-exec-5.c"
| 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 401 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 401 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_inexact = (1 + 1.08420217248550443400745280086994171e-19L
# 401 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 401 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 401 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 401 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_mul_inexact_int; static void * test_thread_long_double_mul_inexact_int (void *arg) { thread_ready = 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 404 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_mul_inexact_int = (1 + 1.08420217248550443400745280086994171e-19L
# 404 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_mul_inexact_int = (0); sched_yield (); } return 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 404 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_mul_inexact_int (void) { thread_stop = 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 404 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 404 "./atomic/c11-atomic-exec-5.c"
; var_long_double_mul_inexact_int = (1 + 1.08420217248550443400745280086994171e-19L
# 404 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 404 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_mul_inexact_int, 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 404 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 404 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_mul_inexact_int *= 3); int rexc = fetestexcept ((
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 404 "./atomic/c11-atomic-exec-5.c"
| 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 404 "./atomic/c11-atomic-exec-5.c"
| 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 404 "./atomic/c11-atomic-exec-5.c"
| 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 404 "./atomic/c11-atomic-exec-5.c"
| 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 404 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 404 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_mul_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_mul_inexact_int = (1 + 1.08420217248550443400745280086994171e-19L
# 404 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 404 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 404 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 404 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_mul_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



static volatile _Atomic _Complex long double var_complex_long_double_mul_overflow; static void * test_thread_complex_long_double_mul_overflow (void *arg) { thread_ready = 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 408 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 408 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_long_double_mul_overflow = (0); sched_yield (); } return 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 408 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_long_double_mul_overflow (void) { thread_stop = 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 408 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 408 "./atomic/c11-atomic-exec-5.c"
; var_complex_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 408 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 408 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_long_double_mul_overflow, 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 408 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 408 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex long double r = ( var_complex_long_double_mul_overflow *= 1.18973149535723176502126385303097021e+4932L
# 408 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 408 "./atomic/c11-atomic-exec-5.c"
| 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 408 "./atomic/c11-atomic-exec-5.c"
| 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 408 "./atomic/c11-atomic-exec-5.c"
| 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 408 "./atomic/c11-atomic-exec-5.c"
| 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 408 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 408 "./atomic/c11-atomic-exec-5.c"
| 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 408 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_long_double_mul_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_long_double_mul_overflow = (1.18973149535723176502126385303097021e+4932L
# 408 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 408 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 408 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 408 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_long_double_mul_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_div_invalid_divbyzero; static void * test_thread_long_double_div_invalid_divbyzero (void *arg) { thread_ready = 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 411 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_div_invalid_divbyzero = (1); sched_yield (); var_long_double_div_invalid_divbyzero = (0); sched_yield (); } return 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 411 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_div_invalid_divbyzero (void) { thread_stop = 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 411 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 411 "./atomic/c11-atomic-exec-5.c"
; var_long_double_div_invalid_divbyzero = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 411 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_div_invalid_divbyzero, 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 411 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 411 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_div_invalid_divbyzero /= 0.0L); int rexc = fetestexcept ((
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 411 "./atomic/c11-atomic-exec-5.c"
| 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 411 "./atomic/c11-atomic-exec-5.c"
| 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 411 "./atomic/c11-atomic-exec-5.c"
| 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 411 "./atomic/c11-atomic-exec-5.c"
| 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 411 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x04
# 411 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_div_invalid_divbyzero = (0); } else { if (rexc == ((0) | (
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
0x01
# 411 "./atomic/c11-atomic-exec-5.c"
))) num_2_pass++; else num_2_fail++; var_long_double_div_invalid_divbyzero = (1); } } thread_stop = 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 411 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 411 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 411 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_div_invalid_divbyzero" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_div_overflow; static void * test_thread_long_double_div_overflow (void *arg) { thread_ready = 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 414 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 414 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_div_overflow = (0); sched_yield (); } return 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 414 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_div_overflow (void) { thread_stop = 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 414 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 414 "./atomic/c11-atomic-exec-5.c"
; var_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 414 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 414 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_div_overflow, 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 414 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 414 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_div_overflow /= 3.36210314311209350626267781732175260e-4932L
# 414 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 414 "./atomic/c11-atomic-exec-5.c"
| 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 414 "./atomic/c11-atomic-exec-5.c"
| 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 414 "./atomic/c11-atomic-exec-5.c"
| 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 414 "./atomic/c11-atomic-exec-5.c"
| 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 414 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (__builtin_isinf (r)) { if (rexc == ((0) | (
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 414 "./atomic/c11-atomic-exec-5.c"
| 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 414 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 414 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 414 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 414 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 414 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_div_underflow; static void * test_thread_long_double_div_underflow (void *arg) { thread_ready = 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 417 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_div_underflow = (3.36210314311209350626267781732175260e-4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_long_double_div_underflow = (1.18973149535723176502126385303097021e+4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); } return 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 417 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_div_underflow (void) { thread_stop = 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 417 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 417 "./atomic/c11-atomic-exec-5.c"
; var_long_double_div_underflow = (3.36210314311209350626267781732175260e-4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 417 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_div_underflow, 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 417 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 417 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_div_underflow /= 1.18973149535723176502126385303097021e+4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 417 "./atomic/c11-atomic-exec-5.c"
| 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 417 "./atomic/c11-atomic-exec-5.c"
| 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 417 "./atomic/c11-atomic-exec-5.c"
| 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 417 "./atomic/c11-atomic-exec-5.c"
| 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 417 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) == 0)) { if (rexc == ((0) | (
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x10 
# 417 "./atomic/c11-atomic-exec-5.c"
| 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 417 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_div_underflow = (1.18973149535723176502126385303097021e+4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_div_underflow = (3.36210314311209350626267781732175260e-4932L
# 417 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 417 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 417 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 417 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_div_underflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_div_inexact; static void * test_thread_long_double_div_inexact (void *arg) { thread_ready = 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 420 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_div_inexact = (1); sched_yield (); var_long_double_div_inexact = (0); sched_yield (); } return 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 420 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_div_inexact (void) { thread_stop = 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 420 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 420 "./atomic/c11-atomic-exec-5.c"
; var_long_double_div_inexact = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 420 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_div_inexact, 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 420 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 420 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_div_inexact /= 3.0L); int rexc = fetestexcept ((
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 420 "./atomic/c11-atomic-exec-5.c"
| 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 420 "./atomic/c11-atomic-exec-5.c"
| 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 420 "./atomic/c11-atomic-exec-5.c"
| 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 420 "./atomic/c11-atomic-exec-5.c"
| 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 420 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 420 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_div_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_div_inexact = (1); } } thread_stop = 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 420 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 420 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 420 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_div_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic long double var_long_double_div_inexact_int; static void * test_thread_long_double_div_inexact_int (void *arg) { thread_ready = 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 423 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_long_double_div_inexact_int = (1); sched_yield (); var_long_double_div_inexact_int = (0); sched_yield (); } return 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 423 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_long_double_div_inexact_int (void) { thread_stop = 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 423 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 423 "./atomic/c11-atomic-exec-5.c"
; var_long_double_div_inexact_int = (1); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 423 "./atomic/c11-atomic-exec-5.c"
, test_thread_long_double_div_inexact_int, 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 423 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 423 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); long double r = ( var_long_double_div_inexact_int /= 3); int rexc = fetestexcept ((
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 423 "./atomic/c11-atomic-exec-5.c"
| 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 423 "./atomic/c11-atomic-exec-5.c"
| 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 423 "./atomic/c11-atomic-exec-5.c"
| 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 423 "./atomic/c11-atomic-exec-5.c"
| 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 423 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 423 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_long_double_div_inexact_int = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_long_double_div_inexact_int = (1); } } thread_stop = 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 423 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 423 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 423 "./atomic/c11-atomic-exec-5.c"
); printf ("long_double_div_inexact_int" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic int var_int_div_long_double_inexact; static void * test_thread_int_div_long_double_inexact (void *arg) { thread_ready = 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 426 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_int_div_long_double_inexact = (4); sched_yield (); var_int_div_long_double_inexact = (0); sched_yield (); } return 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 426 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_int_div_long_double_inexact (void) { thread_stop = 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 426 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 426 "./atomic/c11-atomic-exec-5.c"
; var_int_div_long_double_inexact = (4); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 426 "./atomic/c11-atomic-exec-5.c"
, test_thread_int_div_long_double_inexact, 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 426 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 426 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); int r = ( var_int_div_long_double_inexact /= 3.0L); int rexc = fetestexcept ((
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 426 "./atomic/c11-atomic-exec-5.c"
| 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 426 "./atomic/c11-atomic-exec-5.c"
| 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 426 "./atomic/c11-atomic-exec-5.c"
| 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 426 "./atomic/c11-atomic-exec-5.c"
| 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 426 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if (((r) != 0)) { if (rexc == ((0) | (
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 426 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_int_div_long_double_inexact = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_int_div_long_double_inexact = (4); } } thread_stop = 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 426 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 426 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 426 "./atomic/c11-atomic-exec-5.c"
); printf ("int_div_long_double_inexact" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }


static volatile _Atomic _Complex long double var_complex_long_double_div_overflow; static void * test_thread_complex_long_double_div_overflow (void *arg) { thread_ready = 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 429 "./atomic/c11-atomic-exec-5.c"
; while (!thread_stop) { sched_yield (); var_complex_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 429 "./atomic/c11-atomic-exec-5.c"
); sched_yield (); var_complex_long_double_div_overflow = (0); sched_yield (); } return 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 429 "./atomic/c11-atomic-exec-5.c"
; } static int test_main_complex_long_double_div_overflow (void) { thread_stop = 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 429 "./atomic/c11-atomic-exec-5.c"
; thread_ready = 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0
# 429 "./atomic/c11-atomic-exec-5.c"
; var_complex_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 429 "./atomic/c11-atomic-exec-5.c"
); pthread_t thread_id; int pret = pthread_create (&thread_id, 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 429 "./atomic/c11-atomic-exec-5.c"
, test_thread_complex_long_double_div_overflow, 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 429 "./atomic/c11-atomic-exec-5.c"
); if (pret != 0) { printf ("pthread_create failed: %d\n", pret); return 1; } int num_1_pass = 0, num_1_fail = 0, num_2_pass = 0, num_2_fail = 0; while (!thread_ready) sched_yield (); for (int i = 0; i < 10000; i++) { feclearexcept (
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
(0x20 | 0x04 | 0x10 | 0x08 | 0x01)
# 429 "./atomic/c11-atomic-exec-5.c"
); feraiseexcept (0); _Complex long double r = ( var_complex_long_double_div_overflow /= 3.36210314311209350626267781732175260e-4932L
# 429 "./atomic/c11-atomic-exec-5.c"
); int rexc = fetestexcept ((
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x04 
# 429 "./atomic/c11-atomic-exec-5.c"
| 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x20 
# 429 "./atomic/c11-atomic-exec-5.c"
| 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x01 
# 429 "./atomic/c11-atomic-exec-5.c"
| 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 429 "./atomic/c11-atomic-exec-5.c"
| 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x10
# 429 "./atomic/c11-atomic-exec-5.c"
)); sched_yield (); if ((__builtin_isinf (__real__ (r)))) { if (rexc == ((0) | (
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x08 
# 429 "./atomic/c11-atomic-exec-5.c"
| 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
0x20
# 429 "./atomic/c11-atomic-exec-5.c"
))) num_1_pass++; else num_1_fail++; var_complex_long_double_div_overflow = (0); } else { if (rexc == ((0) | (0))) num_2_pass++; else num_2_fail++; var_complex_long_double_div_overflow = (1.18973149535723176502126385303097021e+4932L
# 429 "./atomic/c11-atomic-exec-5.c"
); } } thread_stop = 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
1
# 429 "./atomic/c11-atomic-exec-5.c"
; pthread_join (thread_id, 
# 429 "./atomic/c11-atomic-exec-5.c" 3 4
((void *)0)
# 429 "./atomic/c11-atomic-exec-5.c"
); printf ("complex_long_double_div_overflow" " (a) %d pass, %d fail; (b) %d pass, %d fail\n", num_1_pass, num_1_fail, num_2_pass, num_2_fail); return num_1_fail || num_2_fail; }



int
main (void)
{
  int ret = 0;
  ret |= test_main_float_add_invalid ();
  ret |= test_main_float_add_invalid_prev ();
  ret |= test_main_float_add_overflow ();
  ret |= test_main_float_add_overflow_prev ();
  ret |= test_main_float_add_overflow_double ();
  ret |= test_main_float_add_overflow_long_double ();
  ret |= test_main_float_add_inexact ();
  ret |= test_main_float_add_inexact_int ();
  ret |= test_main_float_preinc_inexact ();
  ret |= test_main_float_postinc_inexact ();

  ret |= test_main_long_add_float_inexact ();

  ret |= test_main_complex_float_add_overflow ();
  ret |= test_main_float_sub_invalid ();
  ret |= test_main_float_sub_overflow ();
  ret |= test_main_float_sub_inexact ();
  ret |= test_main_float_sub_inexact_int ();
  ret |= test_main_float_predec_inexact ();
  ret |= test_main_float_postdec_inexact ();

  ret |= test_main_long_sub_float_inexact ();

  ret |= test_main_complex_float_sub_overflow ();
  ret |= test_main_float_mul_invalid ();
  ret |= test_main_float_mul_overflow ();
  ret |= test_main_float_mul_underflow ();
  ret |= test_main_float_mul_inexact ();
  ret |= test_main_float_mul_inexact_int ();

  ret |= test_main_long_mul_float_inexact ();

  ret |= test_main_complex_float_mul_overflow ();
  ret |= test_main_float_div_invalid_divbyzero ();
  ret |= test_main_float_div_overflow ();
  ret |= test_main_float_div_underflow ();
  ret |= test_main_float_div_inexact ();
  ret |= test_main_float_div_inexact_int ();
  ret |= test_main_int_div_float_inexact ();
  ret |= test_main_complex_float_div_overflow ();
  ret |= test_main_double_add_invalid ();
  ret |= test_main_double_add_overflow ();
  ret |= test_main_double_add_overflow_long_double ();
  ret |= test_main_double_add_inexact ();
  ret |= test_main_double_add_inexact_int ();
  ret |= test_main_double_preinc_inexact ();
  ret |= test_main_double_postinc_inexact ();

  ret |= test_main_long_long_add_double_inexact ();

  ret |= test_main_complex_double_add_overflow ();
  ret |= test_main_double_sub_invalid ();
  ret |= test_main_double_sub_overflow ();
  ret |= test_main_double_sub_inexact ();
  ret |= test_main_double_sub_inexact_int ();
  ret |= test_main_double_predec_inexact ();
  ret |= test_main_double_postdec_inexact ();

  ret |= test_main_long_long_sub_double_inexact ();

  ret |= test_main_complex_double_sub_overflow ();
  ret |= test_main_double_mul_invalid ();
  ret |= test_main_double_mul_overflow ();
  ret |= test_main_double_mul_overflow_float ();
  ret |= test_main_double_mul_underflow ();
  ret |= test_main_double_mul_inexact ();
  ret |= test_main_double_mul_inexact_int ();

  ret |= test_main_long_long_mul_double_inexact ();

  ret |= test_main_complex_double_mul_overflow ();
  ret |= test_main_double_div_invalid_divbyzero ();
  ret |= test_main_double_div_overflow ();
  ret |= test_main_double_div_underflow ();
  ret |= test_main_double_div_inexact ();
  ret |= test_main_double_div_inexact_int ();
  ret |= test_main_int_div_double_inexact ();
  ret |= test_main_complex_double_div_overflow ();
  ret |= test_main_long_double_add_invalid ();

  ret |= test_main_long_double_add_overflow ();
  ret |= test_main_long_double_add_inexact ();
  ret |= test_main_long_double_add_inexact_int ();
  ret |= test_main_long_double_preinc_inexact ();
  ret |= test_main_long_double_postinc_inexact ();
  ret |= test_main_complex_long_double_add_overflow ();

  ret |= test_main_long_double_sub_invalid ();

  ret |= test_main_long_double_sub_overflow ();
  ret |= test_main_long_double_sub_inexact ();
  ret |= test_main_long_double_sub_inexact_int ();
  ret |= test_main_long_double_predec_inexact ();
  ret |= test_main_long_double_postdec_inexact ();
  ret |= test_main_complex_long_double_sub_overflow ();

  ret |= test_main_long_double_mul_invalid ();
  ret |= test_main_long_double_mul_overflow ();
  ret |= test_main_long_double_mul_overflow_float ();
  ret |= test_main_long_double_mul_overflow_double ();
  ret |= test_main_long_double_mul_underflow ();

  ret |= test_main_long_double_mul_inexact ();
  ret |= test_main_long_double_mul_inexact_int ();

  ret |= test_main_complex_long_double_mul_overflow ();
  ret |= test_main_long_double_div_invalid_divbyzero ();
  ret |= test_main_long_double_div_overflow ();
  ret |= test_main_long_double_div_underflow ();
  ret |= test_main_long_double_div_inexact ();
  ret |= test_main_long_double_div_inexact_int ();
  ret |= test_main_int_div_long_double_inexact ();
  ret |= test_main_complex_long_double_div_overflow ();
  if (ret != 0)
    abort ();
  else
    exit (0);
}
