//type:fp
//options_all:--c++20
//remark:[6.3] Internal error when a coroutine returns a non-class type
// 6/22/21  [EDGcpfe/23920]
//
// Internal error when a coroutine returns a non-class type
//
// The front end previously aborted with an internal error (in
// determine_dynamic_init_for_class_init) when the return type of a coroutine
// is not a class type.  This is now fixed.
#include <coroutine>
struct promise_type {
  promise_type(int);
  int get_return_object();
  auto initial_suspend() { return std::suspend_always{}; }
  auto final_suspend() noexcept { return std::suspend_always{}; }
  void unhandled_exception();
  void return_void() {}
  auto yield_value(int value) {
    return std::suspend_always{};
  }
};
template<typename R> struct std::coroutine_traits<R, int> {
  using promise_type = promise_type;
};
int f(int i) { co_return; }  // Previously caused an internal error
