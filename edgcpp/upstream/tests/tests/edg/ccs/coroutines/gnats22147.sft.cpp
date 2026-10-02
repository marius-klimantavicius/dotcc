//type:fp
//options:--c++20:--microsoft_version 1920 --c++17
//options_all:--set_flag coroutines

#include <coroutine>
using namespace std;

struct A {
  struct promise_type
  {
    A get_return_object() { return{}; }
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception() noexcept {}
    void return_void();

    A await_transform(A) { return {}; }
  };

  ~A();

  bool await_ready() const { return false; }
  void await_suspend(coroutine_handle<> handle) const {}
  A await_resume() const { return {}; }
};

A f()
{
  co_await A{};
}
