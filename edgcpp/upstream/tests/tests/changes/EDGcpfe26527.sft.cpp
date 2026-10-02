//type:fp
//options_all:--gn 110300 --c++17
//remark:[6.6] Non-literal parameter and return types for constexpr functions
// 7/25/23  [EDGcpfe/26527]
//
// Non-literal parameter and return types for constexpr functions
//
// C++23 no longer requires a function declared "constexpr" to have literal-type
// parameters and a literal return type (this is one consequence of paper
// P2448R2).  The front end therefore no longer diagnoses such cases in C++23
// mode.  Furthermore, GCC never diagnosed non-literal parameter and return types
// for constexpr lambda call operators, and the front end now emulates that.
struct N { N() {}; };
void g() {
  N r = []() constexpr -> N {  // Now accepted in C++23 modes and in
    return N{};                // GNU C++11 modes.
  }();
}
