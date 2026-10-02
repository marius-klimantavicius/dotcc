//type:fp
//options_all:--c++20 --gn 110300
//remark:[6.7] Spurious incomplete type error on GNU-mode static data member initializer
// 7/31/24  [EDGcpfe/27466]
//
// Spurious incomplete type error on GNU-mode static data member initializer
//
// Previously, this elicited a spurious error claiming that X<int> is incomplete
// while processing the initializer for S<int>::x.  That is now fixed.
template<typename> struct X {
  int i;
  X& f();
};
template<typename T> struct S {
   static inline X<T> x{42};
};
auto r = S<int>::x.f();
