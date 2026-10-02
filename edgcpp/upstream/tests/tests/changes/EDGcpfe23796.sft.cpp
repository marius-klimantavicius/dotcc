//type:fp
//options_all:--c++20
//remark:[6.2] Nontrivial generated constexpr destructors
// 1/22/21  [EDGcpfe/23796]
//
// Nontrivial generated constexpr destructors
//
// The front end previously always treated nontrivial generated destructors as not
// being constexpr.  However, that is not correct behavior if the generated
// destructor never calls a non-constexpr destructor.
//
// Previously, the front treated the (generated) destructor of C as non-constexpr.
// Now, it is considered a constexpr member function because it only calls the
// destructor of S, which is itself constexpr.
struct S {
  constexpr ~S() {}
};
class C { S s; };
constexpr C c{};  // Previously an error.  Now okay.
