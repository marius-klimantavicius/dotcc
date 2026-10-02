//type:fp
//options_all:--clang
//remark:[4.11] GNU/Clang compatibility: Trivial designators into non-POD class types
// 8/10/15  [EDGcpfe/15514,EDGcpfe/16406]
//
// GNU/Clang compatibility: Trivial designators into non-POD class types
//
// In GNU and Clang modes, the front end now accepts "trivial designators" for
// fields in non-POD class types, where a "trivial designator" designates the
// field that would be initialized if the designator were omitted.
struct N { N(); };
struct S {
  N n;
  int i;
} s = { .n = N(), 42 };  // Now accepted in GNU and Clang modes.
