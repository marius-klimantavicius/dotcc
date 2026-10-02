//type:fp
//options_all:--c++20
//remark:[6.5] __is_constructible for aggregate initialization in C++20 mode
// 4/11/23  [EDGcpfe/24656,EDGcpfe/24926,EDGcpfe/25916]
//
// __is_constructible for aggregate initialization in C++20 mode
//
// Previously, an aggregate with a member of non-aggregate class type that does
// not have a viable constructor would not cause the __is_constructible type
// trait helper to fail.
struct X {
  X(const char *);
};
struct C {
  X x;
};
static_assert(!__is_constructible(C, int));  // Previously failed.  Now okay.
