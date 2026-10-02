//type:fp
//options_all:--clang_v 190000 --c++20
//remark:[6.7] Clang compatibility: allow ext_vector_type on bool elements
// 9/9/24   [EDGcpfe/27579]
//
// Clang compatibility: allow ext_vector_type on bool elements
//
// Clang versions 15.0.0 and later allow the ext_vector_type attribute to apply
// to bool types and now the front end does as well in the appropriate modes.
typedef bool bool4 __attribute__((ext_vector_type(4)));
bool4 v;
