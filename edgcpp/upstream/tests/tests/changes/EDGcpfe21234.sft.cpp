//type:fp
//options_all:--c++17 -A
//remark:[5.1] Spurious error on singleton braced initializer for aggregate class object in a
// 5/23/19  [EDGcpfe/21234]
//
// Spurious error on singleton braced initializer for aggregate class object in a
// template dependent context
//
// The previous fix for EDGcpfe/17295 did not handle the case where the braced
// initializer list contained a single value that was dependent on template
// arguments.
//
// This is now fixed.
struct A {};
template <typename T> void foo(T t)
{
  A a{t};  // Spurious "too many initializer values" error
}
void f()
{
  foo(A{});
}
