//options_all:-r -x -tused
//options: --strict;cp

struct A {
  virtual void f();
};
struct B {
  virtual void f();
};
struct C : virtual A, virtual B {
  virtual void f();
};
struct D : virtual A, virtual B, virtual C { };

int main () {
  D d;
  d.f();
  return 0;
}

