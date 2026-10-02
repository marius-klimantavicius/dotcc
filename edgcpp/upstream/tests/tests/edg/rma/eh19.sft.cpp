//options_all:-r -x -tused
//options: --strict;cp
//options_all:--c_to_obj_option -w

extern "C" void printf(char *, ...);
struct A {
  virtual void f() throw(A) { printf("A::f\n"); throw A(); }
};

struct B {
  virtual void f() throw(B) { printf("B::f\n"); throw B(); }
};

struct C : public A, public B {
  virtual void f() throw(C) { printf("C::f\n"); throw C(); }
};

int main() {
  int flag = 0;
  C c;
  A *pa = &c;
  try {
    pa->f();
  }
  catch (A) { flag = 1; };
  printf("%d\n", flag);
}


