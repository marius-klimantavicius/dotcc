//type:fp
//remark:[4.11] __is_constructible with array or reference types
// 8/27/15  [EDGcpfe/16459,EDGcpfe/16474]
//
// __is_constructible with array or reference types
//
// The front end did not always produce a correct result for the type traits
// helper __is_constructible when the destination type is an array type or a
// reference type.
//
// This is now fixed.
static_assert(__is_constructible(char[3]), "Unexpected!");
  // Previously an error because all array types produced "false".
  // Now accepted.
struct X {};
struct Y { Y(X&&); };
static_assert(!__is_constructible(Y&, X), "Unexpected!");
  // Previously an error because the front end ignored the fact that an
  // lvalue reference to non-const cannot be initialized with a temporary
  // (and therefore the __is_constructible invocation produced a true
  // value).  Now accepted.
