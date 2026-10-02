//type: fp
//options: 
# 0 "./compat/scalar-return-3_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-return-3_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-return-3_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-return-3_y.c" 2
# 67 "./compat/scalar-return-3_y.c"
extern _Complex int g01ci, g02ci, g03ci, g04ci; extern _Complex int g05ci, g06ci, g07ci, g08ci; extern _Complex int g09ci, g10ci, g11ci, g12ci; extern _Complex int g13ci, g14ci, g15ci, g16ci; extern void checkci (_Complex int x, _Complex int v); void initci (_Complex int *p, _Complex int v) { *p = v + (2 + 3 * __extension__ 1i); } void checkgci (void) { checkci (g01ci, 1+(2 + 3 * __extension__ 1i)); checkci (g02ci, 2+(2 + 3 * __extension__ 1i)); checkci (g03ci, 3+(2 + 3 * __extension__ 1i)); checkci (g04ci, 4+(2 + 3 * __extension__ 1i)); checkci (g05ci, 5+(2 + 3 * __extension__ 1i)); checkci (g06ci, 6+(2 + 3 * __extension__ 1i)); checkci (g07ci, 7+(2 + 3 * __extension__ 1i)); checkci (g08ci, 8+(2 + 3 * __extension__ 1i)); checkci (g09ci, 9+(2 + 3 * __extension__ 1i)); checkci (g10ci, 10+(2 + 3 * __extension__ 1i)); checkci (g11ci, 11+(2 + 3 * __extension__ 1i)); checkci (g12ci, 12+(2 + 3 * __extension__ 1i)); checkci (g13ci, 13+(2 + 3 * __extension__ 1i)); checkci (g14ci, 14+(2 + 3 * __extension__ 1i)); checkci (g15ci, 15+(2 + 3 * __extension__ 1i)); checkci (g16ci, 16+(2 + 3 * __extension__ 1i)); } _Complex int test0ci (void) { return g01ci; } _Complex int test1ci (_Complex int x01) { return x01; } _Complex int testvaci (int n, ...) { int i; _Complex int rslt; va_list ap; 
# 67 "./compat/scalar-return-3_y.c" 3 4
__builtin_c23_va_start(
# 67 "./compat/scalar-return-3_y.c"
ap, n
# 67 "./compat/scalar-return-3_y.c" 3 4
)
# 67 "./compat/scalar-return-3_y.c"
; for (i = 0; i < n; i++) rslt = 
# 67 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_arg(
# 67 "./compat/scalar-return-3_y.c"
ap
# 67 "./compat/scalar-return-3_y.c" 3 4
,
# 67 "./compat/scalar-return-3_y.c"
_Complex int
# 67 "./compat/scalar-return-3_y.c" 3 4
)
# 67 "./compat/scalar-return-3_y.c"
; 
# 67 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_end(
# 67 "./compat/scalar-return-3_y.c"
ap
# 67 "./compat/scalar-return-3_y.c" 3 4
)
# 67 "./compat/scalar-return-3_y.c"
; return rslt; }
extern _Complex long g01cl, g02cl, g03cl, g04cl; extern _Complex long g05cl, g06cl, g07cl, g08cl; extern _Complex long g09cl, g10cl, g11cl, g12cl; extern _Complex long g13cl, g14cl, g15cl, g16cl; extern void checkcl (_Complex long x, _Complex long v); void initcl (_Complex long *p, _Complex long v) { *p = v + (3 + 4 * __extension__ 1i); } void checkgcl (void) { checkcl (g01cl, 1+(3 + 4 * __extension__ 1i)); checkcl (g02cl, 2+(3 + 4 * __extension__ 1i)); checkcl (g03cl, 3+(3 + 4 * __extension__ 1i)); checkcl (g04cl, 4+(3 + 4 * __extension__ 1i)); checkcl (g05cl, 5+(3 + 4 * __extension__ 1i)); checkcl (g06cl, 6+(3 + 4 * __extension__ 1i)); checkcl (g07cl, 7+(3 + 4 * __extension__ 1i)); checkcl (g08cl, 8+(3 + 4 * __extension__ 1i)); checkcl (g09cl, 9+(3 + 4 * __extension__ 1i)); checkcl (g10cl, 10+(3 + 4 * __extension__ 1i)); checkcl (g11cl, 11+(3 + 4 * __extension__ 1i)); checkcl (g12cl, 12+(3 + 4 * __extension__ 1i)); checkcl (g13cl, 13+(3 + 4 * __extension__ 1i)); checkcl (g14cl, 14+(3 + 4 * __extension__ 1i)); checkcl (g15cl, 15+(3 + 4 * __extension__ 1i)); checkcl (g16cl, 16+(3 + 4 * __extension__ 1i)); } _Complex long test0cl (void) { return g01cl; } _Complex long test1cl (_Complex long x01) { return x01; } _Complex long testvacl (int n, ...) { int i; _Complex long rslt; va_list ap; 
# 68 "./compat/scalar-return-3_y.c" 3 4
__builtin_c23_va_start(
# 68 "./compat/scalar-return-3_y.c"
ap, n
# 68 "./compat/scalar-return-3_y.c" 3 4
)
# 68 "./compat/scalar-return-3_y.c"
; for (i = 0; i < n; i++) rslt = 
# 68 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_arg(
# 68 "./compat/scalar-return-3_y.c"
ap
# 68 "./compat/scalar-return-3_y.c" 3 4
,
# 68 "./compat/scalar-return-3_y.c"
_Complex long
# 68 "./compat/scalar-return-3_y.c" 3 4
)
# 68 "./compat/scalar-return-3_y.c"
; 
# 68 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_end(
# 68 "./compat/scalar-return-3_y.c"
ap
# 68 "./compat/scalar-return-3_y.c" 3 4
)
# 68 "./compat/scalar-return-3_y.c"
; return rslt; }
extern _Complex long long g01cll, g02cll, g03cll, g04cll; extern _Complex long long g05cll, g06cll, g07cll, g08cll; extern _Complex long long g09cll, g10cll, g11cll, g12cll; extern _Complex long long g13cll, g14cll, g15cll, g16cll; extern void checkcll (_Complex long long x, _Complex long long v); void initcll (_Complex long long *p, _Complex long long v) { *p = v + (4 + 5 * __extension__ 1i); } void checkgcll (void) { checkcll (g01cll, 1+(4 + 5 * __extension__ 1i)); checkcll (g02cll, 2+(4 + 5 * __extension__ 1i)); checkcll (g03cll, 3+(4 + 5 * __extension__ 1i)); checkcll (g04cll, 4+(4 + 5 * __extension__ 1i)); checkcll (g05cll, 5+(4 + 5 * __extension__ 1i)); checkcll (g06cll, 6+(4 + 5 * __extension__ 1i)); checkcll (g07cll, 7+(4 + 5 * __extension__ 1i)); checkcll (g08cll, 8+(4 + 5 * __extension__ 1i)); checkcll (g09cll, 9+(4 + 5 * __extension__ 1i)); checkcll (g10cll, 10+(4 + 5 * __extension__ 1i)); checkcll (g11cll, 11+(4 + 5 * __extension__ 1i)); checkcll (g12cll, 12+(4 + 5 * __extension__ 1i)); checkcll (g13cll, 13+(4 + 5 * __extension__ 1i)); checkcll (g14cll, 14+(4 + 5 * __extension__ 1i)); checkcll (g15cll, 15+(4 + 5 * __extension__ 1i)); checkcll (g16cll, 16+(4 + 5 * __extension__ 1i)); } _Complex long long test0cll (void) { return g01cll; } _Complex long long test1cll (_Complex long long x01) { return x01; } _Complex long long testvacll (int n, ...) { int i; _Complex long long rslt; va_list ap; 
# 69 "./compat/scalar-return-3_y.c" 3 4
__builtin_c23_va_start(
# 69 "./compat/scalar-return-3_y.c"
ap, n
# 69 "./compat/scalar-return-3_y.c" 3 4
)
# 69 "./compat/scalar-return-3_y.c"
; for (i = 0; i < n; i++) rslt = 
# 69 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_arg(
# 69 "./compat/scalar-return-3_y.c"
ap
# 69 "./compat/scalar-return-3_y.c" 3 4
,
# 69 "./compat/scalar-return-3_y.c"
_Complex long long
# 69 "./compat/scalar-return-3_y.c" 3 4
)
# 69 "./compat/scalar-return-3_y.c"
; 
# 69 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_end(
# 69 "./compat/scalar-return-3_y.c"
ap
# 69 "./compat/scalar-return-3_y.c" 3 4
)
# 69 "./compat/scalar-return-3_y.c"
; return rslt; }

extern _Complex double g01cd, g02cd, g03cd, g04cd; extern _Complex double g05cd, g06cd, g07cd, g08cd; extern _Complex double g09cd, g10cd, g11cd, g12cd; extern _Complex double g13cd, g14cd, g15cd, g16cd; extern void checkcd (_Complex double x, _Complex double v); void initcd (_Complex double *p, _Complex double v) { *p = v + (2.0 + 3.0 * __extension__ 1.0i); } void checkgcd (void) { checkcd (g01cd, 1+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g02cd, 2+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g03cd, 3+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g04cd, 4+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g05cd, 5+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g06cd, 6+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g07cd, 7+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g08cd, 8+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g09cd, 9+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g10cd, 10+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g11cd, 11+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g12cd, 12+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g13cd, 13+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g14cd, 14+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g15cd, 15+(2.0 + 3.0 * __extension__ 1.0i)); checkcd (g16cd, 16+(2.0 + 3.0 * __extension__ 1.0i)); } _Complex double test0cd (void) { return g01cd; } _Complex double test1cd (_Complex double x01) { return x01; } _Complex double testvacd (int n, ...) { int i; _Complex double rslt; va_list ap; 
# 71 "./compat/scalar-return-3_y.c" 3 4
__builtin_c23_va_start(
# 71 "./compat/scalar-return-3_y.c"
ap, n
# 71 "./compat/scalar-return-3_y.c" 3 4
)
# 71 "./compat/scalar-return-3_y.c"
; for (i = 0; i < n; i++) rslt = 
# 71 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_arg(
# 71 "./compat/scalar-return-3_y.c"
ap
# 71 "./compat/scalar-return-3_y.c" 3 4
,
# 71 "./compat/scalar-return-3_y.c"
_Complex double
# 71 "./compat/scalar-return-3_y.c" 3 4
)
# 71 "./compat/scalar-return-3_y.c"
; 
# 71 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_end(
# 71 "./compat/scalar-return-3_y.c"
ap
# 71 "./compat/scalar-return-3_y.c" 3 4
)
# 71 "./compat/scalar-return-3_y.c"
; return rslt; }
extern _Complex long double g01cld, g02cld, g03cld, g04cld; extern _Complex long double g05cld, g06cld, g07cld, g08cld; extern _Complex long double g09cld, g10cld, g11cld, g12cld; extern _Complex long double g13cld, g14cld, g15cld, g16cld; extern void checkcld (_Complex long double x, _Complex long double v); void initcld (_Complex long double *p, _Complex long double v) { *p = v + (3.0 + 4.0 * __extension__ 1.0i); } void checkgcld (void) { checkcld (g01cld, 1+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g02cld, 2+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g03cld, 3+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g04cld, 4+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g05cld, 5+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g06cld, 6+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g07cld, 7+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g08cld, 8+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g09cld, 9+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g10cld, 10+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g11cld, 11+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g12cld, 12+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g13cld, 13+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g14cld, 14+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g15cld, 15+(3.0 + 4.0 * __extension__ 1.0i)); checkcld (g16cld, 16+(3.0 + 4.0 * __extension__ 1.0i)); } _Complex long double test0cld (void) { return g01cld; } _Complex long double test1cld (_Complex long double x01) { return x01; } _Complex long double testvacld (int n, ...) { int i; _Complex long double rslt; va_list ap; 
# 72 "./compat/scalar-return-3_y.c" 3 4
__builtin_c23_va_start(
# 72 "./compat/scalar-return-3_y.c"
ap, n
# 72 "./compat/scalar-return-3_y.c" 3 4
)
# 72 "./compat/scalar-return-3_y.c"
; for (i = 0; i < n; i++) rslt = 
# 72 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_arg(
# 72 "./compat/scalar-return-3_y.c"
ap
# 72 "./compat/scalar-return-3_y.c" 3 4
,
# 72 "./compat/scalar-return-3_y.c"
_Complex long double
# 72 "./compat/scalar-return-3_y.c" 3 4
)
# 72 "./compat/scalar-return-3_y.c"
; 
# 72 "./compat/scalar-return-3_y.c" 3 4
__builtin_va_end(
# 72 "./compat/scalar-return-3_y.c"
ap
# 72 "./compat/scalar-return-3_y.c" 3 4
)
# 72 "./compat/scalar-return-3_y.c"
; return rslt; }
