//type:fp
//options_all:--c++11 --g++
//remark:[6.5] Spurious substitution failure for __integer_pack
// 4/28/23  [EDGcpfe/24643,EDGcpfe/24654,EDGcpfe/25000]
//
// Spurious substitution failure for __integer_pack
//
// When explicit template arguments are substituted into the function type,
// function parameter packs cannot be fully expanded yet, meaning that the size of
// such a pack will still be dependent after this initial substitution.  However,
// an __integer_pack construct with a dependent argument was previously considered
// a substitution failure.
template<typename T, T ...>
struct S { };
template<typename ... T>
auto f(T ...) -> S<int, __integer_pack(sizeof ... (T))...>;
auto v = f<int>(1, 2);  // Previously a spurious error.  Now okay.
