//options_all:-r -x -tused
//options: --strict;cp

struct A {
  A();
  ~A();
};
struct B {
  friend A::A();
  friend A::~A();
};

