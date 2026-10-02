//options_all:-r -x -tused
//options: --strict;cn

struct A {
  const ~A() {}
};
struct B {
  ~B();
};
const B::~B() {}

