//type:fp
//options_all:--gn 140100
//remark:Template-dependent array designators
// 2/16/26  [EDGcpfe/28671]
//
// Template-dependent array designators
//
// In C++ modes that permit array designators (e.g., Clang and GNU C++ modes in
// particular), the front end now accepts such designators in some template-
// dependent contexts.
using Arr = float[10];
template<int N> auto f1() {
  Arr arr1 = { [N] = 42 };  // Now accepted in some C++ modes.
  return arr1[N];
}
