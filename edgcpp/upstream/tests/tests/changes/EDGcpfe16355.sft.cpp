//type:fp
//options_all:--c++11 --g++
//remark:[4.11] GNU compatibility: Inheriting constructors from indirect base classes
// 7/13/15  [EDGcpfe/16355]
//
// GNU compatibility: Inheriting constructors from indirect base classes
//
// GCC accepts using-declarations for inheriting constructors that name indirect
// base classes, but does not appear to create usable constructors from such
// constructs.  As an approximation, the front end now simply ignores these
// constructs with a warning.
struct B {};
struct C: B { using B::B; };  // Okay.
struct D: C { using B::B; };  // Normally an error, but now ignored in
                              // GNU C++11 mode (with a warning).
D d;
