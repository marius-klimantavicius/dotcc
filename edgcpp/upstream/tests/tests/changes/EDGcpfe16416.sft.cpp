//type:fp
//options_all:--c++14
//remark:[4.11] Abort in lowering on nested init-captures
// 8/20/15  [EDGcpfe/16416]
//
// Abort in lowering on nested init-captures
//
// In C++14 mode (and other modes that support init-capture in lambdas) the front
// end sometimes aborted with an internal error (in make_init_entity_node) when
// lowering the (invalid) IL for an init-capture whose initializer contains a
// lambda with its own init-capture.  The problematic cases involved captured
// values requiring nontrivial destruction.
//
// This is now fixed.
struct S { ~S() {} };
void g() {
  S s;
  [x = [x = s]{ return x; }()]{ return x; }();  // Previously triggered an
}                                               // abort during lowering.
