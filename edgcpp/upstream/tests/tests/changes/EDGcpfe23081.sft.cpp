//type:fp
//options_all:--c++14 --gnu_version=70300
//remark:[6.1] GNU compatibility: Value-initialized vectors in constant expressions
// 7/16/20  [EDGcpfe/23081]
//
// GNU compatibility: Value-initialized vectors in constant expressions
//
// The front end previously did not fold the value-initialization of GNU vector
// types.
//
// That is now fixed.
using V4 = int __attribute((vector_size(16)));
constexpr V4 x = V4();  // Previously an error.  Now okay.
