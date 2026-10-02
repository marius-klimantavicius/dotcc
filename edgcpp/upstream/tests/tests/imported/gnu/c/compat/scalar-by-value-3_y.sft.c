//type: fp
//options: 
# 0 "./compat/scalar-by-value-3_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-by-value-3_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-by-value-3_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-by-value-3_y.c" 2




const int test_va = 1;
# 89 "./compat/scalar-by-value-3_y.c"
extern _Complex int g01ci, g02ci, g03ci, g04ci; extern _Complex int g05ci, g06ci, g07ci, g08ci; extern _Complex int g09ci, g10ci, g11ci, g12ci; extern _Complex int g13ci, g14ci, g15ci, g16ci; extern void checkci (_Complex int x, _Complex int v); void initci (_Complex int *p, _Complex int v) { *p = v + (2 + 3 * __extension__ 1i); } void checkgci (void) { checkci (g01ci, 1); checkci (g02ci, 2); checkci (g03ci, 3); checkci (g04ci, 4); checkci (g05ci, 5); checkci (g06ci, 6); checkci (g07ci, 7); checkci (g08ci, 8); checkci (g09ci, 9); checkci (g10ci, 10); checkci (g11ci, 11); checkci (g12ci, 12); checkci (g13ci, 13); checkci (g14ci, 14); checkci (g15ci, 15); checkci (g16ci, 16); } void testci (_Complex int x01, _Complex int x02, _Complex int x03, _Complex int x04, _Complex int x05, _Complex int x06, _Complex int x07, _Complex int x08, _Complex int x09, _Complex int x10, _Complex int x11, _Complex int x12, _Complex int x13, _Complex int x14, _Complex int x15, _Complex int x16) { checkci (x01, 1); checkci (x02, 2); checkci (x03, 3); checkci (x04, 4); checkci (x05, 5); checkci (x06, 6); checkci (x07, 7); checkci (x08, 8); checkci (x09, 9); checkci (x10, 10); checkci (x11, 11); checkci (x12, 12); checkci (x13, 13); checkci (x14, 14); checkci (x15, 15); checkci (x16, 16); } void testvaci (int n, ...) { int i; va_list ap; if (test_va) { 
# 89 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_c23_va_start(
# 89 "./compat/scalar-by-value-3_y.c"
ap, n
# 89 "./compat/scalar-by-value-3_y.c" 3 4
)
# 89 "./compat/scalar-by-value-3_y.c"
; for (i = 0; i < n; i++) { _Complex int t = 
# 89 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_arg(
# 89 "./compat/scalar-by-value-3_y.c"
ap
# 89 "./compat/scalar-by-value-3_y.c" 3 4
,
# 89 "./compat/scalar-by-value-3_y.c"
_Complex int
# 89 "./compat/scalar-by-value-3_y.c" 3 4
)
# 89 "./compat/scalar-by-value-3_y.c"
; checkci (t, i+1); } 
# 89 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_end(
# 89 "./compat/scalar-by-value-3_y.c"
ap
# 89 "./compat/scalar-by-value-3_y.c" 3 4
)
# 89 "./compat/scalar-by-value-3_y.c"
; } }
extern _Complex long g01cl, g02cl, g03cl, g04cl; extern _Complex long g05cl, g06cl, g07cl, g08cl; extern _Complex long g09cl, g10cl, g11cl, g12cl; extern _Complex long g13cl, g14cl, g15cl, g16cl; extern void checkcl (_Complex long x, _Complex long v); void initcl (_Complex long *p, _Complex long v) { *p = v + (3 + 4 * __extension__ 1i); } void checkgcl (void) { checkcl (g01cl, 1); checkcl (g02cl, 2); checkcl (g03cl, 3); checkcl (g04cl, 4); checkcl (g05cl, 5); checkcl (g06cl, 6); checkcl (g07cl, 7); checkcl (g08cl, 8); checkcl (g09cl, 9); checkcl (g10cl, 10); checkcl (g11cl, 11); checkcl (g12cl, 12); checkcl (g13cl, 13); checkcl (g14cl, 14); checkcl (g15cl, 15); checkcl (g16cl, 16); } void testcl (_Complex long x01, _Complex long x02, _Complex long x03, _Complex long x04, _Complex long x05, _Complex long x06, _Complex long x07, _Complex long x08, _Complex long x09, _Complex long x10, _Complex long x11, _Complex long x12, _Complex long x13, _Complex long x14, _Complex long x15, _Complex long x16) { checkcl (x01, 1); checkcl (x02, 2); checkcl (x03, 3); checkcl (x04, 4); checkcl (x05, 5); checkcl (x06, 6); checkcl (x07, 7); checkcl (x08, 8); checkcl (x09, 9); checkcl (x10, 10); checkcl (x11, 11); checkcl (x12, 12); checkcl (x13, 13); checkcl (x14, 14); checkcl (x15, 15); checkcl (x16, 16); } void testvacl (int n, ...) { int i; va_list ap; if (test_va) { 
# 90 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_c23_va_start(
# 90 "./compat/scalar-by-value-3_y.c"
ap, n
# 90 "./compat/scalar-by-value-3_y.c" 3 4
)
# 90 "./compat/scalar-by-value-3_y.c"
; for (i = 0; i < n; i++) { _Complex long t = 
# 90 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_arg(
# 90 "./compat/scalar-by-value-3_y.c"
ap
# 90 "./compat/scalar-by-value-3_y.c" 3 4
,
# 90 "./compat/scalar-by-value-3_y.c"
_Complex long
# 90 "./compat/scalar-by-value-3_y.c" 3 4
)
# 90 "./compat/scalar-by-value-3_y.c"
; checkcl (t, i+1); } 
# 90 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_end(
# 90 "./compat/scalar-by-value-3_y.c"
ap
# 90 "./compat/scalar-by-value-3_y.c" 3 4
)
# 90 "./compat/scalar-by-value-3_y.c"
; } }
extern _Complex long long g01cll, g02cll, g03cll, g04cll; extern _Complex long long g05cll, g06cll, g07cll, g08cll; extern _Complex long long g09cll, g10cll, g11cll, g12cll; extern _Complex long long g13cll, g14cll, g15cll, g16cll; extern void checkcll (_Complex long long x, _Complex long long v); void initcll (_Complex long long *p, _Complex long long v) { *p = v + (5 + 6 * __extension__ 1i); } void checkgcll (void) { checkcll (g01cll, 1); checkcll (g02cll, 2); checkcll (g03cll, 3); checkcll (g04cll, 4); checkcll (g05cll, 5); checkcll (g06cll, 6); checkcll (g07cll, 7); checkcll (g08cll, 8); checkcll (g09cll, 9); checkcll (g10cll, 10); checkcll (g11cll, 11); checkcll (g12cll, 12); checkcll (g13cll, 13); checkcll (g14cll, 14); checkcll (g15cll, 15); checkcll (g16cll, 16); } void testcll (_Complex long long x01, _Complex long long x02, _Complex long long x03, _Complex long long x04, _Complex long long x05, _Complex long long x06, _Complex long long x07, _Complex long long x08, _Complex long long x09, _Complex long long x10, _Complex long long x11, _Complex long long x12, _Complex long long x13, _Complex long long x14, _Complex long long x15, _Complex long long x16) { checkcll (x01, 1); checkcll (x02, 2); checkcll (x03, 3); checkcll (x04, 4); checkcll (x05, 5); checkcll (x06, 6); checkcll (x07, 7); checkcll (x08, 8); checkcll (x09, 9); checkcll (x10, 10); checkcll (x11, 11); checkcll (x12, 12); checkcll (x13, 13); checkcll (x14, 14); checkcll (x15, 15); checkcll (x16, 16); } void testvacll (int n, ...) { int i; va_list ap; if (test_va) { 
# 91 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_c23_va_start(
# 91 "./compat/scalar-by-value-3_y.c"
ap, n
# 91 "./compat/scalar-by-value-3_y.c" 3 4
)
# 91 "./compat/scalar-by-value-3_y.c"
; for (i = 0; i < n; i++) { _Complex long long t = 
# 91 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_arg(
# 91 "./compat/scalar-by-value-3_y.c"
ap
# 91 "./compat/scalar-by-value-3_y.c" 3 4
,
# 91 "./compat/scalar-by-value-3_y.c"
_Complex long long
# 91 "./compat/scalar-by-value-3_y.c" 3 4
)
# 91 "./compat/scalar-by-value-3_y.c"
; checkcll (t, i+1); } 
# 91 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_end(
# 91 "./compat/scalar-by-value-3_y.c"
ap
# 91 "./compat/scalar-by-value-3_y.c" 3 4
)
# 91 "./compat/scalar-by-value-3_y.c"
; } }

extern _Complex double g01cd, g02cd, g03cd, g04cd; extern _Complex double g05cd, g06cd, g07cd, g08cd; extern _Complex double g09cd, g10cd, g11cd, g12cd; extern _Complex double g13cd, g14cd, g15cd, g16cd; extern void checkcd (_Complex double x, _Complex double v); void initcd (_Complex double *p, _Complex double v) { *p = v + (7.0 + 8.0 * __extension__ 1.0i); } void checkgcd (void) { checkcd (g01cd, 1); checkcd (g02cd, 2); checkcd (g03cd, 3); checkcd (g04cd, 4); checkcd (g05cd, 5); checkcd (g06cd, 6); checkcd (g07cd, 7); checkcd (g08cd, 8); checkcd (g09cd, 9); checkcd (g10cd, 10); checkcd (g11cd, 11); checkcd (g12cd, 12); checkcd (g13cd, 13); checkcd (g14cd, 14); checkcd (g15cd, 15); checkcd (g16cd, 16); } void testcd (_Complex double x01, _Complex double x02, _Complex double x03, _Complex double x04, _Complex double x05, _Complex double x06, _Complex double x07, _Complex double x08, _Complex double x09, _Complex double x10, _Complex double x11, _Complex double x12, _Complex double x13, _Complex double x14, _Complex double x15, _Complex double x16) { checkcd (x01, 1); checkcd (x02, 2); checkcd (x03, 3); checkcd (x04, 4); checkcd (x05, 5); checkcd (x06, 6); checkcd (x07, 7); checkcd (x08, 8); checkcd (x09, 9); checkcd (x10, 10); checkcd (x11, 11); checkcd (x12, 12); checkcd (x13, 13); checkcd (x14, 14); checkcd (x15, 15); checkcd (x16, 16); } void testvacd (int n, ...) { int i; va_list ap; if (test_va) { 
# 93 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_c23_va_start(
# 93 "./compat/scalar-by-value-3_y.c"
ap, n
# 93 "./compat/scalar-by-value-3_y.c" 3 4
)
# 93 "./compat/scalar-by-value-3_y.c"
; for (i = 0; i < n; i++) { _Complex double t = 
# 93 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_arg(
# 93 "./compat/scalar-by-value-3_y.c"
ap
# 93 "./compat/scalar-by-value-3_y.c" 3 4
,
# 93 "./compat/scalar-by-value-3_y.c"
_Complex double
# 93 "./compat/scalar-by-value-3_y.c" 3 4
)
# 93 "./compat/scalar-by-value-3_y.c"
; checkcd (t, i+1); } 
# 93 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_end(
# 93 "./compat/scalar-by-value-3_y.c"
ap
# 93 "./compat/scalar-by-value-3_y.c" 3 4
)
# 93 "./compat/scalar-by-value-3_y.c"
; } }
extern _Complex long double g01cld, g02cld, g03cld, g04cld; extern _Complex long double g05cld, g06cld, g07cld, g08cld; extern _Complex long double g09cld, g10cld, g11cld, g12cld; extern _Complex long double g13cld, g14cld, g15cld, g16cld; extern void checkcld (_Complex long double x, _Complex long double v); void initcld (_Complex long double *p, _Complex long double v) { *p = v + (8.0 + 9.0 * __extension__ 1.0i); } void checkgcld (void) { checkcld (g01cld, 1); checkcld (g02cld, 2); checkcld (g03cld, 3); checkcld (g04cld, 4); checkcld (g05cld, 5); checkcld (g06cld, 6); checkcld (g07cld, 7); checkcld (g08cld, 8); checkcld (g09cld, 9); checkcld (g10cld, 10); checkcld (g11cld, 11); checkcld (g12cld, 12); checkcld (g13cld, 13); checkcld (g14cld, 14); checkcld (g15cld, 15); checkcld (g16cld, 16); } void testcld (_Complex long double x01, _Complex long double x02, _Complex long double x03, _Complex long double x04, _Complex long double x05, _Complex long double x06, _Complex long double x07, _Complex long double x08, _Complex long double x09, _Complex long double x10, _Complex long double x11, _Complex long double x12, _Complex long double x13, _Complex long double x14, _Complex long double x15, _Complex long double x16) { checkcld (x01, 1); checkcld (x02, 2); checkcld (x03, 3); checkcld (x04, 4); checkcld (x05, 5); checkcld (x06, 6); checkcld (x07, 7); checkcld (x08, 8); checkcld (x09, 9); checkcld (x10, 10); checkcld (x11, 11); checkcld (x12, 12); checkcld (x13, 13); checkcld (x14, 14); checkcld (x15, 15); checkcld (x16, 16); } void testvacld (int n, ...) { int i; va_list ap; if (test_va) { 
# 94 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_c23_va_start(
# 94 "./compat/scalar-by-value-3_y.c"
ap, n
# 94 "./compat/scalar-by-value-3_y.c" 3 4
)
# 94 "./compat/scalar-by-value-3_y.c"
; for (i = 0; i < n; i++) { _Complex long double t = 
# 94 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_arg(
# 94 "./compat/scalar-by-value-3_y.c"
ap
# 94 "./compat/scalar-by-value-3_y.c" 3 4
,
# 94 "./compat/scalar-by-value-3_y.c"
_Complex long double
# 94 "./compat/scalar-by-value-3_y.c" 3 4
)
# 94 "./compat/scalar-by-value-3_y.c"
; checkcld (t, i+1); } 
# 94 "./compat/scalar-by-value-3_y.c" 3 4
__builtin_va_end(
# 94 "./compat/scalar-by-value-3_y.c"
ap
# 94 "./compat/scalar-by-value-3_y.c" 3 4
)
# 94 "./compat/scalar-by-value-3_y.c"
; } }
