//type:fn
//options_all:--microsoft_v 1923 --no_ms_permissive
using BOOL = int;
 
namespace N {
              extern "C" void f(int, int, int, bool);
}
 
void g() {
              N::f(0, 1, 2, false);
}
 
extern "C" void f(int, int, int, BOOL) {
}
