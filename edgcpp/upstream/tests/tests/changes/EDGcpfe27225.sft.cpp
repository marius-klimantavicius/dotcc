//type:fp
//options_all:--gnu=130000
//remark:[6.7] Mangling for _Complex _Float32x and _Complex _Float64x
// 6/6/24   [EDGcpfe/27225]
//
// Mangling for _Complex _Float32x and _Complex _Float64x
//
// The mangling for _Complex _Float32x and _Complex _Float64x types had been
// omitted previously (leading to an assertion failure in
// mangled_encoding_for_type_full).  That has now been fixed.
// (with --gnu_version 130000):
void f(_Complex _Float32x) {}
void f(_Complex _Float64x) {}
