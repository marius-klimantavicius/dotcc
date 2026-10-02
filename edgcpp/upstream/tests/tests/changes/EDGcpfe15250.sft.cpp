//type:fp
//options_all:--c++11
//remark:[4.10] Abort on failure to fold conversion in initializer for constexpr variable
// 6/30/14  [EDGcpfe/15250,EDGcpfe/13864]
//
// Abort on failure to fold conversion in initializer for constexpr variable
//
// In C++11 mode, the front end sometimes failed to fold the conversion of a
// constant derived class object to a base class in a constexpr initialization
// context.  This could lead to an abort in decl_inits.c (in "initializer").
//
// This is now fixed.
struct B { int x = 42; };
struct D: B {};
constexpr B b = B(D());  // Previously aborted in C++11 mode.
