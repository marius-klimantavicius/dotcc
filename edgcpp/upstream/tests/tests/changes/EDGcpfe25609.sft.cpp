//type:fp
//options_all:--c++20
//remark:[6.5] Incorrect expansion of function parameter pack in trailing requires clause
// 3/27/23  [EDGcpfe/25609,EDGcpfe/26117]
//
// Incorrect expansion of function parameter pack in trailing requires clause
//
// Function parameter packs were not expanded correctly in trailing requires
// clauses, resulting in spurious substitution failures during constraint
// checking.
int g(int);
template<typename ... T> int f(T ... t) requires requires { g(t ...); };
int i = f(1);  // Previously a spurious error.  Now okay.
