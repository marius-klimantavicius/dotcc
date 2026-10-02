//type:fn
//remark:[4.13] Binding nontype template parameters of reference type.
// 10/17/16 [EDGcpfe/17625]
//
// Binding nontype template parameters of reference type.
//
// The front end did not always diagnose invalid nontype template arguments for
// nontype template parameters of reference type.
//
// This is now fixed.  (The problem was more prevalent in modes that support the
// "constexpr" feature, but even in other modes the problem could arise, as the
// example above demonstrates.)
template<int&> struct X {};
int x[2];
struct X<x[1]> xx;  // Previously erroneously accepted.  Now an error
                    // because nontype template arguments cannot refer to
                    // proper subobjects.
