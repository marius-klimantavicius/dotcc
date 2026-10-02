//type:fp
//options_all:--c++11
//remark:[6.5] Parsing of friend function noexcept specification in complete class context
// 5/24/23  [EDGcpfe/23420,EDGcpfe/26014,EDGcpfe/26353]
//
// Parsing of friend function noexcept specification in complete class context
//
// The C++ Standard requires the noexcept specification of a member declaration to
// be a complete class context.  Previously, the front end only treated the
// noexcept specification of non-friend declarations as such.  Now this also
// applies to friend declarations.
struct B {
  friend void f(B) noexcept(v);  // Previously a spurious 'identifier "v" is
                                 // undefined' error.  Now okay.
  static constexpr bool v = true;
};
