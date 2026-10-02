//type:fn
//options_all:--c++20 -tused
  struct A {
    int n;
    consteval A() {}
  };
  constexpr A a; // implementations reject

//cwg: 2558
//title: Uninitialized subobjects as a result of an immediate invocation
//meeting: Issaquah 2/23
//edg_status: Passes
