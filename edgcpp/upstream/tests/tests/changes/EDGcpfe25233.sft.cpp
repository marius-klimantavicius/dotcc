//type:fp
//options_all:--c++11 --parse_templates
//remark:[6.4] C++-generating back end: assertion failure on pack expansion with member access
// 6/22/22  [EDGcpfe/25233]
//
// C++-generating back end: assertion failure on pack expansion with member access
//
// When the type of the object expression in a member access expression is a
// pack reference, the C++-generating back end could abort with an assertion
// failure in push_class_name_context.  This is now fixed.
// --c++11 --parse_templates:
struct S {
  template <typename... Ts> void f(Ts &&... ts) {
    g(static_cast<decltype(ts)&&>(ts).t...);  // Previously failed assertion
  }
  template<typename... T> void g(T...){}
};
