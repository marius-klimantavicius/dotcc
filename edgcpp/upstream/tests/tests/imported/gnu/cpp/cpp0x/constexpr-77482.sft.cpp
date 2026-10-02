//type: fn
//options: --c++11
// PR c++/77482
// { dg-do compile { target c++11 } }

constexpr auto x;	// { dg-error "declaration\[^\n\r]*has no initializer" }
extern struct S s;
constexpr auto y = s;	// { dg-error "incomplete" }
