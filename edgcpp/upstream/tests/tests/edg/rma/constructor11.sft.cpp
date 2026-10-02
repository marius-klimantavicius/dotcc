//options_all:-r -x -tused
//options: --strict;cp

struct S {
//  void f();
  S();
};
//void (S::f()) { }
(S::S()) { }


