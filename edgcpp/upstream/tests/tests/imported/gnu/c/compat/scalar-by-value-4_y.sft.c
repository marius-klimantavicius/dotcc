//type: fp
//options: 
# 0 "./compat/scalar-by-value-4_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-by-value-4_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-by-value-4_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-by-value-4_y.c" 2




const int test_va = 1;
# 89 "./compat/scalar-by-value-4_y.c"
extern _Complex char g01cc, g02cc, g03cc, g04cc; extern _Complex char g05cc, g06cc, g07cc, g08cc; extern _Complex char g09cc, g10cc, g11cc, g12cc; extern _Complex char g13cc, g14cc, g15cc, g16cc; extern void checkcc (_Complex char x, _Complex char v); void initcc (_Complex char *p, _Complex char v) { *p = v + (0 + 1 * __extension__ 1i); } void checkgcc (void) { checkcc (g01cc, 1); checkcc (g02cc, 2); checkcc (g03cc, 3); checkcc (g04cc, 4); checkcc (g05cc, 5); checkcc (g06cc, 6); checkcc (g07cc, 7); checkcc (g08cc, 8); checkcc (g09cc, 9); checkcc (g10cc, 10); checkcc (g11cc, 11); checkcc (g12cc, 12); checkcc (g13cc, 13); checkcc (g14cc, 14); checkcc (g15cc, 15); checkcc (g16cc, 16); } void testcc (_Complex char x01, _Complex char x02, _Complex char x03, _Complex char x04, _Complex char x05, _Complex char x06, _Complex char x07, _Complex char x08, _Complex char x09, _Complex char x10, _Complex char x11, _Complex char x12, _Complex char x13, _Complex char x14, _Complex char x15, _Complex char x16) { checkcc (x01, 1); checkcc (x02, 2); checkcc (x03, 3); checkcc (x04, 4); checkcc (x05, 5); checkcc (x06, 6); checkcc (x07, 7); checkcc (x08, 8); checkcc (x09, 9); checkcc (x10, 10); checkcc (x11, 11); checkcc (x12, 12); checkcc (x13, 13); checkcc (x14, 14); checkcc (x15, 15); checkcc (x16, 16); } void testvacc (int n, ...) { int i; va_list ap; if (test_va) { 
# 89 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_c23_va_start(
# 89 "./compat/scalar-by-value-4_y.c"
ap, n
# 89 "./compat/scalar-by-value-4_y.c" 3 4
)
# 89 "./compat/scalar-by-value-4_y.c"
; for (i = 0; i < n; i++) { _Complex char t = 
# 89 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_arg(
# 89 "./compat/scalar-by-value-4_y.c"
ap
# 89 "./compat/scalar-by-value-4_y.c" 3 4
,
# 89 "./compat/scalar-by-value-4_y.c"
_Complex char
# 89 "./compat/scalar-by-value-4_y.c" 3 4
)
# 89 "./compat/scalar-by-value-4_y.c"
; checkcc (t, i+1); } 
# 89 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_end(
# 89 "./compat/scalar-by-value-4_y.c"
ap
# 89 "./compat/scalar-by-value-4_y.c" 3 4
)
# 89 "./compat/scalar-by-value-4_y.c"
; } }
extern _Complex short g01cs, g02cs, g03cs, g04cs; extern _Complex short g05cs, g06cs, g07cs, g08cs; extern _Complex short g09cs, g10cs, g11cs, g12cs; extern _Complex short g13cs, g14cs, g15cs, g16cs; extern void checkcs (_Complex short x, _Complex short v); void initcs (_Complex short *p, _Complex short v) { *p = v + (1 + 2 * __extension__ 1i); } void checkgcs (void) { checkcs (g01cs, 1); checkcs (g02cs, 2); checkcs (g03cs, 3); checkcs (g04cs, 4); checkcs (g05cs, 5); checkcs (g06cs, 6); checkcs (g07cs, 7); checkcs (g08cs, 8); checkcs (g09cs, 9); checkcs (g10cs, 10); checkcs (g11cs, 11); checkcs (g12cs, 12); checkcs (g13cs, 13); checkcs (g14cs, 14); checkcs (g15cs, 15); checkcs (g16cs, 16); } void testcs (_Complex short x01, _Complex short x02, _Complex short x03, _Complex short x04, _Complex short x05, _Complex short x06, _Complex short x07, _Complex short x08, _Complex short x09, _Complex short x10, _Complex short x11, _Complex short x12, _Complex short x13, _Complex short x14, _Complex short x15, _Complex short x16) { checkcs (x01, 1); checkcs (x02, 2); checkcs (x03, 3); checkcs (x04, 4); checkcs (x05, 5); checkcs (x06, 6); checkcs (x07, 7); checkcs (x08, 8); checkcs (x09, 9); checkcs (x10, 10); checkcs (x11, 11); checkcs (x12, 12); checkcs (x13, 13); checkcs (x14, 14); checkcs (x15, 15); checkcs (x16, 16); } void testvacs (int n, ...) { int i; va_list ap; if (test_va) { 
# 90 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_c23_va_start(
# 90 "./compat/scalar-by-value-4_y.c"
ap, n
# 90 "./compat/scalar-by-value-4_y.c" 3 4
)
# 90 "./compat/scalar-by-value-4_y.c"
; for (i = 0; i < n; i++) { _Complex short t = 
# 90 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_arg(
# 90 "./compat/scalar-by-value-4_y.c"
ap
# 90 "./compat/scalar-by-value-4_y.c" 3 4
,
# 90 "./compat/scalar-by-value-4_y.c"
_Complex short
# 90 "./compat/scalar-by-value-4_y.c" 3 4
)
# 90 "./compat/scalar-by-value-4_y.c"
; checkcs (t, i+1); } 
# 90 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_end(
# 90 "./compat/scalar-by-value-4_y.c"
ap
# 90 "./compat/scalar-by-value-4_y.c" 3 4
)
# 90 "./compat/scalar-by-value-4_y.c"
; } }

extern _Complex float g01cf, g02cf, g03cf, g04cf; extern _Complex float g05cf, g06cf, g07cf, g08cf; extern _Complex float g09cf, g10cf, g11cf, g12cf; extern _Complex float g13cf, g14cf, g15cf, g16cf; extern void checkcf (_Complex float x, _Complex float v); void initcf (_Complex float *p, _Complex float v) { *p = v + (6.0 + 7.0 * __extension__ 1.0i); } void checkgcf (void) { checkcf (g01cf, 1); checkcf (g02cf, 2); checkcf (g03cf, 3); checkcf (g04cf, 4); checkcf (g05cf, 5); checkcf (g06cf, 6); checkcf (g07cf, 7); checkcf (g08cf, 8); checkcf (g09cf, 9); checkcf (g10cf, 10); checkcf (g11cf, 11); checkcf (g12cf, 12); checkcf (g13cf, 13); checkcf (g14cf, 14); checkcf (g15cf, 15); checkcf (g16cf, 16); } void testcf (_Complex float x01, _Complex float x02, _Complex float x03, _Complex float x04, _Complex float x05, _Complex float x06, _Complex float x07, _Complex float x08, _Complex float x09, _Complex float x10, _Complex float x11, _Complex float x12, _Complex float x13, _Complex float x14, _Complex float x15, _Complex float x16) { checkcf (x01, 1); checkcf (x02, 2); checkcf (x03, 3); checkcf (x04, 4); checkcf (x05, 5); checkcf (x06, 6); checkcf (x07, 7); checkcf (x08, 8); checkcf (x09, 9); checkcf (x10, 10); checkcf (x11, 11); checkcf (x12, 12); checkcf (x13, 13); checkcf (x14, 14); checkcf (x15, 15); checkcf (x16, 16); } void testvacf (int n, ...) { int i; va_list ap; if (test_va) { 
# 92 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_c23_va_start(
# 92 "./compat/scalar-by-value-4_y.c"
ap, n
# 92 "./compat/scalar-by-value-4_y.c" 3 4
)
# 92 "./compat/scalar-by-value-4_y.c"
; for (i = 0; i < n; i++) { _Complex float t = 
# 92 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_arg(
# 92 "./compat/scalar-by-value-4_y.c"
ap
# 92 "./compat/scalar-by-value-4_y.c" 3 4
,
# 92 "./compat/scalar-by-value-4_y.c"
_Complex float
# 92 "./compat/scalar-by-value-4_y.c" 3 4
)
# 92 "./compat/scalar-by-value-4_y.c"
; checkcf (t, i+1); } 
# 92 "./compat/scalar-by-value-4_y.c" 3 4
__builtin_va_end(
# 92 "./compat/scalar-by-value-4_y.c"
ap
# 92 "./compat/scalar-by-value-4_y.c" 3 4
)
# 92 "./compat/scalar-by-value-4_y.c"
; } }
