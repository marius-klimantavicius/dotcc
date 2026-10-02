//type:fp
//options_all:--clang_v 999999 --c++20
//remark:Assignment of empty class objects during constant evaluation
// 7/9/26   [EDGcpfe/28913]
//
// Assignment of empty class objects during constant evaluation
//
// Assignment of an empty class type with trivial copy semantics does not
// actually access or modify storage.  The front end previously rejected such an
// assignment when the destination object's lifetime began outside the constant
// evaluation, reporting an attempt to access run-time storage.
//
// That is now fixed.
struct E {} x;
constexpr E y{};
constexpr auto r = (x = y);  // Previously an error.  Now okay.
