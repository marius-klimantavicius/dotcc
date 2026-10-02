//options_all:-r -x -tused
//options: --strict;cp

struct A { int a; };

A x = {1};
A y(x);
A z = {1};

