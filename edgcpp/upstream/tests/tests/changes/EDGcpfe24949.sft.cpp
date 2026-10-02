//type:fp
//options_all:--c++11
//remark:[6.4] Spurious error with explicit template argument and dependent return type
// 8/3/22   [EDGcpfe/24949]
//
// Spurious error with explicit template argument and dependent return type
//
// The front end previously issued a spurious "no instance of function
// template matches" error in cases where the function template's return type
// depends on a variadic template parameter whose corresponding template
// argument is explicitly specified.  This is now fixed.
// --c++11:
template<typename... Ts> auto f(Ts&&... ts) -> decltype(int(ts...));
int i = f<int>(0);   // Previously an error, now okay
