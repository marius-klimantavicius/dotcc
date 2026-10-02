//type:fp
//options_all:--c++11
//remark:[4.11] Integral constant variables passed to constexpr reference parameters
// 12/9/15  [EDGcpfe/16677]
//
// Integral constant variables passed to constexpr reference parameters
//
// Previously, the front end incorrectly allowed only constexpr variables to
// be passed as constants to reference parameters of constexpr functions.  It
// has now been changed to allow integral constant variables as arguments as
// well.
constexpr int f(const int& r) { return r; }
void g() {
  const int i = 2;
  constexpr int j = f(i);   // Previously rejected as non-constant
}
