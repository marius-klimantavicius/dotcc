//type:fp
//options_all:--c++11
//remark:[6.8] Equivalence of alias templates in partial template specialization matching
// 7/9/25   [EDGcpfe/28073]
//
// Equivalence of alias templates in partial template specialization matching
//
// The changes for EDGcpfe/20656,EDGcpfe/26160,EDGcpfe/26449 in version 6.6
// implemented the direction of Core issue 1286 to treat simple alias templates as
// equivalent to the aliased template, but did not consider matching of partial
// template specializations.
template<typename T> struct C;
template<typename T> using A = C<T>;
template<typename T, template<typename> class> struct B;
template<typename T> struct B<T, C> { };
B<int, A> b;  // Previously a spurious error.  Now okay.
