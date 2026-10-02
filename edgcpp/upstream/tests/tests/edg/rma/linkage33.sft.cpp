//options_all:-r -x -tused
//options: --strict;cp

namespace A {
  extern "C" int f();
}
int A::f() { return 98; }     // definition for the function f
                              // with C language linkage
extern "C" int g();
int g() {  return 0; }

