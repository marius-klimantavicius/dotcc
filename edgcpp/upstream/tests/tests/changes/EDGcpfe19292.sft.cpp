//type:fp
//options_all:-w -tused --microsoft_v 1900
//remark:[5.0] Abort in should_delay_lowering_on_function in some Microsoft modes
// 2/6/18   [EDGcpfe/19292]
//
// Abort in should_delay_lowering_on_function in some Microsoft modes
//
// The changes for EDGcpfe/18142 (in version 4.14) introduced a regression in
// Microsoft mode on certain member functions declared "constexpr" but found not
// to be constexpr.  This caused an assertion to fail in
// should_delay_lowering_on_function.
//
// This previously triggered an internal error.  That is now fixed.
int g();
template<typename T> struct S {
  typedef int(* const F)() ;
  static constexpr int f() {
    return g();
  }
  static constexpr F af[1]={ &f };
};
int r = S<int>::f();
