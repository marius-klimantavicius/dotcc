//type:fp
//options_all:--g++
//remark:[4.12] GNU C++ compatibility: Local __thread variables
// 6/14/16  [EDGcpfe/17307]
//
// GNU C++ compatibility: Local __thread variables
//
// In GNU C++ modes with gnu_version >= 40800, the front end now accepts local
// static __thread variables requiring dynamic initialization.
int f();
int g() {
  static __thread int r = g();  // Now accepted in some GNU C++ modes.
  return r;
}
