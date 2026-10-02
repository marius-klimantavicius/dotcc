//type: rp
//options: --c99 --strict_gnu
# 0 "./fixed-point/func-vararg.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./fixed-point/func-vararg.c"







# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 9 "./fixed-point/func-vararg.c" 2


# 10 "./fixed-point/func-vararg.c"
extern void abort (void);
# 26 "./fixed-point/func-vararg.c"
static short _Fract vararg_sf (unsigned arg, ...) { int i; va_list ap; short _Fract result; 
# 26 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 26 "./fixed-point/func-vararg.c"
ap
# 26 "./fixed-point/func-vararg.c" 3 4
,
# 26 "./fixed-point/func-vararg.c"
arg
# 26 "./fixed-point/func-vararg.c" 3 4
)
# 26 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 26 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 26 "./fixed-point/func-vararg.c"
ap
# 26 "./fixed-point/func-vararg.c" 3 4
,
# 26 "./fixed-point/func-vararg.c"
short _Fract
# 26 "./fixed-point/func-vararg.c" 3 4
)
# 26 "./fixed-point/func-vararg.c"
; 
# 26 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 26 "./fixed-point/func-vararg.c"
ap
# 26 "./fixed-point/func-vararg.c" 3 4
)
# 26 "./fixed-point/func-vararg.c"
; return result; }
static _Fract vararg_f (unsigned arg, ...) { int i; va_list ap; _Fract result; 
# 27 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 27 "./fixed-point/func-vararg.c"
ap
# 27 "./fixed-point/func-vararg.c" 3 4
,
# 27 "./fixed-point/func-vararg.c"
arg
# 27 "./fixed-point/func-vararg.c" 3 4
)
# 27 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 27 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 27 "./fixed-point/func-vararg.c"
ap
# 27 "./fixed-point/func-vararg.c" 3 4
,
# 27 "./fixed-point/func-vararg.c"
_Fract
# 27 "./fixed-point/func-vararg.c" 3 4
)
# 27 "./fixed-point/func-vararg.c"
; 
# 27 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 27 "./fixed-point/func-vararg.c"
ap
# 27 "./fixed-point/func-vararg.c" 3 4
)
# 27 "./fixed-point/func-vararg.c"
; return result; }
static long _Fract vararg_lf (unsigned arg, ...) { int i; va_list ap; long _Fract result; 
# 28 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 28 "./fixed-point/func-vararg.c"
ap
# 28 "./fixed-point/func-vararg.c" 3 4
,
# 28 "./fixed-point/func-vararg.c"
arg
# 28 "./fixed-point/func-vararg.c" 3 4
)
# 28 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 28 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 28 "./fixed-point/func-vararg.c"
ap
# 28 "./fixed-point/func-vararg.c" 3 4
,
# 28 "./fixed-point/func-vararg.c"
long _Fract
# 28 "./fixed-point/func-vararg.c" 3 4
)
# 28 "./fixed-point/func-vararg.c"
; 
# 28 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 28 "./fixed-point/func-vararg.c"
ap
# 28 "./fixed-point/func-vararg.c" 3 4
)
# 28 "./fixed-point/func-vararg.c"
; return result; }
static long long _Fract vararg_llf (unsigned arg, ...) { int i; va_list ap; long long _Fract result; 
# 29 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 29 "./fixed-point/func-vararg.c"
ap
# 29 "./fixed-point/func-vararg.c" 3 4
,
# 29 "./fixed-point/func-vararg.c"
arg
# 29 "./fixed-point/func-vararg.c" 3 4
)
# 29 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 29 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 29 "./fixed-point/func-vararg.c"
ap
# 29 "./fixed-point/func-vararg.c" 3 4
,
# 29 "./fixed-point/func-vararg.c"
long long _Fract
# 29 "./fixed-point/func-vararg.c" 3 4
)
# 29 "./fixed-point/func-vararg.c"
; 
# 29 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 29 "./fixed-point/func-vararg.c"
ap
# 29 "./fixed-point/func-vararg.c" 3 4
)
# 29 "./fixed-point/func-vararg.c"
; return result; }
static unsigned short _Fract vararg_usf (unsigned arg, ...) { int i; va_list ap; unsigned short _Fract result; 
# 30 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 30 "./fixed-point/func-vararg.c"
ap
# 30 "./fixed-point/func-vararg.c" 3 4
,
# 30 "./fixed-point/func-vararg.c"
arg
# 30 "./fixed-point/func-vararg.c" 3 4
)
# 30 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 30 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 30 "./fixed-point/func-vararg.c"
ap
# 30 "./fixed-point/func-vararg.c" 3 4
,
# 30 "./fixed-point/func-vararg.c"
unsigned short _Fract
# 30 "./fixed-point/func-vararg.c" 3 4
)
# 30 "./fixed-point/func-vararg.c"
; 
# 30 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 30 "./fixed-point/func-vararg.c"
ap
# 30 "./fixed-point/func-vararg.c" 3 4
)
# 30 "./fixed-point/func-vararg.c"
; return result; }
static unsigned _Fract vararg_uf (unsigned arg, ...) { int i; va_list ap; unsigned _Fract result; 
# 31 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 31 "./fixed-point/func-vararg.c"
ap
# 31 "./fixed-point/func-vararg.c" 3 4
,
# 31 "./fixed-point/func-vararg.c"
arg
# 31 "./fixed-point/func-vararg.c" 3 4
)
# 31 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 31 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 31 "./fixed-point/func-vararg.c"
ap
# 31 "./fixed-point/func-vararg.c" 3 4
,
# 31 "./fixed-point/func-vararg.c"
unsigned _Fract
# 31 "./fixed-point/func-vararg.c" 3 4
)
# 31 "./fixed-point/func-vararg.c"
; 
# 31 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 31 "./fixed-point/func-vararg.c"
ap
# 31 "./fixed-point/func-vararg.c" 3 4
)
# 31 "./fixed-point/func-vararg.c"
; return result; }
static unsigned long _Fract vararg_ulf (unsigned arg, ...) { int i; va_list ap; unsigned long _Fract result; 
# 32 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 32 "./fixed-point/func-vararg.c"
ap
# 32 "./fixed-point/func-vararg.c" 3 4
,
# 32 "./fixed-point/func-vararg.c"
arg
# 32 "./fixed-point/func-vararg.c" 3 4
)
# 32 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 32 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 32 "./fixed-point/func-vararg.c"
ap
# 32 "./fixed-point/func-vararg.c" 3 4
,
# 32 "./fixed-point/func-vararg.c"
unsigned long _Fract
# 32 "./fixed-point/func-vararg.c" 3 4
)
# 32 "./fixed-point/func-vararg.c"
; 
# 32 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 32 "./fixed-point/func-vararg.c"
ap
# 32 "./fixed-point/func-vararg.c" 3 4
)
# 32 "./fixed-point/func-vararg.c"
; return result; }
static unsigned long long _Fract vararg_ullf (unsigned arg, ...) { int i; va_list ap; unsigned long long _Fract result; 
# 33 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 33 "./fixed-point/func-vararg.c"
ap
# 33 "./fixed-point/func-vararg.c" 3 4
,
# 33 "./fixed-point/func-vararg.c"
arg
# 33 "./fixed-point/func-vararg.c" 3 4
)
# 33 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 33 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 33 "./fixed-point/func-vararg.c"
ap
# 33 "./fixed-point/func-vararg.c" 3 4
,
# 33 "./fixed-point/func-vararg.c"
unsigned long long _Fract
# 33 "./fixed-point/func-vararg.c" 3 4
)
# 33 "./fixed-point/func-vararg.c"
; 
# 33 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 33 "./fixed-point/func-vararg.c"
ap
# 33 "./fixed-point/func-vararg.c" 3 4
)
# 33 "./fixed-point/func-vararg.c"
; return result; }
static _Sat short _Fract vararg_Ssf (unsigned arg, ...) { int i; va_list ap; _Sat short _Fract result; 
# 34 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 34 "./fixed-point/func-vararg.c"
ap
# 34 "./fixed-point/func-vararg.c" 3 4
,
# 34 "./fixed-point/func-vararg.c"
arg
# 34 "./fixed-point/func-vararg.c" 3 4
)
# 34 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 34 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 34 "./fixed-point/func-vararg.c"
ap
# 34 "./fixed-point/func-vararg.c" 3 4
,
# 34 "./fixed-point/func-vararg.c"
_Sat short _Fract
# 34 "./fixed-point/func-vararg.c" 3 4
)
# 34 "./fixed-point/func-vararg.c"
; 
# 34 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 34 "./fixed-point/func-vararg.c"
ap
# 34 "./fixed-point/func-vararg.c" 3 4
)
# 34 "./fixed-point/func-vararg.c"
; return result; }
static _Sat _Fract vararg_Sf (unsigned arg, ...) { int i; va_list ap; _Sat _Fract result; 
# 35 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 35 "./fixed-point/func-vararg.c"
ap
# 35 "./fixed-point/func-vararg.c" 3 4
,
# 35 "./fixed-point/func-vararg.c"
arg
# 35 "./fixed-point/func-vararg.c" 3 4
)
# 35 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 35 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 35 "./fixed-point/func-vararg.c"
ap
# 35 "./fixed-point/func-vararg.c" 3 4
,
# 35 "./fixed-point/func-vararg.c"
_Sat _Fract
# 35 "./fixed-point/func-vararg.c" 3 4
)
# 35 "./fixed-point/func-vararg.c"
; 
# 35 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 35 "./fixed-point/func-vararg.c"
ap
# 35 "./fixed-point/func-vararg.c" 3 4
)
# 35 "./fixed-point/func-vararg.c"
; return result; }
static _Sat long _Fract vararg_Slf (unsigned arg, ...) { int i; va_list ap; _Sat long _Fract result; 
# 36 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 36 "./fixed-point/func-vararg.c"
ap
# 36 "./fixed-point/func-vararg.c" 3 4
,
# 36 "./fixed-point/func-vararg.c"
arg
# 36 "./fixed-point/func-vararg.c" 3 4
)
# 36 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 36 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 36 "./fixed-point/func-vararg.c"
ap
# 36 "./fixed-point/func-vararg.c" 3 4
,
# 36 "./fixed-point/func-vararg.c"
_Sat long _Fract
# 36 "./fixed-point/func-vararg.c" 3 4
)
# 36 "./fixed-point/func-vararg.c"
; 
# 36 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 36 "./fixed-point/func-vararg.c"
ap
# 36 "./fixed-point/func-vararg.c" 3 4
)
# 36 "./fixed-point/func-vararg.c"
; return result; }
static _Sat long long _Fract vararg_Sllf (unsigned arg, ...) { int i; va_list ap; _Sat long long _Fract result; 
# 37 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 37 "./fixed-point/func-vararg.c"
ap
# 37 "./fixed-point/func-vararg.c" 3 4
,
# 37 "./fixed-point/func-vararg.c"
arg
# 37 "./fixed-point/func-vararg.c" 3 4
)
# 37 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 37 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 37 "./fixed-point/func-vararg.c"
ap
# 37 "./fixed-point/func-vararg.c" 3 4
,
# 37 "./fixed-point/func-vararg.c"
_Sat long long _Fract
# 37 "./fixed-point/func-vararg.c" 3 4
)
# 37 "./fixed-point/func-vararg.c"
; 
# 37 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 37 "./fixed-point/func-vararg.c"
ap
# 37 "./fixed-point/func-vararg.c" 3 4
)
# 37 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned short _Fract vararg_Susf (unsigned arg, ...) { int i; va_list ap; _Sat unsigned short _Fract result; 
# 38 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 38 "./fixed-point/func-vararg.c"
ap
# 38 "./fixed-point/func-vararg.c" 3 4
,
# 38 "./fixed-point/func-vararg.c"
arg
# 38 "./fixed-point/func-vararg.c" 3 4
)
# 38 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 38 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 38 "./fixed-point/func-vararg.c"
ap
# 38 "./fixed-point/func-vararg.c" 3 4
,
# 38 "./fixed-point/func-vararg.c"
_Sat unsigned short _Fract
# 38 "./fixed-point/func-vararg.c" 3 4
)
# 38 "./fixed-point/func-vararg.c"
; 
# 38 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 38 "./fixed-point/func-vararg.c"
ap
# 38 "./fixed-point/func-vararg.c" 3 4
)
# 38 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned _Fract vararg_Suf (unsigned arg, ...) { int i; va_list ap; _Sat unsigned _Fract result; 
# 39 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 39 "./fixed-point/func-vararg.c"
ap
# 39 "./fixed-point/func-vararg.c" 3 4
,
# 39 "./fixed-point/func-vararg.c"
arg
# 39 "./fixed-point/func-vararg.c" 3 4
)
# 39 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 39 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 39 "./fixed-point/func-vararg.c"
ap
# 39 "./fixed-point/func-vararg.c" 3 4
,
# 39 "./fixed-point/func-vararg.c"
_Sat unsigned _Fract
# 39 "./fixed-point/func-vararg.c" 3 4
)
# 39 "./fixed-point/func-vararg.c"
; 
# 39 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 39 "./fixed-point/func-vararg.c"
ap
# 39 "./fixed-point/func-vararg.c" 3 4
)
# 39 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned long _Fract vararg_Sulf (unsigned arg, ...) { int i; va_list ap; _Sat unsigned long _Fract result; 
# 40 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 40 "./fixed-point/func-vararg.c"
ap
# 40 "./fixed-point/func-vararg.c" 3 4
,
# 40 "./fixed-point/func-vararg.c"
arg
# 40 "./fixed-point/func-vararg.c" 3 4
)
# 40 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 40 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 40 "./fixed-point/func-vararg.c"
ap
# 40 "./fixed-point/func-vararg.c" 3 4
,
# 40 "./fixed-point/func-vararg.c"
_Sat unsigned long _Fract
# 40 "./fixed-point/func-vararg.c" 3 4
)
# 40 "./fixed-point/func-vararg.c"
; 
# 40 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 40 "./fixed-point/func-vararg.c"
ap
# 40 "./fixed-point/func-vararg.c" 3 4
)
# 40 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned long long _Fract vararg_Sullf (unsigned arg, ...) { int i; va_list ap; _Sat unsigned long long _Fract result; 
# 41 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 41 "./fixed-point/func-vararg.c"
ap
# 41 "./fixed-point/func-vararg.c" 3 4
,
# 41 "./fixed-point/func-vararg.c"
arg
# 41 "./fixed-point/func-vararg.c" 3 4
)
# 41 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 41 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 41 "./fixed-point/func-vararg.c"
ap
# 41 "./fixed-point/func-vararg.c" 3 4
,
# 41 "./fixed-point/func-vararg.c"
_Sat unsigned long long _Fract
# 41 "./fixed-point/func-vararg.c" 3 4
)
# 41 "./fixed-point/func-vararg.c"
; 
# 41 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 41 "./fixed-point/func-vararg.c"
ap
# 41 "./fixed-point/func-vararg.c" 3 4
)
# 41 "./fixed-point/func-vararg.c"
; return result; }
static short _Accum vararg_sa (unsigned arg, ...) { int i; va_list ap; short _Accum result; 
# 42 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 42 "./fixed-point/func-vararg.c"
ap
# 42 "./fixed-point/func-vararg.c" 3 4
,
# 42 "./fixed-point/func-vararg.c"
arg
# 42 "./fixed-point/func-vararg.c" 3 4
)
# 42 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 42 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 42 "./fixed-point/func-vararg.c"
ap
# 42 "./fixed-point/func-vararg.c" 3 4
,
# 42 "./fixed-point/func-vararg.c"
short _Accum
# 42 "./fixed-point/func-vararg.c" 3 4
)
# 42 "./fixed-point/func-vararg.c"
; 
# 42 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 42 "./fixed-point/func-vararg.c"
ap
# 42 "./fixed-point/func-vararg.c" 3 4
)
# 42 "./fixed-point/func-vararg.c"
; return result; }
static _Accum vararg_a (unsigned arg, ...) { int i; va_list ap; _Accum result; 
# 43 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 43 "./fixed-point/func-vararg.c"
ap
# 43 "./fixed-point/func-vararg.c" 3 4
,
# 43 "./fixed-point/func-vararg.c"
arg
# 43 "./fixed-point/func-vararg.c" 3 4
)
# 43 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 43 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 43 "./fixed-point/func-vararg.c"
ap
# 43 "./fixed-point/func-vararg.c" 3 4
,
# 43 "./fixed-point/func-vararg.c"
_Accum
# 43 "./fixed-point/func-vararg.c" 3 4
)
# 43 "./fixed-point/func-vararg.c"
; 
# 43 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 43 "./fixed-point/func-vararg.c"
ap
# 43 "./fixed-point/func-vararg.c" 3 4
)
# 43 "./fixed-point/func-vararg.c"
; return result; }
static long _Accum vararg_la (unsigned arg, ...) { int i; va_list ap; long _Accum result; 
# 44 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 44 "./fixed-point/func-vararg.c"
ap
# 44 "./fixed-point/func-vararg.c" 3 4
,
# 44 "./fixed-point/func-vararg.c"
arg
# 44 "./fixed-point/func-vararg.c" 3 4
)
# 44 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 44 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 44 "./fixed-point/func-vararg.c"
ap
# 44 "./fixed-point/func-vararg.c" 3 4
,
# 44 "./fixed-point/func-vararg.c"
long _Accum
# 44 "./fixed-point/func-vararg.c" 3 4
)
# 44 "./fixed-point/func-vararg.c"
; 
# 44 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 44 "./fixed-point/func-vararg.c"
ap
# 44 "./fixed-point/func-vararg.c" 3 4
)
# 44 "./fixed-point/func-vararg.c"
; return result; }
static long long _Accum vararg_lla (unsigned arg, ...) { int i; va_list ap; long long _Accum result; 
# 45 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 45 "./fixed-point/func-vararg.c"
ap
# 45 "./fixed-point/func-vararg.c" 3 4
,
# 45 "./fixed-point/func-vararg.c"
arg
# 45 "./fixed-point/func-vararg.c" 3 4
)
# 45 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 45 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 45 "./fixed-point/func-vararg.c"
ap
# 45 "./fixed-point/func-vararg.c" 3 4
,
# 45 "./fixed-point/func-vararg.c"
long long _Accum
# 45 "./fixed-point/func-vararg.c" 3 4
)
# 45 "./fixed-point/func-vararg.c"
; 
# 45 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 45 "./fixed-point/func-vararg.c"
ap
# 45 "./fixed-point/func-vararg.c" 3 4
)
# 45 "./fixed-point/func-vararg.c"
; return result; }
static unsigned short _Accum vararg_usa (unsigned arg, ...) { int i; va_list ap; unsigned short _Accum result; 
# 46 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 46 "./fixed-point/func-vararg.c"
ap
# 46 "./fixed-point/func-vararg.c" 3 4
,
# 46 "./fixed-point/func-vararg.c"
arg
# 46 "./fixed-point/func-vararg.c" 3 4
)
# 46 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 46 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 46 "./fixed-point/func-vararg.c"
ap
# 46 "./fixed-point/func-vararg.c" 3 4
,
# 46 "./fixed-point/func-vararg.c"
unsigned short _Accum
# 46 "./fixed-point/func-vararg.c" 3 4
)
# 46 "./fixed-point/func-vararg.c"
; 
# 46 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 46 "./fixed-point/func-vararg.c"
ap
# 46 "./fixed-point/func-vararg.c" 3 4
)
# 46 "./fixed-point/func-vararg.c"
; return result; }
static unsigned _Accum vararg_ua (unsigned arg, ...) { int i; va_list ap; unsigned _Accum result; 
# 47 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 47 "./fixed-point/func-vararg.c"
ap
# 47 "./fixed-point/func-vararg.c" 3 4
,
# 47 "./fixed-point/func-vararg.c"
arg
# 47 "./fixed-point/func-vararg.c" 3 4
)
# 47 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 47 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 47 "./fixed-point/func-vararg.c"
ap
# 47 "./fixed-point/func-vararg.c" 3 4
,
# 47 "./fixed-point/func-vararg.c"
unsigned _Accum
# 47 "./fixed-point/func-vararg.c" 3 4
)
# 47 "./fixed-point/func-vararg.c"
; 
# 47 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 47 "./fixed-point/func-vararg.c"
ap
# 47 "./fixed-point/func-vararg.c" 3 4
)
# 47 "./fixed-point/func-vararg.c"
; return result; }
static unsigned long _Accum vararg_ula (unsigned arg, ...) { int i; va_list ap; unsigned long _Accum result; 
# 48 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 48 "./fixed-point/func-vararg.c"
ap
# 48 "./fixed-point/func-vararg.c" 3 4
,
# 48 "./fixed-point/func-vararg.c"
arg
# 48 "./fixed-point/func-vararg.c" 3 4
)
# 48 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 48 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 48 "./fixed-point/func-vararg.c"
ap
# 48 "./fixed-point/func-vararg.c" 3 4
,
# 48 "./fixed-point/func-vararg.c"
unsigned long _Accum
# 48 "./fixed-point/func-vararg.c" 3 4
)
# 48 "./fixed-point/func-vararg.c"
; 
# 48 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 48 "./fixed-point/func-vararg.c"
ap
# 48 "./fixed-point/func-vararg.c" 3 4
)
# 48 "./fixed-point/func-vararg.c"
; return result; }
static unsigned long long _Accum vararg_ulla (unsigned arg, ...) { int i; va_list ap; unsigned long long _Accum result; 
# 49 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 49 "./fixed-point/func-vararg.c"
ap
# 49 "./fixed-point/func-vararg.c" 3 4
,
# 49 "./fixed-point/func-vararg.c"
arg
# 49 "./fixed-point/func-vararg.c" 3 4
)
# 49 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 49 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 49 "./fixed-point/func-vararg.c"
ap
# 49 "./fixed-point/func-vararg.c" 3 4
,
# 49 "./fixed-point/func-vararg.c"
unsigned long long _Accum
# 49 "./fixed-point/func-vararg.c" 3 4
)
# 49 "./fixed-point/func-vararg.c"
; 
# 49 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 49 "./fixed-point/func-vararg.c"
ap
# 49 "./fixed-point/func-vararg.c" 3 4
)
# 49 "./fixed-point/func-vararg.c"
; return result; }
static _Sat short _Accum vararg_Ssa (unsigned arg, ...) { int i; va_list ap; _Sat short _Accum result; 
# 50 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 50 "./fixed-point/func-vararg.c"
ap
# 50 "./fixed-point/func-vararg.c" 3 4
,
# 50 "./fixed-point/func-vararg.c"
arg
# 50 "./fixed-point/func-vararg.c" 3 4
)
# 50 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 50 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 50 "./fixed-point/func-vararg.c"
ap
# 50 "./fixed-point/func-vararg.c" 3 4
,
# 50 "./fixed-point/func-vararg.c"
_Sat short _Accum
# 50 "./fixed-point/func-vararg.c" 3 4
)
# 50 "./fixed-point/func-vararg.c"
; 
# 50 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 50 "./fixed-point/func-vararg.c"
ap
# 50 "./fixed-point/func-vararg.c" 3 4
)
# 50 "./fixed-point/func-vararg.c"
; return result; }
static _Sat _Accum vararg_Sa (unsigned arg, ...) { int i; va_list ap; _Sat _Accum result; 
# 51 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 51 "./fixed-point/func-vararg.c"
ap
# 51 "./fixed-point/func-vararg.c" 3 4
,
# 51 "./fixed-point/func-vararg.c"
arg
# 51 "./fixed-point/func-vararg.c" 3 4
)
# 51 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 51 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 51 "./fixed-point/func-vararg.c"
ap
# 51 "./fixed-point/func-vararg.c" 3 4
,
# 51 "./fixed-point/func-vararg.c"
_Sat _Accum
# 51 "./fixed-point/func-vararg.c" 3 4
)
# 51 "./fixed-point/func-vararg.c"
; 
# 51 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 51 "./fixed-point/func-vararg.c"
ap
# 51 "./fixed-point/func-vararg.c" 3 4
)
# 51 "./fixed-point/func-vararg.c"
; return result; }
static _Sat long _Accum vararg_Sla (unsigned arg, ...) { int i; va_list ap; _Sat long _Accum result; 
# 52 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 52 "./fixed-point/func-vararg.c"
ap
# 52 "./fixed-point/func-vararg.c" 3 4
,
# 52 "./fixed-point/func-vararg.c"
arg
# 52 "./fixed-point/func-vararg.c" 3 4
)
# 52 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 52 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 52 "./fixed-point/func-vararg.c"
ap
# 52 "./fixed-point/func-vararg.c" 3 4
,
# 52 "./fixed-point/func-vararg.c"
_Sat long _Accum
# 52 "./fixed-point/func-vararg.c" 3 4
)
# 52 "./fixed-point/func-vararg.c"
; 
# 52 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 52 "./fixed-point/func-vararg.c"
ap
# 52 "./fixed-point/func-vararg.c" 3 4
)
# 52 "./fixed-point/func-vararg.c"
; return result; }
static _Sat long long _Accum vararg_Slla (unsigned arg, ...) { int i; va_list ap; _Sat long long _Accum result; 
# 53 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 53 "./fixed-point/func-vararg.c"
ap
# 53 "./fixed-point/func-vararg.c" 3 4
,
# 53 "./fixed-point/func-vararg.c"
arg
# 53 "./fixed-point/func-vararg.c" 3 4
)
# 53 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 53 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 53 "./fixed-point/func-vararg.c"
ap
# 53 "./fixed-point/func-vararg.c" 3 4
,
# 53 "./fixed-point/func-vararg.c"
_Sat long long _Accum
# 53 "./fixed-point/func-vararg.c" 3 4
)
# 53 "./fixed-point/func-vararg.c"
; 
# 53 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 53 "./fixed-point/func-vararg.c"
ap
# 53 "./fixed-point/func-vararg.c" 3 4
)
# 53 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned short _Accum vararg_Susa (unsigned arg, ...) { int i; va_list ap; _Sat unsigned short _Accum result; 
# 54 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 54 "./fixed-point/func-vararg.c"
ap
# 54 "./fixed-point/func-vararg.c" 3 4
,
# 54 "./fixed-point/func-vararg.c"
arg
# 54 "./fixed-point/func-vararg.c" 3 4
)
# 54 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 54 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 54 "./fixed-point/func-vararg.c"
ap
# 54 "./fixed-point/func-vararg.c" 3 4
,
# 54 "./fixed-point/func-vararg.c"
_Sat unsigned short _Accum
# 54 "./fixed-point/func-vararg.c" 3 4
)
# 54 "./fixed-point/func-vararg.c"
; 
# 54 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 54 "./fixed-point/func-vararg.c"
ap
# 54 "./fixed-point/func-vararg.c" 3 4
)
# 54 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned _Accum vararg_Sua (unsigned arg, ...) { int i; va_list ap; _Sat unsigned _Accum result; 
# 55 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 55 "./fixed-point/func-vararg.c"
ap
# 55 "./fixed-point/func-vararg.c" 3 4
,
# 55 "./fixed-point/func-vararg.c"
arg
# 55 "./fixed-point/func-vararg.c" 3 4
)
# 55 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 55 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 55 "./fixed-point/func-vararg.c"
ap
# 55 "./fixed-point/func-vararg.c" 3 4
,
# 55 "./fixed-point/func-vararg.c"
_Sat unsigned _Accum
# 55 "./fixed-point/func-vararg.c" 3 4
)
# 55 "./fixed-point/func-vararg.c"
; 
# 55 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 55 "./fixed-point/func-vararg.c"
ap
# 55 "./fixed-point/func-vararg.c" 3 4
)
# 55 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned long _Accum vararg_Sula (unsigned arg, ...) { int i; va_list ap; _Sat unsigned long _Accum result; 
# 56 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 56 "./fixed-point/func-vararg.c"
ap
# 56 "./fixed-point/func-vararg.c" 3 4
,
# 56 "./fixed-point/func-vararg.c"
arg
# 56 "./fixed-point/func-vararg.c" 3 4
)
# 56 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 56 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 56 "./fixed-point/func-vararg.c"
ap
# 56 "./fixed-point/func-vararg.c" 3 4
,
# 56 "./fixed-point/func-vararg.c"
_Sat unsigned long _Accum
# 56 "./fixed-point/func-vararg.c" 3 4
)
# 56 "./fixed-point/func-vararg.c"
; 
# 56 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 56 "./fixed-point/func-vararg.c"
ap
# 56 "./fixed-point/func-vararg.c" 3 4
)
# 56 "./fixed-point/func-vararg.c"
; return result; }
static _Sat unsigned long long _Accum vararg_Sulla (unsigned arg, ...) { int i; va_list ap; _Sat unsigned long long _Accum result; 
# 57 "./fixed-point/func-vararg.c" 3 4
__builtin_va_start(
# 57 "./fixed-point/func-vararg.c"
ap
# 57 "./fixed-point/func-vararg.c" 3 4
,
# 57 "./fixed-point/func-vararg.c"
arg
# 57 "./fixed-point/func-vararg.c" 3 4
)
# 57 "./fixed-point/func-vararg.c"
; for (i = 0; i <= arg; i++) result = 
# 57 "./fixed-point/func-vararg.c" 3 4
__builtin_va_arg(
# 57 "./fixed-point/func-vararg.c"
ap
# 57 "./fixed-point/func-vararg.c" 3 4
,
# 57 "./fixed-point/func-vararg.c"
_Sat unsigned long long _Accum
# 57 "./fixed-point/func-vararg.c" 3 4
)
# 57 "./fixed-point/func-vararg.c"
; 
# 57 "./fixed-point/func-vararg.c" 3 4
__builtin_va_end(
# 57 "./fixed-point/func-vararg.c"
ap
# 57 "./fixed-point/func-vararg.c" 3 4
)
# 57 "./fixed-point/func-vararg.c"
; return result; }

int main()
{
# 81 "./fixed-point/func-vararg.c"
  if (vararg_sf (0, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.0hr) abort (); if (vararg_sf (1, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.1hr) abort (); if (vararg_sf (2, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.2hr) abort (); if (vararg_sf (3, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.3hr) abort (); if (vararg_sf (4, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.4hr) abort (); if (vararg_sf (5, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.5hr) abort ();
  if (vararg_f (0, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.0r) abort (); if (vararg_f (1, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.1r) abort (); if (vararg_f (2, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.2r) abort (); if (vararg_f (3, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.3r) abort (); if (vararg_f (4, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.4r) abort (); if (vararg_f (5, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.5r) abort ();
  if (vararg_lf (0, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.0lr) abort (); if (vararg_lf (1, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.1lr) abort (); if (vararg_lf (2, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.2lr) abort (); if (vararg_lf (3, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.3lr) abort (); if (vararg_lf (4, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.4lr) abort (); if (vararg_lf (5, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.5lr) abort ();
  if (vararg_llf (0, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.0llr) abort (); if (vararg_llf (1, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.1llr) abort (); if (vararg_llf (2, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.2llr) abort (); if (vararg_llf (3, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.3llr) abort (); if (vararg_llf (4, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.4llr) abort (); if (vararg_llf (5, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.5llr) abort ();
  if (vararg_usf (0, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.0uhr) abort (); if (vararg_usf (1, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.1uhr) abort (); if (vararg_usf (2, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.2uhr) abort (); if (vararg_usf (3, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.3uhr) abort (); if (vararg_usf (4, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.4uhr) abort (); if (vararg_usf (5, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.5uhr) abort ();
  if (vararg_uf (0, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.0ur) abort (); if (vararg_uf (1, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.1ur) abort (); if (vararg_uf (2, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.2ur) abort (); if (vararg_uf (3, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.3ur) abort (); if (vararg_uf (4, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.4ur) abort (); if (vararg_uf (5, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.5ur) abort ();
  if (vararg_ulf (0, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.0ulr) abort (); if (vararg_ulf (1, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.1ulr) abort (); if (vararg_ulf (2, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.2ulr) abort (); if (vararg_ulf (3, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.3ulr) abort (); if (vararg_ulf (4, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.4ulr) abort (); if (vararg_ulf (5, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.5ulr) abort ();
  if (vararg_ullf (0, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.0ullr) abort (); if (vararg_ullf (1, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.1ullr) abort (); if (vararg_ullf (2, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.2ullr) abort (); if (vararg_ullf (3, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.3ullr) abort (); if (vararg_ullf (4, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.4ullr) abort (); if (vararg_ullf (5, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.5ullr) abort ();
  if (vararg_Ssf (0, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.0hr) abort (); if (vararg_Ssf (1, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.1hr) abort (); if (vararg_Ssf (2, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.2hr) abort (); if (vararg_Ssf (3, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.3hr) abort (); if (vararg_Ssf (4, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.4hr) abort (); if (vararg_Ssf (5, 0.0hr, 0.1hr, 0.2hr, 0.3hr, 0.4hr, 0.5hr) != 0.5hr) abort ();
  if (vararg_Sf (0, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.0r) abort (); if (vararg_Sf (1, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.1r) abort (); if (vararg_Sf (2, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.2r) abort (); if (vararg_Sf (3, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.3r) abort (); if (vararg_Sf (4, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.4r) abort (); if (vararg_Sf (5, 0.0r, 0.1r, 0.2r, 0.3r, 0.4r, 0.5r) != 0.5r) abort ();
  if (vararg_Slf (0, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.0lr) abort (); if (vararg_Slf (1, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.1lr) abort (); if (vararg_Slf (2, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.2lr) abort (); if (vararg_Slf (3, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.3lr) abort (); if (vararg_Slf (4, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.4lr) abort (); if (vararg_Slf (5, 0.0lr, 0.1lr, 0.2lr, 0.3lr, 0.4lr, 0.5lr) != 0.5lr) abort ();
  if (vararg_Sllf (0, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.0llr) abort (); if (vararg_Sllf (1, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.1llr) abort (); if (vararg_Sllf (2, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.2llr) abort (); if (vararg_Sllf (3, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.3llr) abort (); if (vararg_Sllf (4, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.4llr) abort (); if (vararg_Sllf (5, 0.0llr, 0.1llr, 0.2llr, 0.3llr, 0.4llr, 0.5llr) != 0.5llr) abort ();
  if (vararg_Susf (0, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.0uhr) abort (); if (vararg_Susf (1, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.1uhr) abort (); if (vararg_Susf (2, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.2uhr) abort (); if (vararg_Susf (3, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.3uhr) abort (); if (vararg_Susf (4, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.4uhr) abort (); if (vararg_Susf (5, 0.0uhr, 0.1uhr, 0.2uhr, 0.3uhr, 0.4uhr, 0.5uhr) != 0.5uhr) abort ();
  if (vararg_Suf (0, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.0ur) abort (); if (vararg_Suf (1, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.1ur) abort (); if (vararg_Suf (2, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.2ur) abort (); if (vararg_Suf (3, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.3ur) abort (); if (vararg_Suf (4, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.4ur) abort (); if (vararg_Suf (5, 0.0ur, 0.1ur, 0.2ur, 0.3ur, 0.4ur, 0.5ur) != 0.5ur) abort ();
  if (vararg_Sulf (0, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.0ulr) abort (); if (vararg_Sulf (1, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.1ulr) abort (); if (vararg_Sulf (2, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.2ulr) abort (); if (vararg_Sulf (3, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.3ulr) abort (); if (vararg_Sulf (4, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.4ulr) abort (); if (vararg_Sulf (5, 0.0ulr, 0.1ulr, 0.2ulr, 0.3ulr, 0.4ulr, 0.5ulr) != 0.5ulr) abort ();
  if (vararg_Sullf (0, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.0ullr) abort (); if (vararg_Sullf (1, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.1ullr) abort (); if (vararg_Sullf (2, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.2ullr) abort (); if (vararg_Sullf (3, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.3ullr) abort (); if (vararg_Sullf (4, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.4ullr) abort (); if (vararg_Sullf (5, 0.0ullr, 0.1ullr, 0.2ullr, 0.3ullr, 0.4ullr, 0.5ullr) != 0.5ullr) abort ();
  if (vararg_sa (0, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.0hk) abort (); if (vararg_sa (1, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.1hk) abort (); if (vararg_sa (2, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.2hk) abort (); if (vararg_sa (3, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.3hk) abort (); if (vararg_sa (4, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.4hk) abort (); if (vararg_sa (5, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.5hk) abort ();
  if (vararg_a (0, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.0k) abort (); if (vararg_a (1, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.1k) abort (); if (vararg_a (2, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.2k) abort (); if (vararg_a (3, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.3k) abort (); if (vararg_a (4, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.4k) abort (); if (vararg_a (5, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.5k) abort ();
  if (vararg_la (0, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.0lk) abort (); if (vararg_la (1, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.1lk) abort (); if (vararg_la (2, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.2lk) abort (); if (vararg_la (3, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.3lk) abort (); if (vararg_la (4, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.4lk) abort (); if (vararg_la (5, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.5lk) abort ();
  if (vararg_lla (0, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.0llk) abort (); if (vararg_lla (1, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.1llk) abort (); if (vararg_lla (2, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.2llk) abort (); if (vararg_lla (3, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.3llk) abort (); if (vararg_lla (4, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.4llk) abort (); if (vararg_lla (5, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.5llk) abort ();
  if (vararg_usa (0, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.0uhk) abort (); if (vararg_usa (1, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.1uhk) abort (); if (vararg_usa (2, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.2uhk) abort (); if (vararg_usa (3, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.3uhk) abort (); if (vararg_usa (4, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.4uhk) abort (); if (vararg_usa (5, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.5uhk) abort ();
  if (vararg_ua (0, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.0uk) abort (); if (vararg_ua (1, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.1uk) abort (); if (vararg_ua (2, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.2uk) abort (); if (vararg_ua (3, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.3uk) abort (); if (vararg_ua (4, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.4uk) abort (); if (vararg_ua (5, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.5uk) abort ();
  if (vararg_ula (0, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.0ulk) abort (); if (vararg_ula (1, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.1ulk) abort (); if (vararg_ula (2, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.2ulk) abort (); if (vararg_ula (3, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.3ulk) abort (); if (vararg_ula (4, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.4ulk) abort (); if (vararg_ula (5, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.5ulk) abort ();
  if (vararg_ulla (0, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.0ullk) abort (); if (vararg_ulla (1, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.1ullk) abort (); if (vararg_ulla (2, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.2ullk) abort (); if (vararg_ulla (3, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.3ullk) abort (); if (vararg_ulla (4, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.4ullk) abort (); if (vararg_ulla (5, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.5ullk) abort ();
  if (vararg_Ssa (0, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.0hk) abort (); if (vararg_Ssa (1, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.1hk) abort (); if (vararg_Ssa (2, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.2hk) abort (); if (vararg_Ssa (3, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.3hk) abort (); if (vararg_Ssa (4, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.4hk) abort (); if (vararg_Ssa (5, 0.0hk, 0.1hk, 0.2hk, 0.3hk, 0.4hk, 0.5hk) != 0.5hk) abort ();
  if (vararg_Sa (0, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.0k) abort (); if (vararg_Sa (1, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.1k) abort (); if (vararg_Sa (2, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.2k) abort (); if (vararg_Sa (3, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.3k) abort (); if (vararg_Sa (4, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.4k) abort (); if (vararg_Sa (5, 0.0k, 0.1k, 0.2k, 0.3k, 0.4k, 0.5k) != 0.5k) abort ();
  if (vararg_Sla (0, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.0lk) abort (); if (vararg_Sla (1, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.1lk) abort (); if (vararg_Sla (2, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.2lk) abort (); if (vararg_Sla (3, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.3lk) abort (); if (vararg_Sla (4, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.4lk) abort (); if (vararg_Sla (5, 0.0lk, 0.1lk, 0.2lk, 0.3lk, 0.4lk, 0.5lk) != 0.5lk) abort ();
  if (vararg_Slla (0, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.0llk) abort (); if (vararg_Slla (1, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.1llk) abort (); if (vararg_Slla (2, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.2llk) abort (); if (vararg_Slla (3, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.3llk) abort (); if (vararg_Slla (4, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.4llk) abort (); if (vararg_Slla (5, 0.0llk, 0.1llk, 0.2llk, 0.3llk, 0.4llk, 0.5llk) != 0.5llk) abort ();
  if (vararg_Susa (0, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.0uhk) abort (); if (vararg_Susa (1, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.1uhk) abort (); if (vararg_Susa (2, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.2uhk) abort (); if (vararg_Susa (3, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.3uhk) abort (); if (vararg_Susa (4, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.4uhk) abort (); if (vararg_Susa (5, 0.0uhk, 0.1uhk, 0.2uhk, 0.3uhk, 0.4uhk, 0.5uhk) != 0.5uhk) abort ();
  if (vararg_Sua (0, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.0uk) abort (); if (vararg_Sua (1, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.1uk) abort (); if (vararg_Sua (2, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.2uk) abort (); if (vararg_Sua (3, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.3uk) abort (); if (vararg_Sua (4, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.4uk) abort (); if (vararg_Sua (5, 0.0uk, 0.1uk, 0.2uk, 0.3uk, 0.4uk, 0.5uk) != 0.5uk) abort ();
  if (vararg_Sula (0, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.0ulk) abort (); if (vararg_Sula (1, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.1ulk) abort (); if (vararg_Sula (2, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.2ulk) abort (); if (vararg_Sula (3, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.3ulk) abort (); if (vararg_Sula (4, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.4ulk) abort (); if (vararg_Sula (5, 0.0ulk, 0.1ulk, 0.2ulk, 0.3ulk, 0.4ulk, 0.5ulk) != 0.5ulk) abort ();
  if (vararg_Sulla (0, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.0ullk) abort (); if (vararg_Sulla (1, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.1ullk) abort (); if (vararg_Sulla (2, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.2ullk) abort (); if (vararg_Sulla (3, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.3ullk) abort (); if (vararg_Sulla (4, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.4ullk) abort (); if (vararg_Sulla (5, 0.0ullk, 0.1ullk, 0.2ullk, 0.3ullk, 0.4ullk, 0.5ullk) != 0.5ullk) abort ();

  return 0;
}
