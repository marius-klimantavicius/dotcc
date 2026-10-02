//type:fp
//remark:[6.0] C++-generating back end: Abort on malformed constructor template definition
// 9/6/19   [EDGcpfe/21685]
//
// C++-generating back end: Abort on malformed constructor template definition
//
// The C++-generating back end previously triggered an internal error when
// attempting to render a malformed constructor template that the front end failed
// to diagnose.
//
// That is now fixed.
struct S {
  int i;
  template<typename T> S(T i) : i(i);  // Previously aborted.  Now fixed. An
};                                     // error is issued in modes where the
                                       // definition is parsed.
