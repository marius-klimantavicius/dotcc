//options_all:-r -x -tused
//options: --strict;cp

namespace std {
extern "C" { inline void foo(int) { } }
}
using std::foo;
namespace std {
template<class _P> void foo(_P) { }
}

