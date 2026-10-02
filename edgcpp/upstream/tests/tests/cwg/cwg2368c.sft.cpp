//type: fp
//options_all: -A --c++20 -tused -e 200 --no_wrap
//
// This test is ill-formed, but no diagnostic is required. This is another case of "ill-formed, no diagnostic required." 
// There are no arguments that would enable A::f() to be called in a constant expression,
//  so it's ill-formed, but an implementation is not required to diagnose it: 
//  9.1.5p5 [dcl.constexpr] (N4778).
//
//
  struct A { 
    int a; 
  private: 
    int b; 
    constexpr auto f() { return &a < &b; }   // not constant 
  }; 
