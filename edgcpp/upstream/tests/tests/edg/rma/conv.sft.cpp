//options_all:-r -x -tused
//options: --strict;cp

class A {
public:
  int i;
  operator int();
  A(int x=0) : i(x) {}
};
A::operator int() { return i; }
A a(1);
int m = a.operator int();

