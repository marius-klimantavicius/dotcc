//type:fp
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>

struct A {
  struct promise_type {
    void return_void();
    A get_return_object();
    auto initial_suspend() { return std::suspend_always{}; }
    auto final_suspend() noexcept { return std::suspend_always{}; }
    void unhandled_exception();
  };
};

A f() {
  co_return;
}
