//type:fp
//options_all:--c++11
//remark:[6.7] Substitution failures triggered by narrowing conversions in system headers
// 10/29/24 [EDGcpfe/25759,EDGcpfe/27606]
//
// Substitution failures triggered by narrowing conversions in system headers
//
// The front end usually suppresses diagnostics for narrowing conversions
// appearing in system headers.  Consequently, narrowing conversions previously
// didn't trigger substitution failures, which could result in overload resolution
// issues.  That is now fixed.
# 1 "syshdr" 3
template<typename T, typename U>
auto f(T t, U u, int) -> decltype(T{u}, void());
template<typename T, typename U>
int f(T t, U u, long);
# 1 "file.cpp"
int i = f(0.0f, 0.0, 0);  // Previously a spurious error if "f" was declared
                          // in a system header.  Now okay.
