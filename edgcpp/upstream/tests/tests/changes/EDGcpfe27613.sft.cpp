//type:fp
//options_all:--ms_c++20 --microsoft_v 1940
//remark:[6.7] Microsoft C++ compatibility: Casts in templates
// 10/4/24  [EDGcpfe/27613]
//
// Microsoft C++ compatibility: Casts in templates
//
// The front end now accepts some invalid casts in Microsoft-mode templates
// (including in non-permissive Microsoft modes).
struct X {};
struct Y {};
template<typename T> X* f(Y *p) {
  return static_cast<X*>(p);  // Now accepted in Microsoft modes.
}
