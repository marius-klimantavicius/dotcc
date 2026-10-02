//type:fp
//options_all:--ms_c++20 --microsoft_v 1940
//remark:[6.6] Spurious error with capture of init-capture in nested lambda constraint
// 7/21/23  [EDGcpfe/26503]
//
// Spurious error with capture of init-capture in nested lambda constraint
//
// This previously triggered a spurious error claiming the init-capture i could
// not be captured in the context of the requires-expression body.  That is now
// fixed (and the example is thus accepted).
auto f = [i = 1]() {
  auto g = [&i]<class F>(F f) requires requires (F f) { f(i); } {};
};
