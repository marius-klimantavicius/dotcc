//type:fp
//options_all:--c++20
//remark:[6.3] Spurious error for promise constructor with coroutine reference parameter
// 6/22/21  [EDGcpfe/24457]
//
// Spurious error for promise constructor with coroutine reference parameter
//
// The front end previously reported a spurious error ("no default constructor
// exists for class") for the promise type of a coroutine with a reference
// parameter.  This is now fixed.
#include <coroutine>
struct A { };
struct promise_type {
  promise_type(int);
  A get_return_object();
  auto initial_suspend() { return std::suspend_always{}; }
  auto final_suspend() noexcept { return std::suspend_always{}; }
  void unhandled_exception();
  void return_void() {}
  auto yield_value(int value) {
    return std::suspend_always{};
  }
};
template<typename R> struct std::coroutine_traits<R, int&> {
  using promise_type = promise_type;
};
A f(int& i) { co_return; }  // Previously a spurious error
