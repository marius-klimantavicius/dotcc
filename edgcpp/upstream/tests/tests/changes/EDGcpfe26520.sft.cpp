//type:fp
//options_all:--c++11
//remark:[6.6] Abort on dependent noexcept expression during substitution
// 7/25/23  [EDGcpfe/26520]
//
// Abort on dependent noexcept expression during substitution
//
// When substituting the explicitly-specified template arguments into the function
// template declaration, the noexcept operator remains a dependent expression.
// Previously, however, that dependent noexcept operator was not correctly
// represented, which could in turn lead to an abort due to a null pointer
// indirection in f_identical_types (types.c).  That is now fixed.
template<bool B>
struct C { };
template<typename T, typename U>
int f(U, C<noexcept(U())>);
int i = f<int>(1, C<true>());
