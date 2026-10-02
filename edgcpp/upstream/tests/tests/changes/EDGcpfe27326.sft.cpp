//type:fp
//options_all:--c++11
//remark:[6.7] Abort on partial specialization declaration with dependent non-type template
// 12/2/24  [EDGcpfe/27326,EDGcpfe/27729]
//
// Abort on partial specialization declaration with dependent non-type template
// parameter
//
// The changes for [EDGcpfe/19237,EDGcpfe/25886,EDGcpfe/26582] (in version 6.6)
// introduced a null pointer indirection in look_up_member_in_substituted_parent
// in some cases where a primary template with a dependent non-type template
// parameter is partially specialized.
template<typename T>
struct D {
  using type = T;
};
template<typename T, typename U, typename D<T>::type>
struct C;
template<typename U>
struct C<int, U, 0>;  // Previously aborted, now okay.
