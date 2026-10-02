//type:fp
//options_all:--gnu_version=160100 --c++23
//remark:C++23: Relaxed ref-qualifier overloading rule (P1787R6)
// 6/25/26  [EDGcpfe/28870]
//
// C++23: Relaxed ref-qualifier overloading rule (P1787R6)
//
// Two non-static member functions with the same name and parameter types, where
// exactly one has a ref-qualifier, no longer always conflict in C++23.  This is
// a consequence of committee paper P1787R6.
//
// This behavior is extended to GNU C++20 modes with gnu_version >= 160000
// (to match GCC16's behavior).
struct S {
  S f(int) const;
  S f(int) &&;   // Now accepted in C++23 modes.
};
