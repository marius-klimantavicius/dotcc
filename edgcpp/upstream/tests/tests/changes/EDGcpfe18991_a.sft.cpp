//type:fp
//options_all:--c++14
//remark:[5.0] Pack expansion in initializer for variadic static data member
// 12/1/17  [EDGcpfe/18991]
//
// C++-generating back end: Initializers for static data member template
//
// The C++-generating back end previously did not always correctly render an
// initializer for a static data member template.
//
// Previously, the in-class initializer for S::X was not rendered by the
// C++-generating back end.  That is now fixed.
//
// 12/1/17  [EDGcpfe/18991]
//
// Pack expansion in initializer for variadic static data member
//
// The front end previously did not correctly parse a pack expansion in an
// in-class initializer for a variadic static data member.
//
// That is now fixed.
struct S {
  template<int ...Is>
    static constexpr int X[10] = { Is... };  // Previously an error.
};                                           // Now okay.
