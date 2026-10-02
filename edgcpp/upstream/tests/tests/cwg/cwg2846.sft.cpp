//options_all:--c++23 -A
  struct A {
    void f(this A&);
  };
  void A::f(this A&) { }    // #1

//cwg: 2846
//title: Out-of-class definitions of explicit object member functions
//meeting: Tokyo 3/24
//edg_status: Passes
