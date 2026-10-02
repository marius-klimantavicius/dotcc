//type:fp
//options::-A;fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  struct promise_type {
    void return_value();
    A yield_value(const A&);
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
  };
  void await_ready();
  void await_resume();
  void await_suspend(const coroutine_handle<A::promise_type>&);
};

A foo() {
  co_await A();
};
