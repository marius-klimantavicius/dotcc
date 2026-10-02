//type:fp
//options_all:--c++11
//remark:[6.8] Spurious substitution failure for aggregate initialization from a pack
// 10/13/25 [EDGcpfe/24634,EDGcpfe/27214,EDGcpfe/27836,EDGcpfe/27902]
//
// Spurious substitution failure for aggregate initialization from a pack
//
// Previously, when a template argument list was explicitly specified for a pack
// of a function template, initial substitution could fail if that pack was used
// as the initializer of an aggregate.
struct A {
  int i;
};
template<typename ... Ts>
decltype(A{Ts{} ...}) f();
auto v = f<int>();  // Previously a spurious error.  Now okay.
