//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>

struct A {
  struct promise_type {
    void return_void();
    void get_return_object();
    void initial_suspend();
    void final_suspend() noexcept;
    void unhandled_exception();
  };
};

A f() {
  co_return;
}
