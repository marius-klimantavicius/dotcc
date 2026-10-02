//type:fp
//options_all:--c++20 -tused -w --gnu_version=120100
//remark:[6.8] IL write-read error on expressions involving __builtin_source_location
// 9/22/25  [EDGcpfe/27835]
//
// IL write-read error on expressions involving __builtin_source_location
//
// Some uses of __builtin_source_location and similar functions could lead to an
// abort while reading an IL file (an IL write-read error for an expression node).
// This bug manifested in configurations with IL_SHOULD_BE_WRITTEN_TO_FILE set
// to TRUE.
//
// This is now fixed.
template<typename> struct I;
template<typename F> void g(F) { using R = I<decltype(F()().f())>; }
struct L { L(char const*); };
struct result {
  void f(L = __builtin_FUNCTION());  // Previously triggered an
};                                   // IL write-read error.
int main() {
  g([] -> result {});
}
