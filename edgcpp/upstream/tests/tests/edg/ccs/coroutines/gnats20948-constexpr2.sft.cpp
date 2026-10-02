//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

struct A {
  struct promise_type {
    auto get_return_object() { return A{}; }
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    constexpr Future yield_value(A const&) { return {}; }
    void return_value(A const&);
  };
  ~A() {}
};

constexpr Future f() { return {}; }

constexpr A g() {
  co_await f();
  co_yield A();
  co_return A();
}
