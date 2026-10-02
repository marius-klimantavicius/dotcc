//type:fp
//options_all:--clang
//remark:[6.2] Clang compatibility: Conversion to "ext_vector_type" types
// 9/4/20   [EDGcpfe/22952,EDGcpfe/23348]
//
// Clang compatibility: Conversion to "ext_vector_type" types
//
// In Clang mode, any arithmetic or enumeration type can now be converted to a
// "ext_vector_type" type.
typedef float VF4 __attribute__((ext_vector_type(4)));
VF4 vec = (VF4)1;  // Now accepted in Clang modes.
