//type:fp
//options_all:--c++20 --set_flag=coroutines

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

    A&& await_transform(A&& expression) { return (A&&)expression; }
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
