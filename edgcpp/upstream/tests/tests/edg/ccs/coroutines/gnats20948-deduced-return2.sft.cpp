//type:fn
//options::--microsoft_version 1900:--microsoft_version 1920
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
  co_return 1;
}
