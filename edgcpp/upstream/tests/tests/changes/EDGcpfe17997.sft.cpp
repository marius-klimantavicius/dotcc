//type:fp
//options_all:--c++17
//remark:[5.0] C++17: Elision of temporary class object copies
// 9/14/17  [EDGcpfe/17997]
//
// C++17: Elision of temporary class object copies
//
// The forthcoming C++17 standard reformulates the handling of temporaries such
// that the copying of class-type temporaries must be elided in many contexts.
// This eliding, which was previously permitted but not required, was already
// performed by the front end, but the elided copy constructor was still checked
// (e.g., to ensure that it is accessible).  Now those checks are no longer
// performed in C++17 mode.
//
// This example still elicits warnings or errors in pre-C++17 modes.
//
// The changes for this C++17 feature also affect some pre-C++17 cases.  For
// example:
//
// Previously this produced an error in all C++11 modes.  Now, the diagnostic is
// reduced to just a warning in nonstrict C++11 modes.
struct S {
  S();
  S(S const&) = delete;
};
S f() { return S(); }  // Now accepted in C++17 mode.
S s = f();             // Now accepted in C++17 mode.
