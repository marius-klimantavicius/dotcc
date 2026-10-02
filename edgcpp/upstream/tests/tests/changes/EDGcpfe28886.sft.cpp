//type:fp
//options_all:--c++20
//remark:Implicit "typename" on friend declaration in template
// 6/16/26  [EDGcpfe/28886]
//
// Implicit "typename" on friend declaration in template
//
// Previously, this elicited a spurious error about a missing "typename" keyword
// before the return type X<T>::type.  That is now fixed.
template<typename> struct X { using type = int; };
struct S {
  template<typename T> friend X<T>::type f(S);
};
