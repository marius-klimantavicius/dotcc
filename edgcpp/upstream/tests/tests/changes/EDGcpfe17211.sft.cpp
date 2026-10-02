//type:fp
//remark:[4.13] Invalid casts in template-dependent constant-expression contexts
// 11/15/16  [EDGcpfe/17211]
//
// Invalid casts in template-dependent constant-expression contexts
//
// In GNU, Clang, and Microsoft modes, the front end now accepts certain invalid
// casts in template-dependent constant-expression contexts.
//
// Previously, this example triggered an error because reinterpret_cast
// constructs are not permitted in constant-expression contexts.  Now, however,
// it is accepted in some modes.  (Instantiating the template may also be
// accepted because this particular pattern is used for certain implementations
// of the "offsetof" macro, even though it is nonstandard.)
template <class T> struct B { int s; };
template <class T> struct D: B<T> {
  static_assert(
    (unsigned long)&reinterpret_cast<char&>(((B<T> *)0)->s) == 0, "");
};
