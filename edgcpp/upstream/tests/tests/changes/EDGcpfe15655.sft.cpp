//type:fp
//options_all:--c++11
//remark:[4.10] Incorrect handling of reference-returning constexpr functions
// 12/2/14  [EDGcpfe/15655]
//
// Incorrect handling of reference-returning constexpr functions
//
// The front end previously incorrectly handled some constexpr function
// invocations when the function returns a reference.  In some cases, the
// front end simply did not fold the call to a constant; in others, the front
// end aborted with a failed assertion ("get_pointer_offset: bad kind").  This
// is now fixed.
struct S {
  constexpr S(int i): t(i) { }
  int t;
};
constexpr S&& fwd(S& __t) { return static_cast<S&&>(__t); }
constexpr static int&& val (S&& arg) {
  return fwd(arg).t;
}
constexpr int xxx(S&& arg) {
  return val(fwd(arg));
}
static_assert(xxx(S(1)) == 1, "");   // Previously reported "function
                                     // call must have constant value"
static_assert(val(S(1)) == 1, "");   // Previously aborted
