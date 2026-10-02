//type:fp
//options_all:--clang_v 160000
//remark:[6.6] Assertion failure in record_substitution_for_type on type-returning traits
// 10/8/23  [EDGcpfe/26706]
//
// Assertion failure in record_substitution_for_type on type-returning traits
//
// The front end had failed an assertion check (in record_substitution_for_type)
// in configurations that do IA-64 mangling when using type-dependent type-
// returning traits.
template<typename T> __remove_cv(T) f(T);
auto x = f(1);
