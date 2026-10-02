//type:fp
//options::-DNEG;fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

#ifdef NEG
#define NOEXCEPT
#else
#define NOEXCEPT noexcept
#endif

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

struct A {
  struct promise_type {
    promise_type();
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() NOEXCEPT { return suspend_always{}; }
    void unhandled_exception();
    A get_return_object();
    void return_void();
  };
};

Future g();

A f() {
  co_await g();
}
