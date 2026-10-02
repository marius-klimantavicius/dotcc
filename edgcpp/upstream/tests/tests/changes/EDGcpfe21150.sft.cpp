//type:fp
//remark:[5.1] Substitution of parenthesized decltype
// 4/16/19  [EDGcpfe/21150]
//
// Substitution of parenthesized decltype
//
// In some cases, substituting a parenthesized decltype resulted in the front end
// incorrectly ignoring the parentheses.  That could lead to spurious errors or
// incorrect results.
//
// Previously, the instantiation of g<int> failed because the type obtained by
// substitution ignored the extra parentheses and therefore differed from the
// type obtained by instantiation.  This is now fixed.
template<typename> void f();
template<typename T> using X = decltype((f<T>));
template<typename T> struct S {};
template<typename T> S<X<T>> g();
auto r = g<int>();  // Previously triggered an error.
