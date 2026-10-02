//type:fp
//options_all:--c++14
//remark:[4.10] Capturing "this" in nonstatic data member initializers
// 9/4/14   [EDGcpfe/15384,EDGcpfe/15386]
//
// Capturing "this" in nonstatic data member initializers
//
// Previously, the front end did not permit the capturing of the "this" pointer
// by lambda expressions appearing in nonstatic data member initializers.  That
// limitation is now lifted.
struct S {
  int a;
  struct N { int b; } n = { [=]{ return a; }() };
    // The initializer for n implicitly captures this.  Now accepted.
} s = { 42 };  // s is initialized to { 42, { 42 } }.
