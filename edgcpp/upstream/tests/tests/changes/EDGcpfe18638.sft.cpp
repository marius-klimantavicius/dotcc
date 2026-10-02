//type:fp
//options_all:--c++17
//remark:[5.0] C++17: Pack expansions in using-declarations
// 11/17/17 [EDGcpfe/18638]
//
// C++17: Pack expansions in using-declarations
//
// In C++17 mode, the front end now accepts pack expansions in using-declarations
// (as well as using-declarations referring to multiple names).
struct B1 { void f(); };
struct B2 { int f(int); };
template<typename ... Ts> struct D: private Ts... {
  using Ts::f...;
};
D<B1, B2> d;
int r = d.f(42);  // Calls B2::f(int).
