//type:fn
//remark:[4.12] Abort when testing trivial copyability of a class with a using-declaration
// 9/30/16  [EDGcpfe/17542]
//
// Abort when testing trivial copyability of a class with a using-declaration
//
// Previously, the front end sometimes aborted with an internal error in
// "is_trivially_copyable_type" (types.c) when testing trivial copyability of a
// class with a using-declaration for an assignment operator.
//
// This is now fixed.
struct B {};
struct D: B {
  using B::operator=;
};
static_assert(__is_trivial(D), "Unexpected");
  // Previously triggered an internal error.  Now okay.
