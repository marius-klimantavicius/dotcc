//type:fn
//options_all:--c++17 -tused -A
//
  class A {
    ~A() {}
  };
  A f() { return A(); }   // error: destructor of A is private (even though it is never invoked)

//cwg: 2426
//title: Reference to destructor that cannot be invoked
//meeting: Belfast 11/19
//edg_status: Passes
