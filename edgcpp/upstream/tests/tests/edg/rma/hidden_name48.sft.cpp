//options_all:-r -x -tused
//options: --strict;cp

int f();
namespace A {
  int f();
  int g();
  int h();
}
int g();
using namespace A;
int h();
int i = ::f();
int j = ::g();
int k = ::h();

