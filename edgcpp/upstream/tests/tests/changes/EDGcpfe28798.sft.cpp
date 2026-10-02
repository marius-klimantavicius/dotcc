//type:fp
//options_all:--no_warnings --c++17
//remark:Regression causing assertion failure in add_pragmas_to_string
// 4/21/26  [EDGcpfe/28798]
//
// Regression causing assertion failure in add_pragmas_to_string
//
// With the changes for EDGcpfe/27955 (in version 6.8), an assertion failure
// occurs in configurations using the C++-generating back end when encountering
// "pseudo-pragmas".
//
// This is now fixed.
template<class T> void m() { /*NOTREACHED*/ }
