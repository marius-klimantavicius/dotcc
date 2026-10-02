//type:fp
//remark:[4.13] Abort on invocation of compute_has_nothrow_copy
// 11/30/16 [EDGcpfe/17804]
//
// Abort on invocation of compute_has_nothrow_copy
//
// Certain invocations of compute_has_nothrow_copy could result in a null pointer
// dereference.
//
// This is now fixed.
class S {};
struct A { int i; };
struct B: A { S s; };
struct C: B { C() {} };
static bool b = __has_nothrow_copy(C);  // Previously aborted.  Now okay.
