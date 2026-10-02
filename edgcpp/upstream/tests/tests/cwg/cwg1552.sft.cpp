//type:fn
//options_all:--c++14 -tused -A
  struct A {
    A& operator=(A&);
  };
  A& A::operator=(A&) = default;

  A& A::operator=(A&) noexcept(false) = default;

//cwg: 1552
//title: exception-specifications and defaulted special member functions
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
