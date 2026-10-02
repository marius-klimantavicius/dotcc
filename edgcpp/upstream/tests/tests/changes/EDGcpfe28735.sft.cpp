//type:fp
//options_all:--c++11
//remark:Internal error when processing large types
// 3/9/26   [EDGcpfe/28735]
//
// Internal error when processing large types
//
// Previously, an internal error in lower_constant (lower_il.c) could occur
// when processing types whose size exceeded the front end's constexpr
// type size limit.
//
// The error occurred due to incorrect handling of certain failure modes while
// constant folding.  This issue has now been fixed.
struct arr { unsigned char x[1ull << 31]; };

int sink(arr) { return 0; };

int main() {
  arr x{};
  int y = sink(x);
}
