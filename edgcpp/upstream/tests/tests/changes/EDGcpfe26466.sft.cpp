//type:fp
//options_all:--c++23
//remark:[6.6] C++23: std::bfloat16_t type
// 9/7/23   [EDGcpfe/26466]
//
// C++23: std::bfloat16_t type
//
// The std::bfloat16_t type is now supported by the front end in C++23 modes
// (see the entry for EDGcpfe/25100,EDGcpfe/25170,EDGcpfe/25515).  Because of
// currently-limited compiler support for a native bfloat16 type, the internal
// representation of bfloat16 values is a host "float".  (See the comments in
// float_type.h for the FPT_BFLOAT16 case for a more complete discussion of
// the rationale for this choice.)
using bf16_t = decltype(0.0bf16);   // Previously treated as std::float32_t
using f32_t = decltype(0.0f32);
template<typename> struct S {};
template<> struct S<f32_t> {};
template<> struct S<bf16_t> {};     // Previously a redefinition, now okay
