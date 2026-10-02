//type:fp
//options_all:--gnu=140200
//remark:_Float32 vs. float after a user-defined conversion
// 7/9/26   [EDGcpfe/28922]
//
// _Float32 vs. float after a user-defined conversion
//
// When ranking two user-defined conversion sequences that share the same
// conversion function, a same-representation floating-point conversion (for
// example, float to _Float32) was incorrectly preferred over an identity
// conversion or a floating-point promotion.
// --c++23:
//
// That is now fixed.
float g(float);
_Float32 g(_Float32);
template<typename T> struct X { operator T() const; };
using R = decltype(g(X<float>{}));
  // R was previously _Float32; now float.
template<class A, class B> struct same { enum { v = 0 }; };
template<class A> struct same<A, A> { enum { v = 1 }; };
static_assert(same<R, float>::v, "R should be float");
