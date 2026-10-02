//type:fp
//options_all:--c++11
//remark:[4.7] Elided braces in C++11-style aggregate initializers (Core issue 1270)
// 5/8/13   [EDGcpfe/14001]
//
// Elided braces in C++11-style aggregate initializers (Core issue 1270)
//
// When C++11 generalized brace-enclosed initializers the allowance to elide
// braces in aggregate initializers was not extended to the new contexts
// allowing braced initializers.
//
// The C++ committee recently revisited its original decision, however, to deal
// with Core issue 1270, and decided to permit brace elision in all aggregate
// initialization contexts after all.  The front end now implements this in all
// C++11 modes, except in GNU C++11 modes where gnu_version < 40800 (since the
// corresponding GCC compilers do not accept elision in all contexts).
int x[2][2] = { 1, 2, 3, 4 };  // This has always been okay.
int y[2][2]{ 1, 2, 3, 4 };     // This was originally disallowed in C++11,
                               // but now it is permitted.
