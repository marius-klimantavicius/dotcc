//type:fp
//options_all:--clang --c++11
//remark:[6.8] GNU/Clang compatibility: pack expansion in "format" attribute
// 4/7/25   [EDGcpfe/28072]
//
// GNU/Clang compatibility: pack expansion in "format" attribute
//
// The GNU/Clang format attribute is used to enable additional checking for
// routines that have printf/scanf-like arguments.  Previously the front end
// checked that the parameter corresponding to the value of the third argument to
// the format attribute referred to an ellipsis parameter.  That check has now
// been removed in Clang mode, which allows that parameter to be a pack expansion
// (or other parameter).
template <class... T>
int myprintf(char const*, T...) __attribute((format(printf, 1, 2)));
