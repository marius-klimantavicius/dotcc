//type:fp
//options_all:--gn 80000
//remark:Operators with template-dependent but not type-dependent operands
// 4/16/26  [EDGcpfe/24532,EDGcpfe/25753,EDGcpfe/26858,EDGcpfe/28017,
//           EDGcpfe/28664,EDGcpfe/28666]
//
// Operators with template-dependent but not type-dependent operands
//
// The front end previously attempted to perform full semantic analysis on
// operators applied to template-dependent operands whose type is known before
// substituting template parameters.  This sometimes resulted in premature
// diagnostics.
//
// Previously, this triggered an error about attempting to call a deleted
// assignment operator from within S::N::operator=, even though that template is
// never instantiated by this example.  Now, such uses of operators are treated
// more generically, and diagnostics are delayed until actual instantiation.
struct X { X& operator=(X const&) = delete; };
struct S {
  S() {}
  template<typename T> struct N {
    X &x;
    N &operator=(N const &src) {
      x = src.x;
    }
  };
};
S s;
