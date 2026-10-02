//type:fn
//options_all:--c++20 --set_flag coroutines -tused

#include <coroutine>
using namespace std;

struct Future {
  bool await_ready();
  void await_suspend(coroutine_handle<>);
  int await_resume();
};

Future f();

auto g() {
  co_await f();
}

auto h() {
  co_yield 1;
}

auto i() {
  co_return 1;
}
