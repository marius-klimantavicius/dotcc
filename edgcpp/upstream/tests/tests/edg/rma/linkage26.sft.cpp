//options_all:-r -x -tused
//options: --strict;cn

struct S {
  friend void f();
};
extern "C" void f();

