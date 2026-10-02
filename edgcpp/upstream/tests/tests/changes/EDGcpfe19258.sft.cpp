//type:fn
//options_all:--c++17
//remark:[5.0] Relaxed range-based-for loop iterator requirements
// 2/2/18   [EDGcpfe/19258]
//
// Relaxed range-based-for loop iterator requirements
//
// In C++17 mode, the iterator requirements for range-based-for loops were relaxed
// (see the entry for EDGcpfe/17338).  However, the front end failed to fully
// check the remaining constraints.
//
// This example previously failed to diagnose the fact that the begin() and end()
// iterators have types that cannot be compared.  This is now fixed.
enum E { e = 42 };
E* begin(E const&);
float end(E const&);
void g() {
  for (int i : E()) {}  // Previously accepted.  Now an error.
}
