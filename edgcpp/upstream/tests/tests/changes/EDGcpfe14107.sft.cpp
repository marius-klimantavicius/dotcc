//type:fp
//options_all:--c++14
//remark:[4.10] C++14: binary literals
// 8/15/14  [EDGcpfe/14107]
//
// C++14: binary literals
//
// The front end now accepts binary literals, as described in C++ Committee
// paper N3472, in C++14 mode.
int i = 0b1110;  // Equivalent to 0xe or 016
