//type:fp
//options_all:--gn 100999 -tused
//remark:[6.4] C++-generating back end: Abort on nondependent cast in template
// 10/13/22 [EDGcpfe/25694]
//
// C++-generating back end: Abort on nondependent cast in template
//
// Although the cast is nondependent, in GNU C++ mode it is treated as a "generic
// cast" (i.e., without performing complete semantic analysis) because it appears
// in a template and doing so improves GCC compatibility.  However, the C++-
// generating back end performs some basic value category checks that failed in
// this case (with an internal error) because the expression node for p in
// "long(p)" was left to be an lvalue.  Now it is forced to a prvalue to avoid
// that issue.
template<typename> struct S { operator long() const; };
template<typename> void g(S<int> p) {
  (void)long(p);
}
