//type:fp
//options_all:--c++11
//remark:[4.10.1] Spurious error on use of certain type traits helpers
// 1/13/15  [EDGcpfe/15903]
//
// Spurious error on use of certain type traits helpers
//
// In modes that may involve generated deleted special member functions, the front
// end sometimes emitted spurious errors when handling certain type traits helpers
// like __is_assignable or __is_constructible.
//
// This is now fixed.
extern "C" int printf(const char*,...);
template<typename> struct C { C(C&&); };
static_assert(!__is_trivially_assignable(C<int>, C<int>), "Unexpected");
  // Previously triggered an error in C++11 mode.
