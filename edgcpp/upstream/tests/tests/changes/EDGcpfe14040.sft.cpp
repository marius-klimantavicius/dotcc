//type:fn
//options_all:--c++11
//remark:[4.8] Generated default constructor with const members
// 5/24/13  [EDGcpfe/14040]
//
// Generated default constructor with const members
//
// Previously, in C++11 mode, the front end generated an ordinary default
// constructor definition for classes with uninitialized const members.  Now,
// the default constructor is defined as deleted (except in GNU C++11 mode,
// since GCC does not appear to implement that C++11 requirement).
struct A {
  constexpr int f() const { return x+1; }
  int const x;   // Uninitialized const field means the generated default
};               // constructor should be "= delete" in C++11 mode.
int x[A().f()];  // Previously accepted in C++11 mode; now an error.
