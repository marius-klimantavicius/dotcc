//type:fp
//options_all:--c++11
//remark:Spurious pack expansion error for default template argument of member template
// 4/30/26  [EDGcpfe/24302,EDGcpfe/28505]
//
// Spurious pack expansion error for default template argument of member template
//
// Previously, a pack expansion where the pattern refers to a member template with
// default template arguments could result in a spurious pack expansion error.
template<typename ... Ts>
struct B { };
template<typename ... Ts>
struct C {
  template<typename T, typename U = T>  // Previously a spurious error.
  struct A;                             // Now okay.
  B<A<Ts> ...> b;
};
