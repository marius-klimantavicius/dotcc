//type:fn
//options_all:-w -A --c++20
//remark:[6.5] new/delete mismatch in constant expressions
// 5/2/23   [EDGcpfe/26301]
//
// new/delete mismatch in constant expressions
//
// The call to f() is invalid because a non-array delete-expression is used to
// deallocate storage acquired with an array new-expression.  Previously, the
// front end failed to catch that mismatch.  Now it does (resulting in an error
// for the example above).
constexpr bool f() {
  delete new int[42];
  return true;
}
static_assert(f());  // Previously okay.  Now an error.
