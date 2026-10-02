//type:fn
//options_all:--microsoft_version 1900 --set_flag coroutines -tused

#include <coroutine>
using namespace std;
 
struct S {
  S(const S&) {}
  bool await_ready() { return false; }
  void await_suspend(coroutine_handle<>) {}
  void await_resume() {}
};

S f() noexcept;
auto bar() {
  decltype(co_await f());
}
