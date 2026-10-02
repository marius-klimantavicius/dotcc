//type:fp
//options_all:--c++20
//remark:[6.2] Assertion failed in mangled_encoding_for_function_type
// 8/24/20  [EDGcpfe/23268]
//
// Assertion failed in mangled_encoding_for_function_type
//
// In some configurations, an assertion failure could occur (in
// mangled_encoding_for_function_type) when a lambda appears in an initializer
// of a selection statement (see the Changes entry for EDGcpfe/17414).
consteval int f(int n) {
  switch(int x = [](){return 10;}(); n) {
    default:
      return x;
  }
}
int x = f(0);
