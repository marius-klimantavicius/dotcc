//options_all:--c++23
  struct A {
    A(const A&) = delete;
  };
  struct B {
    operator A&&();
  };
  const A& r = B();

//cwg: 2919
//title: Conversion function candidates for initialization of const lvalue reference
//meeting: Wroclaw 11/24
//edg_status: Passes
