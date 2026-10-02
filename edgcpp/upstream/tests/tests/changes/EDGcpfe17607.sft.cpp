//type:fn
//options_all:--c++14
//remark:[4.13] Core issue 1344: Special members and default arguments
// 10/13/16 [EDGcpfe/17607]
//
// Core issue 1344: Special members and default arguments
//
// In C++14 mode, the front end now issues an error if an out-of-class definition
// of a constructor adds default arguments in such a way that the constructor
// becomes a special member function (i.e., a default constructor or copy/move
// constructor).
//
// This corresponds to the resolution of Core issue 1344.
struct S { S(int); };
S::S(int = 0) {}  // Now an error in C++14 mode since adding the default
                  // argument makes the constructor a default constructor.
