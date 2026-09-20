#ifndef THREADED_LAYOUT_NATIVE_INTRINSICS_H
#define THREADED_LAYOUT_NATIVE_INTRINSICS_H
/* GCC-only declaration support for DotCC's intrinsic runtime types. None of
   these declarations replaces a measured upstream or pthread/signal record.
   This TU does not call native libc using the managed declarations. */
typedef __builtin_va_list VaList;
#define va_start(ap,last) __builtin_va_start(ap,last)
#define va_arg(ap,type) __builtin_va_arg(ap,type)
#define va_end(ap) __builtin_va_end(ap)
#define va_copy(dst,src) __builtin_va_copy(dst,src)
#define offsetof(type,member) __builtin_offsetof(type,member)
typedef struct { int quot; int rem; } div_t;
typedef struct { long quot; long rem; } ldiv_t;
typedef struct { long quot; long rem; } lldiv_t;
typedef struct { long quot; long rem; } imaxdiv_t;
/* FILE occurs only as an opaque pointer in this layout closure; native-main
   uses the system FILE in its separately compiled actual-libc translation. */
typedef struct { int _slot; } FILE;
struct timespec { long tv_sec; long tv_nsec; };
#endif
