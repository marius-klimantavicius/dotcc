//type:fp
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct A {
  struct promise_type {
    promise_type(int, float);
    promise_type();
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
    void return_void();
  };
};

struct B {
  ~B();
};

A f(int i, float f) {
  B foo;
  co_return;
}
