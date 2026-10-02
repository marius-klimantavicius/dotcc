//options_all:-r -x -tused
//options: --strict;cp

// EDGqa01088
int zero = 0;
namespace A {
  double zero = 0.0;
}
int a = zero;
using namespace A;
int b = ::zero;           // would be ambiguous without global qualifier

