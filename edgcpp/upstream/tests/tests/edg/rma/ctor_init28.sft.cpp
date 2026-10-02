//options_all:-r -x -tused
//options: --strict;cn

namespace N {
  class A { A(int); };
}
class B : public N::A {
  B() : A(0) { }
};

