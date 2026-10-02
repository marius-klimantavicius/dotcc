//type: fp
//options: 
# 0 "./compat/scalar-by-value-dfp_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-by-value-dfp_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-by-value-dfp_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-by-value-dfp_y.c" 2




const int test_va = 1;
# 87 "./compat/scalar-by-value-dfp_y.c"
extern _Decimal32 g01d32, g02d32, g03d32, g04d32; extern _Decimal32 g05d32, g06d32, g07d32, g08d32; extern _Decimal32 g09d32, g10d32, g11d32, g12d32; extern _Decimal32 g13d32, g14d32, g15d32, g16d32; extern void checkd32 (_Decimal32 x, _Decimal32 v); void initd32 (_Decimal32 *p, _Decimal32 v) { *p = v + 1.2df; } void checkgd32 (void) { checkd32 (g01d32, 1); checkd32 (g02d32, 2); checkd32 (g03d32, 3); checkd32 (g04d32, 4); checkd32 (g05d32, 5); checkd32 (g06d32, 6); checkd32 (g07d32, 7); checkd32 (g08d32, 8); checkd32 (g09d32, 9); checkd32 (g10d32, 10); checkd32 (g11d32, 11); checkd32 (g12d32, 12); checkd32 (g13d32, 13); checkd32 (g14d32, 14); checkd32 (g15d32, 15); checkd32 (g16d32, 16); } void testd32 (_Decimal32 x01, _Decimal32 x02, _Decimal32 x03, _Decimal32 x04, _Decimal32 x05, _Decimal32 x06, _Decimal32 x07, _Decimal32 x08, _Decimal32 x09, _Decimal32 x10, _Decimal32 x11, _Decimal32 x12, _Decimal32 x13, _Decimal32 x14, _Decimal32 x15, _Decimal32 x16) { checkd32 (x01, 1); checkd32 (x02, 2); checkd32 (x03, 3); checkd32 (x04, 4); checkd32 (x05, 5); checkd32 (x06, 6); checkd32 (x07, 7); checkd32 (x08, 8); checkd32 (x09, 9); checkd32 (x10, 10); checkd32 (x11, 11); checkd32 (x12, 12); checkd32 (x13, 13); checkd32 (x14, 14); checkd32 (x15, 15); checkd32 (x16, 16); } void testvad32 (int n, ...) { int i; va_list ap; if (test_va) { 
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_c23_va_start(
# 87 "./compat/scalar-by-value-dfp_y.c"
ap, n
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 87 "./compat/scalar-by-value-dfp_y.c"
; for (i = 0; i < n; i++) { _Decimal32 t = 
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_arg(
# 87 "./compat/scalar-by-value-dfp_y.c"
ap
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
,
# 87 "./compat/scalar-by-value-dfp_y.c"
_Decimal32
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 87 "./compat/scalar-by-value-dfp_y.c"
; checkd32 (t, i+1); } 
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_end(
# 87 "./compat/scalar-by-value-dfp_y.c"
ap
# 87 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 87 "./compat/scalar-by-value-dfp_y.c"
; } }
extern _Decimal64 g01d64, g02d64, g03d64, g04d64; extern _Decimal64 g05d64, g06d64, g07d64, g08d64; extern _Decimal64 g09d64, g10d64, g11d64, g12d64; extern _Decimal64 g13d64, g14d64, g15d64, g16d64; extern void checkd64 (_Decimal64 x, _Decimal64 v); void initd64 (_Decimal64 *p, _Decimal64 v) { *p = v + 12.34dd; } void checkgd64 (void) { checkd64 (g01d64, 1); checkd64 (g02d64, 2); checkd64 (g03d64, 3); checkd64 (g04d64, 4); checkd64 (g05d64, 5); checkd64 (g06d64, 6); checkd64 (g07d64, 7); checkd64 (g08d64, 8); checkd64 (g09d64, 9); checkd64 (g10d64, 10); checkd64 (g11d64, 11); checkd64 (g12d64, 12); checkd64 (g13d64, 13); checkd64 (g14d64, 14); checkd64 (g15d64, 15); checkd64 (g16d64, 16); } void testd64 (_Decimal64 x01, _Decimal64 x02, _Decimal64 x03, _Decimal64 x04, _Decimal64 x05, _Decimal64 x06, _Decimal64 x07, _Decimal64 x08, _Decimal64 x09, _Decimal64 x10, _Decimal64 x11, _Decimal64 x12, _Decimal64 x13, _Decimal64 x14, _Decimal64 x15, _Decimal64 x16) { checkd64 (x01, 1); checkd64 (x02, 2); checkd64 (x03, 3); checkd64 (x04, 4); checkd64 (x05, 5); checkd64 (x06, 6); checkd64 (x07, 7); checkd64 (x08, 8); checkd64 (x09, 9); checkd64 (x10, 10); checkd64 (x11, 11); checkd64 (x12, 12); checkd64 (x13, 13); checkd64 (x14, 14); checkd64 (x15, 15); checkd64 (x16, 16); } void testvad64 (int n, ...) { int i; va_list ap; if (test_va) { 
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_c23_va_start(
# 88 "./compat/scalar-by-value-dfp_y.c"
ap, n
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 88 "./compat/scalar-by-value-dfp_y.c"
; for (i = 0; i < n; i++) { _Decimal64 t = 
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_arg(
# 88 "./compat/scalar-by-value-dfp_y.c"
ap
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
,
# 88 "./compat/scalar-by-value-dfp_y.c"
_Decimal64
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 88 "./compat/scalar-by-value-dfp_y.c"
; checkd64 (t, i+1); } 
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_end(
# 88 "./compat/scalar-by-value-dfp_y.c"
ap
# 88 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 88 "./compat/scalar-by-value-dfp_y.c"
; } }
extern _Decimal128 g01d128, g02d128, g03d128, g04d128; extern _Decimal128 g05d128, g06d128, g07d128, g08d128; extern _Decimal128 g09d128, g10d128, g11d128, g12d128; extern _Decimal128 g13d128, g14d128, g15d128, g16d128; extern void checkd128 (_Decimal128 x, _Decimal128 v); void initd128 (_Decimal128 *p, _Decimal128 v) { *p = v + 123.456dl; } void checkgd128 (void) { checkd128 (g01d128, 1); checkd128 (g02d128, 2); checkd128 (g03d128, 3); checkd128 (g04d128, 4); checkd128 (g05d128, 5); checkd128 (g06d128, 6); checkd128 (g07d128, 7); checkd128 (g08d128, 8); checkd128 (g09d128, 9); checkd128 (g10d128, 10); checkd128 (g11d128, 11); checkd128 (g12d128, 12); checkd128 (g13d128, 13); checkd128 (g14d128, 14); checkd128 (g15d128, 15); checkd128 (g16d128, 16); } void testd128 (_Decimal128 x01, _Decimal128 x02, _Decimal128 x03, _Decimal128 x04, _Decimal128 x05, _Decimal128 x06, _Decimal128 x07, _Decimal128 x08, _Decimal128 x09, _Decimal128 x10, _Decimal128 x11, _Decimal128 x12, _Decimal128 x13, _Decimal128 x14, _Decimal128 x15, _Decimal128 x16) { checkd128 (x01, 1); checkd128 (x02, 2); checkd128 (x03, 3); checkd128 (x04, 4); checkd128 (x05, 5); checkd128 (x06, 6); checkd128 (x07, 7); checkd128 (x08, 8); checkd128 (x09, 9); checkd128 (x10, 10); checkd128 (x11, 11); checkd128 (x12, 12); checkd128 (x13, 13); checkd128 (x14, 14); checkd128 (x15, 15); checkd128 (x16, 16); } void testvad128 (int n, ...) { int i; va_list ap; if (test_va) { 
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_c23_va_start(
# 89 "./compat/scalar-by-value-dfp_y.c"
ap, n
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 89 "./compat/scalar-by-value-dfp_y.c"
; for (i = 0; i < n; i++) { _Decimal128 t = 
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_arg(
# 89 "./compat/scalar-by-value-dfp_y.c"
ap
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
,
# 89 "./compat/scalar-by-value-dfp_y.c"
_Decimal128
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 89 "./compat/scalar-by-value-dfp_y.c"
; checkd128 (t, i+1); } 
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
__builtin_va_end(
# 89 "./compat/scalar-by-value-dfp_y.c"
ap
# 89 "./compat/scalar-by-value-dfp_y.c" 3 4
)
# 89 "./compat/scalar-by-value-dfp_y.c"
; } }
