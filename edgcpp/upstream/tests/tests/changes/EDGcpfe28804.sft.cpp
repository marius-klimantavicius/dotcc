//type:fp
//options_all:--c++20
//remark:if consteval in C++20 modes
// 4/27/26  [EDGcpfe/28804]
//
// if consteval in C++20 modes
//
// The front end now accepts "if consteval" and "if not consteval" in C++20 mode
// with a warning, matching the behavior of other compilers.  In strict ANSI mode,
// the diagnostic uses the configured strict ANSI error severity instead.  For
// example, with --c++20:
consteval int f(int i) { return i; }
constexpr int g(int i) {
  if consteval {
    return f(i);
  }
  return 0;
}
