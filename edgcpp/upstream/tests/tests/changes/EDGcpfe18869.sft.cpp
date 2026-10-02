//type:fp
//options_all:--c++17
//remark:[5.0] Abort on use of [[nodiscard]] function in template-dependent context
// 11/2/17  [EDGcpfe/18869]
//
// Abort on use of [[nodiscard]] function in template-dependent context
//
// In C++17 mode, the front end sometimes aborted (while evaluating the function
// check_expression_for_nodiscard_warning) when processing a function call
// involving a function marked with the [[nodiscard]] attribute in a template-
// dependent context.
//
// That problem is now fixed.
void g(char, ...);
struct S {
  [[nodiscard]] char f();
};
template<typename T> void h(T p) {
  S s;
  g(s.f(), p);  // Previously triggered an abort.
}
