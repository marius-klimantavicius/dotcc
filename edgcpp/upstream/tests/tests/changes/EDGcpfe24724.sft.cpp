//type:fp
//options_all:--c++20
//remark:[6.4] Use of [[no_unique_address]] fields with type aliases caused undefined behavior
// 2/2/22   [EDGcpfe/24724]
//
// Use of [[no_unique_address]] fields with type aliases caused undefined behavior
//
// A missing skip_typeref had caused undefined behavior (particularly in
// Cfront configurations) when using fields declared with type aliases and the
// [[no_unique_address]] attribute.
struct empty {};
using T1 = empty;
using T2 = empty;
struct A {
  [[no_unique_address]] T1 a;
  [[no_unique_address]] T2 b;
} x;
