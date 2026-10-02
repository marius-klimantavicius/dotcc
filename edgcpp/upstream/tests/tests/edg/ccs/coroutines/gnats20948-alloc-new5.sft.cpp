//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

typedef decltype(sizeof(int)) size_t;

struct B;

struct A {
  struct promise_type {
    void* operator new(size_t, int, float);
    void return_void();
    A get_return_object();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
  };
};

A f(int, float) {
  co_return;
}

struct B {
  A g(A*) {
    co_return;
  }
};
