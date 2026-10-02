//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  struct promise_type {
    void return_value(int);
    void return_void();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
  };
};

A foo() {
  co_return;
}
