//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Constexpr virtual functions
// 10/11/18 [EDGcpfe/20037,EDGcpfe/20124]
//
// C++20: Constexpr virtual functions
//
// In C++20 mode (and --ms_c++latest mode with microsoft_version >= 1920), the
// front end now implements support for constexpr virtual functions as per the
// standardization committee's paper P1064R0.
struct B {
  constexpr virtual int f() const { return 1; };  // Okay in C++20 mode.
};
struct D: B {
  constexpr virtual int f() const { return 2; };  // Okay in C++20 mode.
};
constexpr int g(B const &p) { return p.f(); }
static_assert(g(B{}) == 1);  // Okay in C++20 mode.
static_assert(g(D{}) == 2);  // Okay in C++20 mode.
