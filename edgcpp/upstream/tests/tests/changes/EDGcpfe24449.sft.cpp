//type:fp
//options_all:--c++11 --clang
//remark:[6.5] Clang compatibility: support _Atomic class types
// 3/14/23  [EDGcpfe/24449,EDGcpfe/25680]
//
// Clang compatibility: support _Atomic class types
//
// Previously, support for _Atomic class types was disabled in Clang mode as Clang
// may add additional padding and stricter alignment requirements to these types.
// The front end now emulates that behavior.  A new configuration macro,
// TARG_SIZEOF_LARGEST_ATOMIC, has been added to control what types are affected.
struct C {
  char c1, c2, c3;
};
using AC = _Atomic C;  // Previously not supported.  Now okay.
static_assert(sizeof(AC) == 4 && alignof(AC) == 4, "Unexpected");
