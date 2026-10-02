//type:fp
//options_all:--c++20 --gn 110200
//remark:[6.3] Constraints on alias templates
// 8/12/21  [EDGcpfe/24600]
//
// Constraints on alias templates
//
// The front end sometimes failed to check constraints on alias templates during
// substitution.
//
// That is now fixed.
template<typename T> requires false using A = T;
template<typename T> A<T> f(T);
template<typename T> int f(T);
int r = f(42);  // Previously ambiguous.  Now okay.
