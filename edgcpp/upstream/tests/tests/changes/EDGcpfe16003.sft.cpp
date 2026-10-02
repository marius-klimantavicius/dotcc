//type:fp
//remark:[4.10.1] __is_convertible_to with private or ambiguous base classes
// 3/18/15  [EDGcpfe/16003,EDGcpfe/16093]
//
// __is_convertible_to with private or ambiguous base classes
//
// Previously, the front end's implementation of __is_convertible incorrectly
// produced a "true" value when one type is privately or ambiguously derived from
// the other.  The same problem occurred for pointers or references to such types.
//
// Another example:
//
// This is now fixed.
struct B {};
struct C: private B {};  // Private base class.
static_assert(!__is_convertible_to(C, B), "Unexpected!");
  // Previously this assertion check failed.  Now okay.
