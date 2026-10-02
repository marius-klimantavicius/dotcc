//type:fp
//options_all:--c++20
//remark:[6.8] Spurious instantiation of variable template initializer during substitution
// 9/9/25   [EDGcpfe/27845]
//
// Spurious instantiation of variable template initializer during substitution
//
// Previously, substitution of a variable template always triggered the
// instantiation of its initializer, even when the variable was used only in an
// unevaluated context.
template<typename T>
int c = sizeof(T);  // Previously a spurious error.  Now okay.
template<typename T>
constexpr bool v = requires { c<T>; };
static_assert(v<void>);
