//type:fn
//remark:[4.2] Internal error on mem-initializer referring to anonymous union member
// 11/19/09 [EDGcpfe/10191]
//
// Internal error on mem-initializer referring to anonymous union member
//
// In configurations with EXPENSIVE_CHECKING set to TRUE, the front end aborted
// with an internal error in ctor_initializer when a mem-initializer refers to a
// member of a namespace-scope anonymous union.
//
// This regression was introduced in version 4.0 and is now fixed: An ordinary
// error is issued.
static union { int x, y; };
struct A {
  A() : x(y) {}  // Previously triggered an internal error.
};
