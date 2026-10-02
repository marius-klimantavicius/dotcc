//type:fn
//options_all:--c++20 -tused --set_flag coroutines -tused

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

template<class T>
T f(int i, float f) {
  co_return;
}

template<class T>
T g(double d) {
  co_return;
}

void foo() {
  f<A>(0, 1.0f);
  g<A>(1.0);
}
