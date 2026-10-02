//type:fn
//options_all:--c++17
//remark:[5.1] Segfault in attach_attributes_to_routine_instance
// 7/2/19   [EDGcpfe/21475]
//
// Segfault in attach_attributes_to_routine_instance
//
// An abort in attach_attributes_to_routine_instance could occur when trying to
// attach attributes to an ill-formed function template.
// (with --c++17):
template <class T> [[deprecated]] void f(T) noexcept (f<int>) {}
