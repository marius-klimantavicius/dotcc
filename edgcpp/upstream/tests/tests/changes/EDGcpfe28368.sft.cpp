//type:fp
//options_all:--targ linux_aarch64 --clang_v 210100 --c++17
//remark:[6.8] Abort on lowering of __mfp8 initialization
// 7/31/25  [EDGcpfe/28368]
//
// Abort on lowering of __mfp8 initialization
//
// This previously triggered an internal error when attempting to lower the IL
// for this example (lowering caused the invocation of type_change_constant_full,
// where the internal error occurred).  That is now fixed.
typedef __attribute__((neon_vector_type(16))) __mfp8 WV;
__mfp8 wght;
WV wv = {wght};
