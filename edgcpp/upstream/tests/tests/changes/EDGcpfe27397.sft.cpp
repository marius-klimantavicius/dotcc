//type:fp
//options_all:-w --gn 999999
//remark:[6.7] Incorrect constant-evaluation of real-to-complex conversion
// 7/19/24  [EDGcpfe/27397]
//
// Incorrect constant-evaluation of real-to-complex conversion
//
// This example previously failed to evaluate to a constant expression, due to
// the incorrect evaluation of the implicit conversion of f from "float" to
// "_Complex double".  That is now fixed.
constexpr auto g() {
  float f{1};
  _Complex double cx{1};
  return cx / f;  // Previously incorrectly evaluated.  Now okay.
}
constexpr auto r = g();
