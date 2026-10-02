//type:fp
//remark:[4.12] Spurious error on singleton braced initializer for aggregate class object
// 6/10/16  [EDGcpfe/17295]
//
// Spurious error on singleton braced initializer for aggregate class object
//
// An aggregate class object can be initialized with a braced list containing a
// single value of that class type (or a related type); this is different from
// ordinary aggregate initialization in that the element in braces does not
// represent a proper subobject value but the whole object's value.  In some
// contexts, however, the front end did not correctly recognize that special
// case and issued a spurious error as a result.
//
// This is now fixed.
struct E {};
struct S {
  E e;
  S(E p): e{p} {}  // Previously triggered a spurious error
};                 // ("too many initializer values").  Now okay.
