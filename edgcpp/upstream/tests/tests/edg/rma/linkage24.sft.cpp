//options_all:-r -x -tused
//options: --strict;cp

namespace A {
  extern "C" {
    void f();
    void g();
  }
}
extern "C++" void f();      // Not treated as a redeclaration of A::f()
int g();                    // Not treated as a redeclaration of A::g()

