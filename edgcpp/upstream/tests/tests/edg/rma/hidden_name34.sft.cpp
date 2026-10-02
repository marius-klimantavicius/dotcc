//options_all:-r -x -tused
//options: --strict;cp


int f();
namespace A {
  int f();
}
void g() {
  using namespace A;
  A::f();
  ::f();
}
using namespace A;
void gg() {
  A::f();
  ::f();
}
int i = ::f();


