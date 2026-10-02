//type:fp
//options_all:--c++17 --gnu_version=70300
//remark:[6.1] Spurious failure to fold call to generated copy/move constructors
// 5/12/20  [EDGcpfe/21604,EDGcpfe/21715]
//
// Spurious failure to fold call to generated copy/move constructors
//
// Generated copy and move constructors were previously never marked as constexpr
// if their associated class has variant members.  That in turn resulted in
// spurious errors.
//
// That is now fixed.  The changes for EDGcpfe/20539 in version 5.1 interacted
// with this issue in a way that caused apparent regressions (although the
// underlying issue was pre-existing);
// in version 5.0 but triggered an error in 5.1.
template<typename> struct X {
  constexpr X(): i() { }
  constexpr X(float): i() { }
  constexpr X(bool, X): X(0 ? X() : float{}) {}
    // This delegating constructor was previously treated as non-constexpr
    // because it invokes the generated move constructor which was
    // erroneously marked as non-constexpr for moving a variant data member.
  union { int i; };
};
constexpr X<void> x(true, X<void>{});  // Previously an error.  Now okay.
