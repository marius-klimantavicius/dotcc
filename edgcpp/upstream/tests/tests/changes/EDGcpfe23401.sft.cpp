//type:fp
//options_all:--c++17
//remark:[6.2] Deleted virtual overrider and exception specification mismatch
// 9/22/20  [EDGcpfe/23401]
//
// Deleted virtual overrider and exception specification mismatch
//
// In C++17 mode, the front end now permits a deleted virtual function overrider
// to have a less-strict exception specification than the (deleted) virtual
// function it overrides.
struct B { virtual void h() noexcept = delete; };
struct D : B {
  void h() = delete;  // Now accepted in C++17 mode.
};
