//type:fp
//options_all:--c++11
//remark:[4.10.1] Use of an unevaluated "this" in a non-capturing lambda
// 2/9/15   [EDGcpfe/15988]
//
// Use of an unevaluated "this" in a non-capturing lambda
//
// The front end previously required "this" to be captured for uses of "this" in
// an unevaluated context within a lambda.
//
// This is now fixed.
//
// Prior to the changes for EDGcpfe/15384,EDGcpfe/15386 (see entry of 9/4/14) the
// front end did (accidentally) accept cases like the above when the lambda
// appears in a nonstatic data member initializer.  In that sense this fixes a
// regression.
struct S {
  unsigned f() {
    return []{ return sizeof(i); }();  // Previously an error because
  }                                    // i requires access to "this", but
  int i;                               // "this" is not captured.
};
