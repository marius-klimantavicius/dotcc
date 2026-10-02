//remark:auto(x)/auto{x}
//options:--c++23;fp:--c++23 -DNEG;fn

struct A {};
  void f(A&);  // #1
  void f(A&&); // #2
  A& g();
  void h() {
    f(g());        // calls #1
    f(A(g()));     // calls #2 with a temporary object
    f(auto{g()});  // calls #2 with a temporary object
#ifdef NEG
    f(auto());
    f(auto(1, 1));
#endif
  }
