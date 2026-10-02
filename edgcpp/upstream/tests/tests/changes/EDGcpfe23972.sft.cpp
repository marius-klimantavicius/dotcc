//type:fp
//options_all:--gn 102000 --c++20 -w
//remark:[6.3] Internal errors when substituting certain requires-expression
// 4/23/21  [EDGcpfe/23972,EDGcpfe/23973,EDGcpfe/24178]
//
// Internal errors when substituting certain requires-expression
//
// The front end previously aborted with an internal error (usually indicating an
// unexpected template-dependent construct) when substituting certain
// requires-expressions.
//
// That is now fixed.
template<typename T> struct X { X(T); };
template<typename T> T&& f(T);
template<typename T> constexpr void g(T t) {
  requires { X{ f<T>(t) }; };
}
struct S {};
int main() {
  g(S{});  // Previously triggered an abort.  Now okay.
}
