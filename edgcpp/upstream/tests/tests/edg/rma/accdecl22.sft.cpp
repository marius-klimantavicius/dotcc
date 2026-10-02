//options_all:-r -x -tused
//options: --strict;cn:;ln

class A {
public:
  void f();
  void f(int);
  void f(int,int);
};
class B : private A {
public:
  A::f;
};
main() {
  B b;
  b.f();
  b.f(0);
  b.f(0,0);
}

