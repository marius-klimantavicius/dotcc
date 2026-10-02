//type:fn
//remark:[4.6] Abort on malformed ctor-initializer when list-initialization is disabled
// 11/15/12 [EDGcpfe/13379]
//
// Abort on malformed ctor-initializer when list-initialization is disabled
//
// In C++ modes that do not permit list initialization syntax, a malformed
// ctor-initializer consisting of an identifier followed by a brace was likely
// to cause version 4.5 of the front end to abort with an internal error in
// scan_mem_initializer (decl_inits.c).
//
// This regression (introduced by the changes for EDGcpfe/9170; see entry of
struct A {};
struct B : A {
  B() : A {}  // Triggered an internal error in version 4.5.
};
