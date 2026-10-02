//type:fp
//options_all:--c++20
//remark:[6.7] Casts to incomplete types in substitution contexts
// 7/3/24   [EDGcpfe/27338]
//
// Casts to incomplete types in substitution contexts
//
// Previously, the front end issued an error because of the attempt to cast to an
// incomplete (non-dependent) class type.  That error was issued when parsing the
// variable template (i.e., before substituting the requires-expression) since the
// cast can never be valid.  However, MSVC, GCC, and Clang all accept such code
// and the front end therefore now also accepts it in nonstrict modes.
template<class T> constexpr bool x = requires (T x) { (struct c)x; };
static_assert(!x<int>);
