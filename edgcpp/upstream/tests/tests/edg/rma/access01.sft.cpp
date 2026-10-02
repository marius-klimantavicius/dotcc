//options_all:-r -x -tused
//options: --strict;cp

class A {
protected:
  int i;
};
class B : private A {
private:
  int i;
  void f();
};
void B::f() {
  A::i = 1;
}

