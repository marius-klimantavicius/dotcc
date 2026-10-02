//type:fn
//remark:[4.14] Binding a nontype template parameter of reference type
// 5/8/17   [EDGcpfe/18293]
//
// Binding a nontype template parameter of reference type
//
// A nontype template parameter of reference type should bind to a function or
// to a complete object.  However, the front previously failed to diagnose
// attempts to bind such a parameter to a null address.
//
// This is now fixed (an error is issued).
template<int &R> struct X {};
constexpr int *p = nullptr;
X<*p> x;  // Previously accepted.  Now an error.
