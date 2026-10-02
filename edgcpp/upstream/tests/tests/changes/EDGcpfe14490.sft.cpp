//type:fp
//remark:[4.8] User-declared operators and enum operands
// 9/13/13  [EDGcpfe/14490]
//
// GNU C++ compatibility: "best worst conversion" in overload resolution
//
// In GNU C++ mode with gnu_version < 40000 the front end sometimes prefers a
// builtin operator candidate over a user-declared candidate even though the
// former is not an unambiguously better match than the latter.  Specifically,
// the builtin candidate is selected if its worst argument conversion is better
// than the worst argument conversion needed for the user-declared candidate
// (see the entries of 4/5/05 and 7/20/04).  Now this behavior is also enabled
// when gnu_version >= 40400.  This causes the front end to accept the following
// case:
//
// Note that this is the same example as the following entry (below, also for
// EDGcpfe/14490), but the mechanism that makes it acceptable in some GNU modes
// is different.
//
// 9/13/13  [EDGcpfe/14490]
//
// User-declared operators and enum operands
//
// The standard specifies that nonmember user-declared operators without a
// parameter of enumeration type (or a reference thereto) are not considered for
// a use of the corresponding operator in which none of the operands has a class
// type.
//
// Previously, the front end issued an ambiguity error because it considered the
// user-declared operator==, which is a better match for the first operand since
// the built-in operand requires promotion of the char value.  Now, however, the
// user-declared operator== is dropped from consideration because it has no
// parameter of enum (or reference to enum) type and none of the operands has
// class type.
//
// This change does not apply in Microsoft C++ mode, nor in GNU C++ mode when
// gnu_version < 40800 (although the corresponding compilers accept the case
// above, it is for different reasons, as they appear not to apply the standard
// rule in general).
struct S { S(char); };
bool operator==(char, S const&);
enum E { e };
bool g(char c) {
  return c == e;  // Previously ambiguous; now okay.
}
