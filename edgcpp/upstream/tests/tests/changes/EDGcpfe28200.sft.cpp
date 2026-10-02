//type:fp
//options_all:--gn 140100 --c++17 -w
//remark:[6.8] Spurious diagnostic with deleted move constructor
// 8/29/25  [EDGcpfe/28200]
//
// Spurious diagnostic with deleted move constructor
//
// The front end previously issued an error claiming invocation of the deleted
// move constructor of V, even though that invocation is completely avoided in
// C++17 (through so-called "mandatory copy elision").  This is now fixed.
struct V {
  constexpr V() noexcept = default;
  constexpr V(const V&) noexcept = default;
  V(V&&) = delete;
  constexpr V(char const (&)[42]) noexcept {}
};
constexpr V makeV() { return V{}; }
constexpr V k{makeV()};  // Previously an error.  Now okay.
