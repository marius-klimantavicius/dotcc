//options_all:-r -x -tused
//options: --strict;cn:;cn

class A {
public:
  int i;
  A(int);
};
A a1;
A::A(int x = 0) : i(x) {}
A a2;
main() {
  A a3;
  a3.i = a2.i;
  a2.i = a1.i;
}

