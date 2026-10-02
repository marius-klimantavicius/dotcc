//type:fp
//options_all:--clang
//remark:[6.7] Clang compatibility: __ext_vector_type__ attribute in alias declarations
// 7/8/24   [EDGcpfe/27266,EDGcpfe/27434]
//
// Clang compatibility: __ext_vector_type__ attribute in alias declarations
//
// The Clang-specific __ext_vector_type__ attribute was previously only allowed
// on typedef declarations and is now also accepted on alias declarations.
using FourShorts = short __attribute__((ext_vector_type(4)));
