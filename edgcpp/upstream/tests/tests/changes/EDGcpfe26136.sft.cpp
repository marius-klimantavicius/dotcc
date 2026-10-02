//type:fp
//options_all:--c++17
//remark:[6.5] Abort on default argument of inherited constructor
// 3/17/23  [EDGcpfe/26136]
//
// Abort on default argument of inherited constructor
//
// In some cases where a class template inherited a constructor with a
// non-trivially destructible temporary in one of its default arguments, the front
// end would abort in i_copy_dynamic_init if that class template was instantiated
// from within a template argument list.
struct A {
  ~A();
};
struct B {
  B(A = A());
};
template<typename T>
struct C : B {
  using B::B;
};
template<unsigned> struct X {};
X<sizeof(C<int>)> x;
