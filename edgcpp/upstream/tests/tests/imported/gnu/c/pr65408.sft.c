//type: rp
//options: 
# 0 "./pr65408.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr65408.c"




# 1 "/usr/include/sys/mman.h" 1 3 4
# 22 "/usr/include/sys/mman.h" 3 4
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
# 23 "/usr/include/sys/mman.h" 2 3 4
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
# 24 "/usr/include/sys/mman.h" 2 3 4

# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 26 "/usr/include/sys/mman.h" 2 3 4



typedef __off_t off_t;







typedef __mode_t mode_t;



# 1 "/usr/include/bits/mman.h" 1 3 4
# 42 "/usr/include/sys/mman.h" 2 3 4





# 57 "/usr/include/sys/mman.h" 3 4
extern void *mmap (void *__addr, size_t __len, int __prot,
     int __flags, int __fd, __off_t __offset) __attribute__ ((__nothrow__ , __leaf__));
# 76 "/usr/include/sys/mman.h" 3 4
extern int munmap (void *__addr, size_t __len) __attribute__ ((__nothrow__ , __leaf__));




extern int mprotect (void *__addr, size_t __len, int __prot) __attribute__ ((__nothrow__ , __leaf__));







extern int msync (void *__addr, size_t __len, int __flags);




extern int madvise (void *__addr, size_t __len, int __advice) __attribute__ ((__nothrow__ , __leaf__));



extern int posix_madvise (void *__addr, size_t __len, int __advice) __attribute__ ((__nothrow__ , __leaf__));




extern int mlock (const void *__addr, size_t __len) __attribute__ ((__nothrow__ , __leaf__));


extern int munlock (const void *__addr, size_t __len) __attribute__ ((__nothrow__ , __leaf__));




extern int mlockall (int __flags) __attribute__ ((__nothrow__ , __leaf__));



extern int munlockall (void) __attribute__ ((__nothrow__ , __leaf__));







extern int mincore (void *__start, size_t __len, unsigned char *__vec)
     __attribute__ ((__nothrow__ , __leaf__));
# 144 "/usr/include/sys/mman.h" 3 4
extern int shm_open (const char *__name, int __oflag, mode_t __mode);


extern int shm_unlink (const char *__name);


# 6 "./pr65408.c" 2
# 16 "./pr65408.c"

# 16 "./pr65408.c"
typedef struct
{
  unsigned char r;
  unsigned char g;
  unsigned char b;
} __attribute__((packed)) pr58744;

typedef struct
{
  unsigned short r;
  unsigned short g;
  unsigned short b;
} pr36043;

typedef struct
{
  int r;
  int g;
  int b;
} pr65408;

__attribute__ ((noinline, noclone))
void
f1a (pr58744 x)
{
  if (x.r != 1 || x.g != 2 || x.b != 3)
    __builtin_abort();
}

__attribute__ ((noinline, noclone))
void
f1 (pr58744 *x)
{
  f1a (*x);
}

__attribute__ ((noinline, noclone))
void
f2a (pr36043 x)
{
  if (x.r != 1 || x.g != 2 || x.b != 3)
    __builtin_abort();
}

__attribute__ ((noinline, noclone))
void
f2 (pr36043 *x)
{
  f2a (*x);
}

__attribute__ ((noinline, noclone))
void
f3a (pr65408 x)
{
  if (x.r != 1 || x.g != 2 || x.b != 3)
    __builtin_abort();
}

__attribute__ ((noinline, noclone))
void
f3 (pr65408 *x)
{
  f3a (*x);
}

int
main ()
{
  char *p = mmap ((void *) 0, 131072, 
# 85 "./pr65408.c" 3 4
                                     0x1 
# 85 "./pr65408.c"
                                               | 
# 85 "./pr65408.c" 3 4
                                                 0x2
# 85 "./pr65408.c"
                                                           ,
    
# 86 "./pr65408.c" 3 4
   0x02 
# 86 "./pr65408.c"
               | 
# 86 "./pr65408.c" 3 4
                 0x20
# 86 "./pr65408.c"
                              , -1, 0);
  if (p == 
# 87 "./pr65408.c" 3 4
          ((void *) -1)
# 87 "./pr65408.c"
                    )
    return 0;
  char *endp = p + 65536;
  if (munmap (endp, 65536) < 0)
    return 0;

  pr58744 *s1 = (pr58744 *) endp - 1;
  s1->r = 1;
  s1->g = 2;
  s1->b = 3;
  f1 (s1);

  pr36043 *s2 = (pr36043 *) endp - 1;
  s2->r = 1;
  s2->g = 2;
  s2->b = 3;
  f2 (s2);

  pr65408 *s3 = (pr65408 *) endp - 1;
  s3->r = 1;
  s3->g = 2;
  s3->b = 3;
  f3 (s3);

  return 0;
}
