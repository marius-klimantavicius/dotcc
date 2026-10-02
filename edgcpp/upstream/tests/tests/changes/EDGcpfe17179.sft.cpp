//type:fp
//remark:[4.12] Folded initializer not rendered by C++-generating back end
// 5/23/16  [EDGcpfe/17179]
//
// Folded initializer not rendered by C++-generating back end
//
// The changes for EDGcpfe/16501 introduced a regression causing the C++-
// generating back end not to render an initializer consisting of a functional-
// notation style cast folded to an aggregate constant.
//
// This is now fixed (although, in the case above, the constant is rendered as
// "S{}").  The folded constant is now marked has having an explicit cast
// applied.
constexpr struct S {
  explicit S() = default;  // Implicitly constexpr.
} s = S();  // Previously "S()" was not rendered by the C++-generating
            // back end.
