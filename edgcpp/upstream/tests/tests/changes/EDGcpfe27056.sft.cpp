//type:fp
//options_all:--c++11
//remark:[6.8] Generated default constructor with default-initialized variant member
// 7/21/25  [EDGcpfe/27056,EDGcpfe/28185,EDGcpfe/28343]
//
// Generated default constructor with default-initialized variant member
//
// According to the standard, the default constructor of S should be deleted
// because the variant member S::x cannot be default constructed.  However, Core
// issue 1623 (which is still open) has long argued that this shouldn't matter
// since there is a default-initialized variant member that determines which
// member should be initialized by default.  Only Clang appears to still enforce
// the standard rule.  The front end now matches the other implementations (GCC,
// MSVC) in accepting the example above when not in Clang mode.
struct X { X(int); };
struct S {
  S() = default;
  union {
    int i = {};
    X x;
  };
};
S s;  // Previously an error.  Now okay.
