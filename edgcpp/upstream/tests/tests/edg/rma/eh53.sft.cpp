//options_all:-r -x -tused
//options: --strict;cn

struct A {
  A() try {} catch (...) { return; }
};

