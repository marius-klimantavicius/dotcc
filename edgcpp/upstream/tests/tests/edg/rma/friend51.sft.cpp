//options_all:-r -x -tused
//options: --strict;cp

class A {
  friend void f();
  friend void f(int);
  friend void g();
  friend void h();
};
class B {
  friend void g();
  friend void g(int);
};
void h(int);

