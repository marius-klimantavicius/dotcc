//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>

struct A {
  struct promise_type {
    void return_void();
  };
};

A f() {
  co_return;
}
