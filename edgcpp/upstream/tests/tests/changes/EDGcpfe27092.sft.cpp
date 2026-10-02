//type:fp
//options_all:--c++11
//remark:[6.7] Equivalence of expressions containing pack expansions
// 3/13/24  [EDGcpfe/27092]
//
// Equivalence of expressions containing pack expansions
//
// Previously, the front end incorrectly considered expressions that only differ
// in their pack expansions to be equivalent.
template<typename T> T z();
template<typename ... TT>
void f(decltype(x(y(z<TT>()...)))) {}
template<typename ... TT>
void f(decltype(x(y(z<TT>())...))) {}  // Previously a spurious error.
                                       // Now okay.
