//type: fp
//options: 
# 0 "./compat/scalar-return-4_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-return-4_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-return-4_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-return-4_y.c" 2
# 67 "./compat/scalar-return-4_y.c"
extern _Complex char g01cc, g02cc, g03cc, g04cc; extern _Complex char g05cc, g06cc, g07cc, g08cc; extern _Complex char g09cc, g10cc, g11cc, g12cc; extern _Complex char g13cc, g14cc, g15cc, g16cc; extern void checkcc (_Complex char x, _Complex char v); void initcc (_Complex char *p, _Complex char v) { *p = v + (0 + 1 * __extension__ 1i); } void checkgcc (void) { checkcc (g01cc, 1+(0 + 1 * __extension__ 1i)); checkcc (g02cc, 2+(0 + 1 * __extension__ 1i)); checkcc (g03cc, 3+(0 + 1 * __extension__ 1i)); checkcc (g04cc, 4+(0 + 1 * __extension__ 1i)); checkcc (g05cc, 5+(0 + 1 * __extension__ 1i)); checkcc (g06cc, 6+(0 + 1 * __extension__ 1i)); checkcc (g07cc, 7+(0 + 1 * __extension__ 1i)); checkcc (g08cc, 8+(0 + 1 * __extension__ 1i)); checkcc (g09cc, 9+(0 + 1 * __extension__ 1i)); checkcc (g10cc, 10+(0 + 1 * __extension__ 1i)); checkcc (g11cc, 11+(0 + 1 * __extension__ 1i)); checkcc (g12cc, 12+(0 + 1 * __extension__ 1i)); checkcc (g13cc, 13+(0 + 1 * __extension__ 1i)); checkcc (g14cc, 14+(0 + 1 * __extension__ 1i)); checkcc (g15cc, 15+(0 + 1 * __extension__ 1i)); checkcc (g16cc, 16+(0 + 1 * __extension__ 1i)); } _Complex char test0cc (void) { return g01cc; } _Complex char test1cc (_Complex char x01) { return x01; } _Complex char testvacc (int n, ...) { int i; _Complex char rslt; va_list ap; 
# 67 "./compat/scalar-return-4_y.c" 3 4
__builtin_c23_va_start(
# 67 "./compat/scalar-return-4_y.c"
ap, n
# 67 "./compat/scalar-return-4_y.c" 3 4
)
# 67 "./compat/scalar-return-4_y.c"
; for (i = 0; i < n; i++) rslt = 
# 67 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_arg(
# 67 "./compat/scalar-return-4_y.c"
ap
# 67 "./compat/scalar-return-4_y.c" 3 4
,
# 67 "./compat/scalar-return-4_y.c"
_Complex char
# 67 "./compat/scalar-return-4_y.c" 3 4
)
# 67 "./compat/scalar-return-4_y.c"
; 
# 67 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_end(
# 67 "./compat/scalar-return-4_y.c"
ap
# 67 "./compat/scalar-return-4_y.c" 3 4
)
# 67 "./compat/scalar-return-4_y.c"
; return rslt; }
extern _Complex short g01cs, g02cs, g03cs, g04cs; extern _Complex short g05cs, g06cs, g07cs, g08cs; extern _Complex short g09cs, g10cs, g11cs, g12cs; extern _Complex short g13cs, g14cs, g15cs, g16cs; extern void checkcs (_Complex short x, _Complex short v); void initcs (_Complex short *p, _Complex short v) { *p = v + (1 + 2 * __extension__ 1i); } void checkgcs (void) { checkcs (g01cs, 1+(1 + 2 * __extension__ 1i)); checkcs (g02cs, 2+(1 + 2 * __extension__ 1i)); checkcs (g03cs, 3+(1 + 2 * __extension__ 1i)); checkcs (g04cs, 4+(1 + 2 * __extension__ 1i)); checkcs (g05cs, 5+(1 + 2 * __extension__ 1i)); checkcs (g06cs, 6+(1 + 2 * __extension__ 1i)); checkcs (g07cs, 7+(1 + 2 * __extension__ 1i)); checkcs (g08cs, 8+(1 + 2 * __extension__ 1i)); checkcs (g09cs, 9+(1 + 2 * __extension__ 1i)); checkcs (g10cs, 10+(1 + 2 * __extension__ 1i)); checkcs (g11cs, 11+(1 + 2 * __extension__ 1i)); checkcs (g12cs, 12+(1 + 2 * __extension__ 1i)); checkcs (g13cs, 13+(1 + 2 * __extension__ 1i)); checkcs (g14cs, 14+(1 + 2 * __extension__ 1i)); checkcs (g15cs, 15+(1 + 2 * __extension__ 1i)); checkcs (g16cs, 16+(1 + 2 * __extension__ 1i)); } _Complex short test0cs (void) { return g01cs; } _Complex short test1cs (_Complex short x01) { return x01; } _Complex short testvacs (int n, ...) { int i; _Complex short rslt; va_list ap; 
# 68 "./compat/scalar-return-4_y.c" 3 4
__builtin_c23_va_start(
# 68 "./compat/scalar-return-4_y.c"
ap, n
# 68 "./compat/scalar-return-4_y.c" 3 4
)
# 68 "./compat/scalar-return-4_y.c"
; for (i = 0; i < n; i++) rslt = 
# 68 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_arg(
# 68 "./compat/scalar-return-4_y.c"
ap
# 68 "./compat/scalar-return-4_y.c" 3 4
,
# 68 "./compat/scalar-return-4_y.c"
_Complex short
# 68 "./compat/scalar-return-4_y.c" 3 4
)
# 68 "./compat/scalar-return-4_y.c"
; 
# 68 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_end(
# 68 "./compat/scalar-return-4_y.c"
ap
# 68 "./compat/scalar-return-4_y.c" 3 4
)
# 68 "./compat/scalar-return-4_y.c"
; return rslt; }

extern _Complex float g01cf, g02cf, g03cf, g04cf; extern _Complex float g05cf, g06cf, g07cf, g08cf; extern _Complex float g09cf, g10cf, g11cf, g12cf; extern _Complex float g13cf, g14cf, g15cf, g16cf; extern void checkcf (_Complex float x, _Complex float v); void initcf (_Complex float *p, _Complex float v) { *p = v + (1.0 + 2.0 * __extension__ 1.0i); } void checkgcf (void) { checkcf (g01cf, 1+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g02cf, 2+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g03cf, 3+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g04cf, 4+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g05cf, 5+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g06cf, 6+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g07cf, 7+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g08cf, 8+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g09cf, 9+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g10cf, 10+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g11cf, 11+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g12cf, 12+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g13cf, 13+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g14cf, 14+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g15cf, 15+(1.0 + 2.0 * __extension__ 1.0i)); checkcf (g16cf, 16+(1.0 + 2.0 * __extension__ 1.0i)); } _Complex float test0cf (void) { return g01cf; } _Complex float test1cf (_Complex float x01) { return x01; } _Complex float testvacf (int n, ...) { int i; _Complex float rslt; va_list ap; 
# 70 "./compat/scalar-return-4_y.c" 3 4
__builtin_c23_va_start(
# 70 "./compat/scalar-return-4_y.c"
ap, n
# 70 "./compat/scalar-return-4_y.c" 3 4
)
# 70 "./compat/scalar-return-4_y.c"
; for (i = 0; i < n; i++) rslt = 
# 70 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_arg(
# 70 "./compat/scalar-return-4_y.c"
ap
# 70 "./compat/scalar-return-4_y.c" 3 4
,
# 70 "./compat/scalar-return-4_y.c"
_Complex float
# 70 "./compat/scalar-return-4_y.c" 3 4
)
# 70 "./compat/scalar-return-4_y.c"
; 
# 70 "./compat/scalar-return-4_y.c" 3 4
__builtin_va_end(
# 70 "./compat/scalar-return-4_y.c"
ap
# 70 "./compat/scalar-return-4_y.c" 3 4
)
# 70 "./compat/scalar-return-4_y.c"
; return rslt; }
