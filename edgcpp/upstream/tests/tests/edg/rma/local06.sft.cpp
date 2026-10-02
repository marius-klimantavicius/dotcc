//options_all:-r -x -tused
//options: --strict;cp

void fff() {
struct A {
  typedef int I;
  typedef int II;
  virtual void f() {}
  struct B {
    typedef int J;
    typedef int JJ;
    virtual void f() {}
  };
  typedef int III;
} a;
}

