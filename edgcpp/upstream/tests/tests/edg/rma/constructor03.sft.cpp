//options_all:-r -x -tused
//options: --strict;cn:;cn

class A {
  ~A;
  ~B();
  A::~A();
  static ~A();
};

