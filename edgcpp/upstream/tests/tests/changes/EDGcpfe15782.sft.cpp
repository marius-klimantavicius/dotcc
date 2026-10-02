//type:fp
//options_all:--c++11
//remark:[4.10.1] Generalized constant-expressions in non-type template arguments
// 1/26/15  [EDGcpfe/15782]
//
// Generalized constant-expressions in non-type template arguments
//
// The front end previously didn't always accept constant-expressions resulting
// from calls to constexpr functions in template arguments, even in cases where
// C++11 permits them.
//
// This is now fixed.
struct S {};
constexpr int one(S const&) { return 1; }
template<int I> void g() {};
int main() {
  g<one(S())>();  // Previously triggered a spurious error.  Now okay.
}
