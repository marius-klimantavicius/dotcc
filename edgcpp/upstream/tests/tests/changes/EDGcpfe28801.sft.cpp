//type:fp
//options_all:--c++20
//remark:Assertion failure on constexpr variable template with class temporary
// 4/27/26  [EDGcpfe/28801]
//
// Assertion failure on constexpr variable template with class temporary
//
// Previously, the front end aborted with an assertion failure in
// unbundle_init_component_expressions in overload.c, due to inconsistent
// handling of the initial parsing of the initializer for the instantiation of
// v<S> to determine its deduced type and the subsequent processing of that
// initializer to complete the representation of vs.  That is now fixed.
struct D {
  constexpr ~D() {}
};
struct S {
  D a = D{};
};
template<typename T> constexpr auto v = T{}.a;
D vs = v<S>;
