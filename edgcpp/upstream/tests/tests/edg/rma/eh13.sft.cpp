//options_all:-r -x -tused
//options: --strict;cn

void f() {
  class A { };
  typedef void F() throw (A);
  class B {
    void g() throw (A) { throw A(); }
    F gg;
  };
  extern void g() throw (A);      // Error
  extern F gg;                    // Error
}

