//type:fp
//options_all:--c++11
//remark:[4.11] Wrong disambiguation with C++11-style functional-notation cast with braces
// 8/27/15  [EDGcpfe/16460]
//
// Wrong disambiguation with C++11-style functional-notation cast with braces
//
// In C++11 mode, the front end sometimes incorrectly disambiguated a
// parenthesized initializer containing a function-notation cast with braces.
//
// The front end incorrectly parsed the declaration of x as the declaration of a
// function, which produced a spurious syntax error.  This is now fixed.
template<typename T> void g() {
  T x(typename T::X{});
}
