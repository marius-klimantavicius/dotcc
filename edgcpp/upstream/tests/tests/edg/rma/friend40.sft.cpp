//options_all:-r -x -tused
//options: --strict;cn

class A {
  class B { };
};
class C {
  friend class A::B;
};
class D : A {
  using A::B;
  friend class B;
};

