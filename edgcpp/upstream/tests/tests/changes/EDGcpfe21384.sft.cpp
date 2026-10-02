//type:fp
//options_all:--c++11 -tused
//remark:[6.0] Spurious access error instead of substitution failure
// 8/23/19  [EDGcpfe/21384]
//
// Spurious access error instead of substitution failure
//
// In certain cases the front end would issue an access failure error when
// attempting to instantiate a template, instead of producing a substitution
// failure.
//
// This is now fixed.
template<class Tx> void f(const Tx &);
template<class Tx> auto g(int) -> decltype(f<Tx>({}));
template<class Ty> bool g(float);
class A
{
  A() {}; // Private ctor
};
decltype(g<A>(0)) a; // Spurious A::A() is inaccessible error
