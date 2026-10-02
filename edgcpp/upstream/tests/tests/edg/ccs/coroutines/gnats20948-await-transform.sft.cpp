//type:fp
//options::-DNEG;fn:-DNEG2;fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

struct A;

struct B {
  struct promise_type {
    promise_type();
    ~promise_type();
    auto initial_suspend() { return suspend_always{}; }
    auto final_suspend() noexcept { return suspend_always{}; }
    void unhandled_exception();
    B get_return_object();
    void return_void();
#ifndef NEG2
    Future await_transform(int);
    Future await_transform(const A&);
#endif /* NEG2 */
  };
};

int g();

B f(A&& a) {
  co_await g();
  co_await a;
#ifdef NEG
  co_await B();
#endif /* NEG */
}
