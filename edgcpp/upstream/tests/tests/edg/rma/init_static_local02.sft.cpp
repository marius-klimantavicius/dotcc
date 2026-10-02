//options_all:-r -x -tused
//options: --strict;cp

struct A {
  ~A();
};
void f() {
  static A a;
}

