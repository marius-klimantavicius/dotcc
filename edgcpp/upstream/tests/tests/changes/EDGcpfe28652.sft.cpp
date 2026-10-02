//type:fp
//options_all:--gn 160001 --c++14
//remark:GNU C++ compatibility: Explicit this in pre-C++23 modes
// 1/21/26  [EDGcpfe/28652]
//
// GNU C++ compatibility: Explicit "this" in pre-C++23 modes
//
// In GNU C++ modes with gnu_version >= 140000 the front end now accepts the C++23
// explicit "this" feature (see the entry for EDGcpfe/24781) with a warning in
// pre-C++23 modes.
struct S {
  int f(this S const& self) { return 0; }
};
