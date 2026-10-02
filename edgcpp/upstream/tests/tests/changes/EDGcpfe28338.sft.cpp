//type:fp
//options_all:--c++20
//remark:[6.8] Abort on requires expression in default template argument
// 7/21/25  [EDGcpfe/28338]
//
// Abort on requires expression in default template argument
//
// With the changes for EDGcpfe/27599 (in version 6.7), the front end could enter
// an unbounded recursion for a requires expression appearing in a default
// template argument.
template<typename T, bool = requires { T::v; }>
struct A : A<T> { };  // Previously aborted, now okay.
