//type: fp
//options: 
# 0 "./compat/scalar-return-dfp_y.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./compat/scalar-return-dfp_y.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4

# 40 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 103 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdarg.h" 3 4
typedef __gnuc_va_list va_list;
# 2 "./compat/scalar-return-dfp_y.c" 2

# 1 "./compat/compat-common.h" 1
# 53 "./compat/compat-common.h"

# 53 "./compat/compat-common.h"
extern void abort (void);

extern int fails;
# 4 "./compat/scalar-return-dfp_y.c" 2
# 65 "./compat/scalar-return-dfp_y.c"
extern _Decimal32 g01d32, g02d32, g03d32, g04d32; extern _Decimal32 g05d32, g06d32, g07d32, g08d32; extern _Decimal32 g09d32, g10d32, g11d32, g12d32; extern _Decimal32 g13d32, g14d32, g15d32, g16d32; extern void checkd32 (_Decimal32 x, _Decimal32 v); void initd32 (_Decimal32 *p, _Decimal32 v) { *p = v + 1.2df; } void checkgd32 (void) { checkd32 (g01d32, 1+1.2df); checkd32 (g02d32, 2+1.2df); checkd32 (g03d32, 3+1.2df); checkd32 (g04d32, 4+1.2df); checkd32 (g05d32, 5+1.2df); checkd32 (g06d32, 6+1.2df); checkd32 (g07d32, 7+1.2df); checkd32 (g08d32, 8+1.2df); checkd32 (g09d32, 9+1.2df); checkd32 (g10d32, 10+1.2df); checkd32 (g11d32, 11+1.2df); checkd32 (g12d32, 12+1.2df); checkd32 (g13d32, 13+1.2df); checkd32 (g14d32, 14+1.2df); checkd32 (g15d32, 15+1.2df); checkd32 (g16d32, 16+1.2df); } _Decimal32 test0d32 (void) { return g01d32; } _Decimal32 test1d32 (_Decimal32 x01) { return x01; } _Decimal32 testvad32 (int n, ...) { int i; _Decimal32 rslt; va_list ap; 
# 65 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_c23_va_start(
# 65 "./compat/scalar-return-dfp_y.c"
ap, n
# 65 "./compat/scalar-return-dfp_y.c" 3 4
)
# 65 "./compat/scalar-return-dfp_y.c"
; for (i = 0; i < n; i++) rslt = 
# 65 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_arg(
# 65 "./compat/scalar-return-dfp_y.c"
ap
# 65 "./compat/scalar-return-dfp_y.c" 3 4
,
# 65 "./compat/scalar-return-dfp_y.c"
_Decimal32
# 65 "./compat/scalar-return-dfp_y.c" 3 4
)
# 65 "./compat/scalar-return-dfp_y.c"
; 
# 65 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_end(
# 65 "./compat/scalar-return-dfp_y.c"
ap
# 65 "./compat/scalar-return-dfp_y.c" 3 4
)
# 65 "./compat/scalar-return-dfp_y.c"
; return rslt; }
extern _Decimal64 g01d64, g02d64, g03d64, g04d64; extern _Decimal64 g05d64, g06d64, g07d64, g08d64; extern _Decimal64 g09d64, g10d64, g11d64, g12d64; extern _Decimal64 g13d64, g14d64, g15d64, g16d64; extern void checkd64 (_Decimal64 x, _Decimal64 v); void initd64 (_Decimal64 *p, _Decimal64 v) { *p = v + 12.34dd; } void checkgd64 (void) { checkd64 (g01d64, 1+12.34dd); checkd64 (g02d64, 2+12.34dd); checkd64 (g03d64, 3+12.34dd); checkd64 (g04d64, 4+12.34dd); checkd64 (g05d64, 5+12.34dd); checkd64 (g06d64, 6+12.34dd); checkd64 (g07d64, 7+12.34dd); checkd64 (g08d64, 8+12.34dd); checkd64 (g09d64, 9+12.34dd); checkd64 (g10d64, 10+12.34dd); checkd64 (g11d64, 11+12.34dd); checkd64 (g12d64, 12+12.34dd); checkd64 (g13d64, 13+12.34dd); checkd64 (g14d64, 14+12.34dd); checkd64 (g15d64, 15+12.34dd); checkd64 (g16d64, 16+12.34dd); } _Decimal64 test0d64 (void) { return g01d64; } _Decimal64 test1d64 (_Decimal64 x01) { return x01; } _Decimal64 testvad64 (int n, ...) { int i; _Decimal64 rslt; va_list ap; 
# 66 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_c23_va_start(
# 66 "./compat/scalar-return-dfp_y.c"
ap, n
# 66 "./compat/scalar-return-dfp_y.c" 3 4
)
# 66 "./compat/scalar-return-dfp_y.c"
; for (i = 0; i < n; i++) rslt = 
# 66 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_arg(
# 66 "./compat/scalar-return-dfp_y.c"
ap
# 66 "./compat/scalar-return-dfp_y.c" 3 4
,
# 66 "./compat/scalar-return-dfp_y.c"
_Decimal64
# 66 "./compat/scalar-return-dfp_y.c" 3 4
)
# 66 "./compat/scalar-return-dfp_y.c"
; 
# 66 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_end(
# 66 "./compat/scalar-return-dfp_y.c"
ap
# 66 "./compat/scalar-return-dfp_y.c" 3 4
)
# 66 "./compat/scalar-return-dfp_y.c"
; return rslt; }
extern _Decimal128 g01d128, g02d128, g03d128, g04d128; extern _Decimal128 g05d128, g06d128, g07d128, g08d128; extern _Decimal128 g09d128, g10d128, g11d128, g12d128; extern _Decimal128 g13d128, g14d128, g15d128, g16d128; extern void checkd128 (_Decimal128 x, _Decimal128 v); void initd128 (_Decimal128 *p, _Decimal128 v) { *p = v + 123.456dl; } void checkgd128 (void) { checkd128 (g01d128, 1+123.456dl); checkd128 (g02d128, 2+123.456dl); checkd128 (g03d128, 3+123.456dl); checkd128 (g04d128, 4+123.456dl); checkd128 (g05d128, 5+123.456dl); checkd128 (g06d128, 6+123.456dl); checkd128 (g07d128, 7+123.456dl); checkd128 (g08d128, 8+123.456dl); checkd128 (g09d128, 9+123.456dl); checkd128 (g10d128, 10+123.456dl); checkd128 (g11d128, 11+123.456dl); checkd128 (g12d128, 12+123.456dl); checkd128 (g13d128, 13+123.456dl); checkd128 (g14d128, 14+123.456dl); checkd128 (g15d128, 15+123.456dl); checkd128 (g16d128, 16+123.456dl); } _Decimal128 test0d128 (void) { return g01d128; } _Decimal128 test1d128 (_Decimal128 x01) { return x01; } _Decimal128 testvad128 (int n, ...) { int i; _Decimal128 rslt; va_list ap; 
# 67 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_c23_va_start(
# 67 "./compat/scalar-return-dfp_y.c"
ap, n
# 67 "./compat/scalar-return-dfp_y.c" 3 4
)
# 67 "./compat/scalar-return-dfp_y.c"
; for (i = 0; i < n; i++) rslt = 
# 67 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_arg(
# 67 "./compat/scalar-return-dfp_y.c"
ap
# 67 "./compat/scalar-return-dfp_y.c" 3 4
,
# 67 "./compat/scalar-return-dfp_y.c"
_Decimal128
# 67 "./compat/scalar-return-dfp_y.c" 3 4
)
# 67 "./compat/scalar-return-dfp_y.c"
; 
# 67 "./compat/scalar-return-dfp_y.c" 3 4
__builtin_va_end(
# 67 "./compat/scalar-return-dfp_y.c"
ap
# 67 "./compat/scalar-return-dfp_y.c" 3 4
)
# 67 "./compat/scalar-return-dfp_y.c"
; return rslt; }
