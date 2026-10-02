//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  struct promise_type {
    promise_type(int, float);
    promise_type() = delete;
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
    void return_void();
  };
};

// Promise constructor matches (int, float)
A f(int i, float f) {
  co_return;
}

struct foo {
  // Promise constructor is deleted
  A f(double d, char c) {
    co_return;
  }
};
