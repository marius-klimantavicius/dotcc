//type:fn
//options_all:--clang
//remark:[6.0] Abort in Clang mode during error recovery
// 8/15/19  [EDGcpfe/21660]
//
// Abort in Clang mode during error recovery
//
// During some error recovery situations the front end could abort due to access
// through a null pointer in Clang mode (in scan_expr_full).
//
// That problem is now fixed.
namespace N { template<typename> struct E; }
template <class T> struct E {};
using namespace N;
void f() {
  E<int> e;  // Error: Ambiguous E.  Previously aborted afterwards.
}
