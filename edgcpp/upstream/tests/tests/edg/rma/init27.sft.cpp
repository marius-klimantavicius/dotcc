//options_all:-r -x -tused
//options: --strict;cn

struct A { int i; const int j; int k; };
A a[2] = { 1, 1, 1 };
struct B { int i; const int &j; int k; };
B b[2] = { 1, 1, 1 };

