//type:fp
//options_all:--c++11
//remark:[5.0] C++-generating back end: assertion failure with decltype in template argument
// 3/20/18  [EDGcpfe/17642]
//
// C++-generating back end: assertion failure with decltype in template argument
//
// The C++-generating back end previously failed an assertion (in function
// get_param_for_param_ref) when a template instance for which a template
// argument refers to a function parameter via a decltype operator is referred
// to in a different context.  This is now fixed.
template <typename T> using type_t = T;
template <typename T> T fn1(T a, T b);
template <typename T, typename U>
auto fn2(T t, U u) -> type_t<decltype(t*u)>;
void fn3(int a) {
  fn2(a, a);                         // Instantiates type_t<int> as
                                     // type_t<decltype(t*u)>
  fn1<type_t<decltype(a+a)>>(a, a);  // Uses type_t<int> but t and u are
                                     // unavailable in this context, which
                                     // led to an assertion failure
}
