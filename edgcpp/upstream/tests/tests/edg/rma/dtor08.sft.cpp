//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A {
  (~A)();
};
(A::~A)() { };

