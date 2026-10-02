//type:fp
//options_all:--c++11 --clang
//remark:[6.6] Substitution of GNU/clang type-transforming intrinsics
// 10/5/23  [EDGcpfe/21200,EDGcpfe/26657,EDGcpfe/26690]
//
// Substitution of GNU/clang type-transforming intrinsics
//
// The changes for EDGcpfe/26193 added front end support for a number of
// type-transforming intrinsics such as __remove_cv and __add_pointer.  However,
// these intrinsics were not handled during template substitution.
// with --c++11 --clang:
template<typename T> __add_pointer(T) f(T);
auto v = f(1);  // Spurious error.  Now okay.
